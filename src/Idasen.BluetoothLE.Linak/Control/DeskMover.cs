using System.Reactive.Concurrency ;
using System.Reactive.Linq ;
using System.Reactive.Subjects ;
using System.Runtime.CompilerServices ;
using Autofac.Extras.DynamicProxy ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Core ;
using Idasen.BluetoothLE.Linak.Interfaces ;
using Serilog ;
using System.Threading ;

[ assembly : InternalsVisibleTo ( "Idasen.BluetoothLE.Linak.Tests" ) ]

namespace Idasen.BluetoothLE.Linak.Control ;

[ Intercept ( typeof ( LogAspect ) ) ]
public class DeskMover
    : IDeskMover
{
    public delegate IDeskMover Factory ( IDeskCommandExecutor executor ,
                                         IDeskHeightAndSpeed heightAndSpeed ) ;

    private readonly IStoppingHeightCalculator _calculator ;
    private readonly IDeskCommandExecutor _executor ;
    private readonly IDeskHeightAndSpeed _heightAndSpeed ;
    private readonly IDeskHeightMonitor _heightMonitor ;

    private readonly ILogger _logger ;
    private readonly IDeskMovementMonitorFactory _monitorFactory ;
    private readonly object _padlock = new ( ) ;
    private readonly IInitialHeightAndSpeedProviderFactory _providerFactory ;
    private readonly IScheduler _scheduler ;
    private readonly ISubject < uint > _subjectFinished ;

    /// <summary>
    ///     Timer tick interval used to drive movement evaluation.
    /// </summary>
    public readonly TimeSpan TimerInterval = TimeSpan.FromMilliseconds ( 100 ) ;

    private IDisposable? _disposableProvider ;
    private IDisposable? _disposableTimer ;
    private IDisposable? _disposalHeightAndSpeed ;
    private IInitialHeightProvider? _initialProvider ;
    private IDeskMovementMonitor? _monitor ;

    // Prevent overlapping Move() executions across timer ticks
    private readonly SemaphoreSlim _moveSemaphore = new ( 1 , 1 ) ;
    private volatile bool _finishedEmitted ;

    // Backing field with volatile memory semantics for IsAllowedToMove
    private bool _isAllowedToMove ;

    // Tracks when we are executing inside Move() to avoid re-entrant deadlocks in Stop()
    private static readonly AsyncLocal < bool > _inMove = new ( ) ;

    // Tracks the last command sent to the executor to avoid re-sending the same command
    private Direction _currentCommandedDirection = Direction.None ;

    // Near-target tolerance band (device units)
    private const uint NearTargetBaseTolerance = 2u ;
    private const uint NearTargetMaxDynamicTolerance = 10u ;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DeskMover" /> class.
    /// </summary>
    public DeskMover ( ILogger logger ,
                       IScheduler scheduler ,
                       IInitialHeightAndSpeedProviderFactory providerFactory ,
                       IDeskMovementMonitorFactory monitorFactory ,
                       IDeskCommandExecutor executor ,
                       IDeskHeightAndSpeed heightAndSpeed ,
                       IStoppingHeightCalculator calculator ,
                       ISubject < uint > subjectFinished ,
                       IDeskHeightMonitor heightMonitor )
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

        _logger = logger ;
        _scheduler = scheduler ;
        _providerFactory = providerFactory ;
        _monitorFactory = monitorFactory ;
        _executor = executor ;
        _heightAndSpeed = heightAndSpeed ;
        _calculator = calculator ;
        _subjectFinished = subjectFinished ;
        _heightMonitor = heightMonitor ;
    }

    /// <summary>
    ///     Direction used for the first movement step once height is known.
    /// </summary>
    public Direction StartMovingIntoDirection { get ; set ; }

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
        }
    }

    /// <inheritdoc />
    public void Start ( )
    {
        _logger.Debug ( "Starting..." ) ;

        _disposalHeightAndSpeed?.Dispose ( ) ;
        _disposableTimer?.Dispose ( ) ;

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
            return await _executor.Up ( ) ;
        }

        return false ;
    }

    /// <inheritdoc />
    public async Task < bool > Down ( )
    {
        if ( IsAllowedToMove )
        {
            return await _executor.Down ( ) ;
        }

        return false ;
    }

    /// <inheritdoc />
    public async Task < bool > Stop ( )
    {
        _logger.Debug ( "Stopping..." ) ;

        // If we are already stopped, avoid duplicate work and events
        if ( ! IsAllowedToMove && _disposableTimer == null )
        {
            _logger.Debug ( "Already stopped" ) ;
            return true ;
        }

        IsAllowedToMove = false ;
        _calculator.MoveIntoDirection = Direction.None ;

        _disposableTimer?.Dispose ( ) ;
        _disposableTimer = null ;
        _currentCommandedDirection = Direction.None ;

        var calledFromMove = _inMove.Value ;

        if ( calledFromMove )
        {
            // Avoid deadlock: OnTimerElapsed already holds the semaphore
            var stop = await _executor.Stop ( ).ConfigureAwait ( false ) ;

            if ( ! stop )
            {
                _logger.Error ( "Failed to stop" ) ;
            }

            _logger.Debug ( $"Sending finished with height {Height}" ) ;

            if ( ! _finishedEmitted )
            {
                _finishedEmitted = true ;
                _subjectFinished.OnNext ( Height ) ;
            }

            return stop ;
        }

        // Ensure no concurrent Move() is running while issuing Stop
        await _moveSemaphore.WaitAsync ( ).ConfigureAwait ( false ) ;
        try
        {
            var stop = await _executor.Stop ( ).ConfigureAwait ( false ) ;

            if ( ! stop )
            {
                _logger.Error ( "Failed to stop" ) ;
            }

            _logger.Debug ( $"Sending finished with height {Height}" ) ;

            if ( ! _finishedEmitted )
            {
                _finishedEmitted = true ;
                _subjectFinished.OnNext ( Height ) ;
            }

            return stop ;
        }
        finally
        {
            _moveSemaphore.Release ( ) ;
        }
    }

    /// <inheritdoc />
    public void Dispose ( )
    {
        _monitor?.Dispose ( ) ;
        _disposableProvider?.Dispose ( ) ;
        _disposalHeightAndSpeed?.Dispose ( ) ;
        _disposableTimer?.Dispose ( ) ; // todo testing
    }

    /// <inheritdoc />
    public bool IsAllowedToMove
    {
        get => Volatile.Read ( ref _isAllowedToMove ) ;
        private set => Volatile.Write ( ref _isAllowedToMove , value ) ;
    }

    private void StartAfterReceivingCurrentHeight ( )
    {
        _logger.Debug ( "Start after refreshed..." ) ;

        if ( TargetHeight == 0 )
        {
            _logger.Warning ( "TargetHeight is 0" ) ;

            return ;
        }

        Height = _heightAndSpeed.Height ;
        Speed = _heightAndSpeed.Speed ;

        _calculator.Height = Height ;
        _calculator.Speed = Speed ;
        _calculator.StartMovingIntoDirection = Direction.None ;
        _calculator.TargetHeight = TargetHeight ;

        _calculator.Calculate ( ) ;

        StartMovingIntoDirection = _calculator.MoveIntoDirection ;

        _disposalHeightAndSpeed = _heightAndSpeed.HeightAndSpeedChanged
                                                 .Subscribe ( OnHeightAndSpeedChanged ) ;

        _disposableTimer?.Dispose ( ) ;
        // Use OnTimerElapsed to centralize execution and avoid overlapping calls
        _disposableTimer = Observable.Interval ( TimerInterval )
                                     .ObserveOn ( _scheduler )
                                     .SubscribeAsync ( OnTimerElapsed ) ;

        IsAllowedToMove = true ;
        _finishedEmitted = false ;
        _currentCommandedDirection = Direction.None ;

        _heightMonitor.Reset ( ) ;
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
        if ( _disposableTimer == null || ! IsAllowedToMove )
        {
            return ;
        }

        if ( ! await _moveSemaphore.WaitAsync ( 0 ).ConfigureAwait ( false ) )
        {
            _logger.Debug ( "Move() still running, skipping evaluation" ) ;
            return ;
        }

        try
        {
            _inMove.Value = true ;
            await Move ( fromTimer ).ConfigureAwait ( false ) ;
        }
        catch ( Exception e )
        {
            _logger.Error ( e ,
                            "Calling Move() failed" ) ;
        }
        finally
        {
            _inMove.Value = false ;
            _moveSemaphore.Release ( ) ;
        }
    }

    private void OnHeightAndSpeedChanged ( HeightSpeedDetails details )
    {
        Height = details.Height ;
        Speed = details.Speed ;
        _ = TryEvaluateMoveAsync ( false ) ;
    }

    private async Task Move ( bool fromTimer )
    {
        _logger.Debug ( "Move..." ) ;

        if ( ! IsAllowedToMove )
        {
            _logger.Debug ( "Not allowed to move..." ) ;
            return ;
        }

        if ( TargetHeight == 0u )
        {
            _logger.Debug ( "*** TargetHeight = 0\r\n" +
                            $"{Environment.StackTrace}" ) ;
        }

        _heightMonitor.AddHeight ( Height ) ;

        if ( ! _heightMonitor.IsHeightChanging ( ) )
        {
            _logger.Warning ( "Failed, desk not moving during last " +
                              $"{DeskHeightMonitor.MinimumNumberOfItems} polls." ) ;
            await Stop ( ) ;
            return ;
        }

        _calculator.Height = Height ;
        _calculator.Speed = Speed ;
        _calculator.TargetHeight = TargetHeight ;
        _calculator.StartMovingIntoDirection = StartMovingIntoDirection ;
        _calculator.Calculate ( ) ;

        var desired = _calculator.MoveIntoDirection ;

        // Compute tolerance band and predicted crossing using MovementUntilStop
        var movementAbs = (uint)Math.Abs ( _calculator.MovementUntilStop ) ;
        var toleranceDynamic = Math.Min ( NearTargetMaxDynamicTolerance , movementAbs ) ;
        var tolerance = Math.Max ( NearTargetBaseTolerance , toleranceDynamic ) ;

        // If within near-target band, stop immediately to avoid hunting
        var diff = Height > TargetHeight ? Height - TargetHeight : TargetHeight - Height ;
        if ( diff <= tolerance )
        {
            await Stop ( ) ;
            return ;
        }

        // Predictive crossing-stop to avoid overshoot
        if ( desired == Direction.Up )
        {
            if ( movementAbs > 0 && Height < TargetHeight )
            {
                var predictedStop = Height + movementAbs ;
                if ( predictedStop >= TargetHeight )
                {
                    await Stop ( ) ;
                    return ;
                }
            }
        }
        else if ( desired == Direction.Down )
        {
            if ( movementAbs > 0 && Height > TargetHeight )
            {
                var predictedStop = Height <= movementAbs ? 0u : Height - movementAbs ;
                if ( predictedStop <= TargetHeight )
                {
                    await Stop ( ) ;
                    return ;
                }
            }
        }

        if ( desired == Direction.None || _calculator.HasReachedTargetHeight )
        {
            // Only stop on timer ticks or if we were moving already.
            if ( fromTimer || _currentCommandedDirection != Direction.None )
            {
                await Stop ( ) ;
            }
            return ;
        }

        if ( desired != _currentCommandedDirection )
        {
            if ( _currentCommandedDirection != Direction.None )
            {
                await Stop ( ) ;
                return ;
            }

            switch ( desired )
            {
                case Direction.Up:
                    if ( await Up ( ) )
                    {
                        _currentCommandedDirection = Direction.Up ;
                    }
                    break ;
                case Direction.Down:
                    if ( await Down ( ) )
                    {
                        _currentCommandedDirection = Direction.Down ;
                    }
                    break ;
            }

            return ;
        }

        // desired == _currentCommandedDirection: do nothing
    }
}