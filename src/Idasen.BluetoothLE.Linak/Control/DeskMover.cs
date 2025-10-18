using System.Reactive.Concurrency ;
using System.Reactive.Disposables ;
using System.Reactive.Linq ;
using System.Reactive.Subjects ;
using System.Runtime.CompilerServices ;
using Autofac.Extras.DynamicProxy ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Core ;
using Idasen.BluetoothLE.Linak.Interfaces ;
using Serilog ;

[ assembly : InternalsVisibleTo ( "Idasen.BluetoothLE.Linak.Tests" ) ]

namespace Idasen.BluetoothLE.Linak.Control ;

[ Intercept ( typeof ( LogAspect ) ) ]
public class DeskMover
    : IDeskMover
{
    public delegate IDeskMover Factory ( IDeskCommandExecutor executor ,
                                         IDeskHeightAndSpeed  heightAndSpeed ) ;

    private readonly IStoppingHeightCalculator _calculator ;

    private readonly IDeskMoveEngine      _engine ;
    private readonly IDeskCommandExecutor _executor ;
    private readonly IDeskMoveGuard       _guard ;
    private readonly IDeskHeightAndSpeed  _heightAndSpeed ;

    private readonly ILogger                     _logger ;
    private readonly IDeskMovementMonitorFactory _monitorFactory ;

    private readonly object                                _padlock = new( ) ;
    private readonly IInitialHeightAndSpeedProviderFactory _providerFactory ;
    private readonly IScheduler                            _scheduler ;
    private readonly ISubject < uint >                     _subjectFinished ;
    private          CompositeDisposable ?                 _cycleDisposables ;

    private          IDisposable ? _disposableProvider ;
    private          IDisposable ? _guardTargetHeightReached ;

    private IInitialHeightProvider ? _initialProvider ;

    private bool                   _isAllowedToMove ;
    private IDeskMovementMonitor ? _monitor ;

    private IDisposable ? _rawHeightAndSpeedSubscription ;

    public DeskMover ( ILogger                               logger ,
                       IScheduler                            scheduler ,
                       IInitialHeightAndSpeedProviderFactory providerFactory ,
                       IDeskMovementMonitorFactory           monitorFactory ,
                       IDeskCommandExecutor                  executor ,
                       IDeskHeightAndSpeed                   heightAndSpeed ,
                       IStoppingHeightCalculator             calculator ,
                       ISubject < uint >                     subjectFinished )
        : this ( logger ,
                 scheduler ,
                 subjectFinished ,
                 new DeskLocationHandlers ( heightAndSpeed ,
                                            providerFactory ) ,
                 new DeskMovementHandlers ( logger ,
                                            heightAndSpeed ,
                                            monitorFactory ,
                                            executor ,
                                            calculator ) )
    {
    }

    internal DeskMover ( ILogger               logger ,
                         IScheduler            scheduler ,
                         ISubject < uint >     subjectFinished ,
                         IDeskLocationHandlers locationHandlers ,
                         IDeskMovementHandlers movementHandlers )
    {
        Guard.ArgumentNotNull ( logger ,
                                nameof ( logger ) ) ;
        Guard.ArgumentNotNull ( scheduler ,
                                nameof ( scheduler ) ) ;
        Guard.ArgumentNotNull ( subjectFinished ,
                                nameof ( subjectFinished ) ) ;
        Guard.ArgumentNotNull ( locationHandlers ,
                                nameof ( locationHandlers ) ) ;
        Guard.ArgumentNotNull ( movementHandlers ,
                                nameof ( movementHandlers ) ) ;

        _logger          = logger ;
        _scheduler       = scheduler ;
        _subjectFinished = subjectFinished ;
        _providerFactory = locationHandlers.ProviderFactory ;
        _heightAndSpeed  = locationHandlers.HeightAndSpeed ;
        _monitorFactory  = movementHandlers.MonitorFactory ;
        _executor        = movementHandlers.CommandExecutor ;
        _calculator      = movementHandlers.Calculator ;
        _engine          = movementHandlers.MoveEngine ;
        _guard           = movementHandlers.MoveGuard ;
    }

    /// <summary>
    ///     Direction used for the first movement step once height is known.
    /// </summary>
    public Direction StartMovingIntoDirection { get ; private set ; }

    /// <inheritdoc />
    public uint Height { get ; private set ; }

    /// <inheritdoc />
    public int Speed { get ; private set ; }

    /// <inheritdoc />
    public uint TargetHeight { get ; set ; }

    /// <inheritdoc />
    public IObservable < uint > Finished => _subjectFinished ;

    /// <inheritdoc />
    public void Initialize ( )
    {
        lock ( _padlock )
        {
            _monitor?.Dispose ( ) ;
            _monitor = _monitorFactory.Create ( _heightAndSpeed ) ;
            _monitor.Initialize ( ) ;

            _initialProvider?.Dispose ( ) ;
            _initialProvider = _providerFactory.Create ( _executor ,
                                                         _heightAndSpeed ) ;
            if ( _initialProvider != null )
            {
                _initialProvider.Initialize ( ) ;

                _disposableProvider?.Dispose ( ) ;
                _disposableProvider = _initialProvider.Finished
                                                      .ObserveOn ( _scheduler )
                                                      .SubscribeAsync ( OnFinished ) ;
            }

            // Global raw subscription to reflect latest Height/Speed for observers/tests
            _rawHeightAndSpeedSubscription?.Dispose ( ) ;
            _rawHeightAndSpeedSubscription = _heightAndSpeed.HeightAndSpeedChanged
                                                            .Subscribe ( d =>
                                                                         {
                                                                             Height = d.Height ;
                                                                             Speed  = d.Speed ;
                                                                         } ) ;

            var targetReached = _guard.TargetHeightReached ;

            _guardTargetHeightReached = targetReached
                                              .ObserveOn ( _scheduler )
                                              .Subscribe ( targetHeight =>
                                                           {
                                                               _logger.Information ( "Reached target height={TargetHeight}" ,
                                                                                     targetHeight ) ;
                                                               _engine.StopMoveAsync ( ) ;
                                                               StopMovement ( ) ;
                                                           } ) ;
        }
    }

    /// <inheritdoc />
    public void Start ( )
    {
        _logger.Debug ( "Starting movement cycle (target={TargetHeight})" ,
                        TargetHeight ) ;

        _cycleDisposables?.Dispose ( ) ;
        _cycleDisposables = null ;

        if ( _initialProvider == null )
            _logger.Error ( $"{nameof ( _initialProvider )} is null" ) ;

        _initialProvider?.Start ( ) ;
    }

    /// <inheritdoc />
    public async Task < bool > Up ( )
    {
        if ( IsAllowedToMove )
        {
            _logger.Debug ( "Manual Up() requested (height={Height} target={Target})" ,
                            Height ,
                            TargetHeight ) ;
            return await _executor.Up ( ).ConfigureAwait ( false ) ;
        }

        return false ;
    }

    /// <inheritdoc />
    public async Task < bool > Down ( )
    {
        if ( IsAllowedToMove )
        {
            _logger.Debug ( "Manual Down() requested (height={Height} target={Target})" ,
                            Height ,
                            TargetHeight ) ;
            return await _executor.Down ( ).ConfigureAwait ( false ) ;
        }

        return false ;
    }

    /// <inheritdoc />
    public Task < bool > StopMovement ( )
    {
        _logger.Debug ( "Stopping... (height={Height} speed={Speed} target={Target})" ,
                        Height ,
                        Speed ,
                        TargetHeight ) ;

        // If we are already stopped, avoid duplicate work and events
        if ( ! IsAllowedToMove &&
             _cycleDisposables == null )
        {
            _logger.Debug ( "Already stopped (suppress duplicate)" ) ;
            return Task.FromResult ( true ) ;
        }

        IsAllowedToMove               = false ;
        _calculator.MoveIntoDirection = Direction.None ;

        _cycleDisposables?.Dispose ( ) ;
        _cycleDisposables = null ;

        _guard.StopGuarding ( ) ;
        _engine.StopMoveAsync ( ) ;

        _logger.Debug ( "Emitting finished (height={Height})" ,
                        Height ) ;

        _subjectFinished.OnNext ( Height ) ;

        return Task.FromResult ( true ) ;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _monitor?.Dispose();
            _disposableProvider?.Dispose();
            _rawHeightAndSpeedSubscription?.Dispose();
            _cycleDisposables?.Dispose();
            _guardTargetHeightReached?.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);

        GC.SuppressFinalize(this);
    }

    ~DeskMover()
    {
        Dispose(false);
    }

    /// <inheritdoc />
    public bool IsAllowedToMove
    {
        get => Volatile.Read ( ref _isAllowedToMove ) ;
        private set =>
            Volatile.Write ( ref _isAllowedToMove ,
                             value ) ;
    }

    private async Task StartAfterReceivingCurrentHeight ( uint height ,
                                                          int  speed )
    {
        _logger.Debug ( "Initial height received -> start control loop (height={Height} target={Target})" ,
                        _heightAndSpeed.Height ,
                        TargetHeight ) ;

        if ( TargetHeight == 0 )
        {
            _logger.Warning ( "TargetHeight is 0 (control loop may no-op)" ) ;
            return ;
        }

        // Compute initial start direction once for this cycle
        _calculator.Height                   = height ;
        _calculator.Speed                    = speed ;
        _calculator.TargetHeight             = TargetHeight ;
        _calculator.StartMovingIntoDirection = Direction.None ;
        _calculator.Calculate ( ) ;
        StartMovingIntoDirection = _calculator.MoveIntoDirection ;

        _logger.Debug ( "Calculated initial direction={Direction}" ,
                        StartMovingIntoDirection ) ;

        // Now allow movement
        _guard.StartGuarding ( _calculator.MoveIntoDirection ,
                               TargetHeight ,
                               CancellationToken.None ) ;

        await _engine.StartMoveAsync ( _calculator.MoveIntoDirection ,
                                       CancellationToken.None ) ;
    }

    private async Task OnFinished ( uint height )
    {
        // Do not override Height/Speed here; rely on raw subscription to reflect latest values.
        await StartAfterReceivingCurrentHeight ( height ,
                                                 0 ) ;
    }
}
