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
                                         IDeskHeightAndSpeed heightAndSpeed ) ;

    private readonly IStoppingHeightCalculator _calculator ;

    private readonly IDeskMoveEngine _engine ;
    private readonly IDeskCommandExecutor _executor ;
    private readonly IDeskHeightAndSpeed _heightAndSpeed ;

    private readonly ILogger _logger ;
    private readonly IDeskMovementMonitorFactory _monitorFactory ;

    private readonly SemaphoreSlim _moveSemaphore = new ( 1 ,
                                                          1 ) ;

    private readonly object _padlock = new ( ) ;
    private readonly IInitialHeightAndSpeedProviderFactory _providerFactory ;
    private readonly IScheduler _scheduler ;
    private readonly DeskMoverSettings _settings ;
    private readonly IDeskStopper _stopper ;
    private readonly ISubject < uint > _subjectFinished ;
    private CompositeDisposable? _cycleDisposables ;

    private IDisposable? _disposableProvider ;
    private volatile bool _finishedEmitted ;

    private IObservable < HeightSpeedDetails >? _heightAndSpeedSampled ;
    private IInitialHeightProvider? _initialProvider ;

    private bool _isAllowedToMove ;
    private IDeskMovementMonitor? _monitor ;

    private Task < bool >? _pendingStopTask ;
    private IDisposable? _rawHeightAndSpeedSubscription ;

    public DeskMover ( ILogger logger ,
                       IScheduler scheduler ,
                       IInitialHeightAndSpeedProviderFactory providerFactory ,
                       IDeskMovementMonitorFactory monitorFactory ,
                       IDeskCommandExecutor executor ,
                       IDeskHeightAndSpeed heightAndSpeed ,
                       IStoppingHeightCalculator calculator ,
                       ISubject < uint > subjectFinished ,
                       IDeskHeightMonitor heightMonitor )
        : this ( logger ,
                 scheduler ,
                 providerFactory ,
                 monitorFactory ,
                 executor ,
                 heightAndSpeed ,
                 calculator ,
                 subjectFinished ,
                 heightMonitor ,
                 DeskMoverSettings.Default ,
                 new DeskMoveEngine ( logger ,
                                      executor ,
                                      DeskMoverSettings.Default ) ,
                 new DeskStopper ( logger ,
                                   DeskMoverSettings.Default ,
                                   heightMonitor ,
                                   calculator ) )
    {
    }

    internal DeskMover ( ILogger logger ,
                         IScheduler scheduler ,
                         IInitialHeightAndSpeedProviderFactory providerFactory ,
                         IDeskMovementMonitorFactory monitorFactory ,
                         IDeskCommandExecutor executor ,
                         IDeskHeightAndSpeed heightAndSpeed ,
                         IStoppingHeightCalculator calculator ,
                         ISubject < uint > subjectFinished ,
                         IDeskHeightMonitor heightMonitor ,
                         DeskMoverSettings settings ,
                         IDeskMoveEngine engine ,
                         IDeskStopper stopper )
    {
        Guard.ArgumentNotNull ( logger ,
                                nameof ( logger ) ) ;
        Guard.ArgumentNotNull ( scheduler ,
                                nameof ( scheduler ) ) ;
        Guard.ArgumentNotNull ( providerFactory ,
                                nameof ( providerFactory ) ) ;
        Guard.ArgumentNotNull ( monitorFactory ,
                                nameof ( monitorFactory ) ) ;
        Guard.ArgumentNotNull ( executor ,
                                nameof ( executor ) ) ;
        Guard.ArgumentNotNull ( heightAndSpeed ,
                                nameof ( heightAndSpeed ) ) ;
        Guard.ArgumentNotNull ( calculator ,
                                nameof ( calculator ) ) ;
        Guard.ArgumentNotNull ( subjectFinished ,
                                nameof ( subjectFinished ) ) ;
        Guard.ArgumentNotNull ( heightMonitor ,
                                nameof ( heightMonitor ) ) ;
        Guard.ArgumentNotNull ( settings ,
                                nameof ( settings ) ) ;
        Guard.ArgumentNotNull ( engine ,
                                nameof ( engine ) ) ;
        Guard.ArgumentNotNull ( stopper ,
                                nameof ( stopper ) ) ;

        _logger = logger ;
        _scheduler = scheduler ;
        _providerFactory = providerFactory ;
        _monitorFactory = monitorFactory ;
        _executor = executor ;
        _heightAndSpeed = heightAndSpeed ;
        _calculator = calculator ;
        _subjectFinished = subjectFinished ;
        _settings = settings ;

        _engine = engine ;
        _stopper = stopper ;
    }

    public TimeSpan TimerInterval => _settings.TimerInterval ;

    /// <summary>
    ///     Direction used for the first movement step once height is known.
    /// </summary>
    public Direction StartMovingIntoDirection { get ; private set ; }

    public IObservable < HeightSpeedDetails > HeightAndSpeedSampled =>
        _heightAndSpeedSampled ??= _heightAndSpeed.HeightAndSpeedChanged
                                                  .Buffer ( TimerInterval ,
                                                            _scheduler )
                                                  .Where ( batch => batch.Count > 0 )
                                                  .Select ( batch => batch[^1] )
                                                  .StartWith ( new HeightSpeedDetails ( DateTimeOffset.Now ,
                                                                                        Height ,
                                                                                        Speed ) )
                                                  .Replay ( 1 )
                                                  .RefCount ( ) ;

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
        lock (_padlock)
        {
            _monitor?.Dispose ( ) ;
            _monitor = _monitorFactory.Create ( _heightAndSpeed ) ;
            _monitor.Initialize ( ) ;

            _initialProvider?.Dispose ( ) ;
            _initialProvider = _providerFactory.Create ( _executor ,
                                                         _heightAndSpeed ) ;
            _initialProvider.Initialize ( ) ;

            _disposableProvider?.Dispose ( ) ;
            _disposableProvider = _initialProvider.Finished
                                                  .ObserveOn ( _scheduler )
                                                  .Subscribe ( OnFinished ) ;

            // Global raw subscription to reflect latest Height/Speed for observers/tests
            _rawHeightAndSpeedSubscription?.Dispose ( ) ;
            _rawHeightAndSpeedSubscription = _heightAndSpeed.HeightAndSpeedChanged
                                                            .ObserveOn ( _scheduler )
                                                            .Subscribe ( d =>
                                                                         {
                                                                             Height = d.Height ;
                                                                             Speed = d.Speed ;
                                                                         } ) ;
        }
    }

    /// <inheritdoc />
    public void Start ( )
    {
        _logger.Debug ( "Starting movement cycle (target={TargetHeight})" , TargetHeight ) ;

        _cycleDisposables?.Dispose ( ) ;
        _cycleDisposables = null ;

        if ( _initialProvider == null )
        {
            _logger.Error ( $"{nameof ( _initialProvider )} is null" ) ;
        }

        _initialProvider?.Start ( ) ;
    }

    /// <inheritdoc />
    public async Task < bool > Up ( )
    {
        if ( IsAllowedToMove )
        {
            _logger.Debug ( "Manual Up() requested (height={Height} target={Target})" , Height , TargetHeight ) ;
            return await _executor.Up ( ).ConfigureAwait ( false ) ;
        }

        return false ;
    }

    /// <inheritdoc />
    public async Task < bool > Down ( )
    {
        if ( IsAllowedToMove )
        {
            _logger.Debug ( "Manual Down() requested (height={Height} target={Target})" , Height , TargetHeight ) ;
            return await _executor.Down ( ).ConfigureAwait ( false ) ;
        }

        return false ;
    }

    /// <inheritdoc />
    public async Task < bool > Stop ( )
    {
        _logger.Debug ( "Stopping... (height={Height} speed={Speed} target={Target})" , Height , Speed , TargetHeight ) ;

        // If we are already stopped, avoid duplicate work and events
        if ( ! IsAllowedToMove && _cycleDisposables == null )
        {
            _logger.Debug ( "Already stopped (suppress duplicate)" ) ;
            return true ;
        }

        IsAllowedToMove = false ;
        _calculator.MoveIntoDirection = Direction.None ;

        _cycleDisposables?.Dispose ( ) ;
        _cycleDisposables = null ;

        var stop = await _engine.StopAsync ( ).ConfigureAwait ( false ) ;

        if ( ! stop )
        {
            _logger.Error ( "Failed to stop (executor returned false)" ) ;
        }

        _logger.Debug ( "Emitting finished (height={Height})" ,
                        Height ) ;

        if ( ! _finishedEmitted )
        {
            _finishedEmitted = true ;
            _subjectFinished.OnNext ( Height ) ;
        }

        return stop ;
    }

    /// <inheritdoc />
    public void Dispose ( )
    {
        _monitor?.Dispose ( ) ;
        _disposableProvider?.Dispose ( ) ;
        _rawHeightAndSpeedSubscription?.Dispose ( ) ;
        _cycleDisposables?.Dispose ( ) ;

        GC.SuppressFinalize ( this ) ;
    }

    /// <inheritdoc />
    public bool IsAllowedToMove
    {
        get => Volatile.Read ( ref _isAllowedToMove ) ;
        private set => Volatile.Write ( ref _isAllowedToMove ,
                                        value ) ;
    }

    private void StartAfterReceivingCurrentHeight ( )
    {
        _logger.Debug ( "Initial height received -> start control loop (height={Height} target={Target})" , _heightAndSpeed.Height , TargetHeight ) ;

        if ( TargetHeight == 0 )
        {
            _logger.Warning ( "TargetHeight is 0 (control loop may no-op)" ) ;
            return ;
        }

        Height = _heightAndSpeed.Height ;
        Speed = _heightAndSpeed.Speed ;

        // Compute initial start direction once for this cycle
        _calculator.Height = Height ;
        _calculator.Speed = Speed ;
        _calculator.TargetHeight = TargetHeight ;
        _calculator.StartMovingIntoDirection = Direction.None ;
        _calculator.Calculate ( ) ;
        StartMovingIntoDirection = _calculator.MoveIntoDirection ;

        _logger.Debug ( "Calculated initial direction={Dir}" , StartMovingIntoDirection ) ;

        _cycleDisposables?.Dispose ( ) ;
        _cycleDisposables = new CompositeDisposable ( ) ;

        // Use sampled stream only to trigger movement evaluation
        _cycleDisposables.Add ( HeightAndSpeedSampled.Subscribe ( __ => _ = TryEvaluateMoveAsync ( ) ) ) ;

        // Periodic evaluation timer
        _cycleDisposables.Add ( Observable.Interval ( TimerInterval ,
                                                      _scheduler ).SubscribeAsync ( OnTimerElapsed ) ) ;

        IsAllowedToMove = true ;
        _finishedEmitted = false ;

        _stopper.Reset ( ) ;
    }

    private void OnFinished ( uint height )
    {
        Height = height ;
        StartAfterReceivingCurrentHeight ( ) ;
    }

    internal async Task OnTimerElapsed ( long time )
    {
        await TryEvaluateMoveAsync ( true ).ConfigureAwait ( false ) ;
    }

    private async Task TryEvaluateMoveAsync ( bool fromTimer = false )
    {
        if ( _cycleDisposables == null || ! IsAllowedToMove )
        {
            return ;
        }

        if ( ! await _moveSemaphore.WaitAsync ( 0 ).ConfigureAwait ( false ) )
        {
            _logger.Debug ( "Evaluation skipped: previous evaluation still running" ) ;
            return ;
        }

        try
        {
            _logger.Debug ( "Evaluate move (height={Height} speed={Speed} target={Target} engineDir={EngineDir})" , Height , Speed , TargetHeight , _engine.CurrentDirection ) ;

            if ( TargetHeight == 0u )
            {
                _logger.Debug ( "TargetHeight = 0 -> forcing stop" ) ;
            }

            var result = _stopper.ShouldStop ( Height ,
                                               Speed ,
                                               TargetHeight ,
                                               StartMovingIntoDirection ,
                                               _engine.CurrentDirection ) ;

            _logger.Debug ( "StopEval result stop={Stop} desired={Desired} engineDir={EngineDir}" , result.ShouldStop , result.Desired , _engine.CurrentDirection ) ;

            if ( result.ShouldStop )
            {
                if ( fromTimer || _engine.IsMoving )
                {
                    _logger.Debug ( "Issuing stop (fromTimer={FromTimer} engineMoving={Moving})" , fromTimer , _engine.IsMoving ) ;
                    IssueStopIfNotPending ( ) ;
                }

                return ;
            }

            _logger.Debug ( "Continuing movement desired={Desired}" , result.Desired ) ;
            _engine.Move ( result.Desired ,
                           fromTimer ) ;
        }
        catch ( Exception e )
        {
            _logger.Error ( e ,
                            "Move evaluation failed" ) ;
        }
        finally
        {
            _moveSemaphore.Release ( ) ;
        }
    }

    private void IssueStopIfNotPending ( )
    {
        if ( _pendingStopTask is { IsCompleted: false } )
        {
            _logger.Debug ( "Stop already pending -> coalesced" ) ;
            return ;
        }

        _pendingStopTask = Stop ( ) ;

        _ = _pendingStopTask.ContinueWith ( t =>
                                            {
                                                if ( t.IsFaulted )
                                                {
                                                    _logger.Error ( t.Exception ,
                                                                    "Stop command faulted (continuation)" ) ;
                                                }

                                                Interlocked.Exchange ( ref _pendingStopTask ,
                                                                       null ) ;
                                            } ,
                                            CancellationToken.None ,
                                            TaskContinuationOptions.ExecuteSynchronously ,
                                            TaskScheduler.Default ) ;
    }
}