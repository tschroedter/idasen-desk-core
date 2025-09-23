using System.Reactive.Concurrency ;
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

    // Tracks when we are executing inside Move() to avoid re-entrant deadlocks in Stop()
    private static readonly AsyncLocal < bool > InMove = new ( ) ;

    private readonly IStoppingHeightCalculator _calculator ;
    private readonly IDeskCommandExecutor _executor ;
    private readonly IDeskHeightAndSpeed _heightAndSpeed ;
    private readonly IDeskHeightMonitor _heightMonitor ;

    private readonly ILogger _logger ;
    private readonly IDeskMovementMonitorFactory _monitorFactory ;

    // Prevent overlapping Move() executions across timer ticks
    private readonly SemaphoreSlim _moveSemaphore = new ( 1 ,
                                                          1 ) ;

    private readonly object _padlock = new ( ) ;
    private readonly IInitialHeightAndSpeedProviderFactory _providerFactory ;
    private readonly IScheduler _scheduler ;
    private readonly DeskMoverSettings _settings ;
    private readonly ISubject < uint > _subjectFinished ;

    // Tracks the last command sent to the executor to avoid re-sending the same command
    private Direction _currentCommandedDirection = Direction.None ;

    private IDisposable? _disposableProvider ;
    private IDisposable? _disposableTimer ;
    private IDisposable? _disposalHeightAndSpeed ;
    private volatile bool _finishedEmitted ;

    // Exposes a reduced-rate stream of the latest height/speed using TimerInterval.
    private IObservable < HeightSpeedDetails >? _heightAndSpeedSampled ;
    private IInitialHeightProvider? _initialProvider ;

    // Backing field with volatile memory semantics for IsAllowedToMove
    private bool _isAllowedToMove ;
    private IDeskMovementMonitor? _monitor ;

    // Pending async command tasks to avoid overlapping awaits and reduce latency while holding the semaphore
    private Task < bool >? _pendingMoveCommandTask ;
    private Task < bool >? _pendingStopTask ;

    // Tracks consecutive polls where height did not change to avoid premature stop near target
    private int _noMovementPolls ;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DeskMover" /> class.
    ///     This overload uses default settings.
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
        : this ( logger ,
                 scheduler ,
                 providerFactory ,
                 monitorFactory ,
                 executor ,
                 heightAndSpeed ,
                 calculator ,
                 subjectFinished ,
                 heightMonitor ,
                 DeskMoverSettings.Default )
    {
    }

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
                       IDeskHeightMonitor heightMonitor ,
                       DeskMoverSettings settings )
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

        _logger = logger ;
        _scheduler = scheduler ;
        _providerFactory = providerFactory ;
        _monitorFactory = monitorFactory ;
        _executor = executor ;
        _heightAndSpeed = heightAndSpeed ;
        _calculator = calculator ;
        _subjectFinished = subjectFinished ;
        _heightMonitor = heightMonitor ;
        _settings = settings ;
    }

    // Expose configured interval for internal use and diagnostics
    public TimeSpan TimerInterval => _settings.TimerInterval ;

    /// <summary>
    ///     Direction used for the first movement step once height is known.
    /// </summary>
    public Direction StartMovingIntoDirection { get ; set ; }

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
            return await _executor.Up ( ).ConfigureAwait ( false ) ;
        }

        return false ;
    }

    /// <inheritdoc />
    public async Task < bool > Down ( )
    {
        if ( IsAllowedToMove )
        {
            return await _executor.Down ( ).ConfigureAwait ( false ) ;
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
        _noMovementPolls = 0 ;

        var calledFromMove = InMove.Value ;

        if ( calledFromMove )
        {
            // Avoid deadlock: OnTimerElapsed already holds the semaphore
            var stop = await _executor.Stop ( ).ConfigureAwait ( false ) ;

            if ( ! stop )
            {
                _logger.Error ( "Failed to stop" ) ;
            }

            _logger.Debug ( "Sending finished with height {Height}" ,
                            Height ) ;

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

            _logger.Debug ( "Sending finished with height {Height}" ,
                            Height ) ;

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

        // Subscribe to reduced-rate height/speed stream
        _disposalHeightAndSpeed = HeightAndSpeedSampled
           .Subscribe ( OnHeightAndSpeedChanged ) ;

        _disposableTimer?.Dispose ( ) ;
        // Use OnTimerElapsed to centralize execution and avoid overlapping calls
        _disposableTimer = Observable.Interval ( TimerInterval ,
                                                 _scheduler )
                                     .SubscribeAsync ( OnTimerElapsed ) ;

        IsAllowedToMove = true ;
        _finishedEmitted = false ;
        _currentCommandedDirection = Direction.None ;
        _noMovementPolls = 0 ;

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
            InMove.Value = true ;
            await Move ( fromTimer ).ConfigureAwait ( false ) ;
        }
        catch ( Exception e )
        {
            _logger.Error ( e ,
                            "Calling Move() failed" ) ;
        }
        finally
        {
            InMove.Value = false ;
            _moveSemaphore.Release ( ) ;
        }
    }

    private void OnHeightAndSpeedChanged ( HeightSpeedDetails details )
    {
        Height = details.Height ;
        Speed = details.Speed ;
        _ = TryEvaluateMoveAsync ( ) ;
    }

    private void IssueStopIfNotPending ( )
    {
        if ( _pendingStopTask is { IsCompleted: false } )
        {
            return ;
        }

        _pendingStopTask = Stop ( ) ;

        _ = _pendingStopTask.ContinueWith ( t =>
                                            {
                                                if ( t.IsFaulted )
                                                {
                                                    _logger.Error ( t.Exception ,
                                                                    "Stop command faulted" ) ;
                                                }

                                                Interlocked.Exchange ( ref _pendingStopTask ,
                                                                       null ) ;
                                            } ,
                                            CancellationToken.None ,
                                            TaskContinuationOptions.ExecuteSynchronously ,
                                            TaskScheduler.Default ) ;
    }

    private void IssueMoveCommand ( Direction desired )
    {
        if ( _pendingMoveCommandTask is { IsCompleted: false } )
        {
            return ;
        }

        _currentCommandedDirection = desired ; // optimistic set to avoid duplicate sends

        var task = desired == Direction.Up
                       ? _executor.Up ( )
                       : _executor.Down ( ) ;

        _pendingMoveCommandTask = task ;

        _ = task.ContinueWith ( t =>
                                {
                                    if ( t.IsFaulted )
                                    {
                                        _logger.Error ( t.Exception ,
                                                        "Move command faulted" ) ;
                                    }

                                    var ok = t is { Status: TaskStatus.RanToCompletion , Result: true } ;

                                    if ( ! ok )
                                    {
                                        // Only reset if we still think we are commanding this direction
                                        if ( _currentCommandedDirection == desired )
                                        {
                                            _currentCommandedDirection = Direction.None ;
                                        }
                                    }

                                    // clear pending task reference
                                    Interlocked.Exchange ( ref _pendingMoveCommandTask ,
                                                           null ) ;
                                } ,
                                CancellationToken.None ,
                                TaskContinuationOptions.ExecuteSynchronously ,
                                TaskScheduler.Default ) ;
    }

    private Task Move ( bool fromTimer )
    {
        _logger.Debug ( "Move..." ) ;

        if ( ! IsAllowedToMove )
        {
            _logger.Debug ( "Not allowed to move..." ) ;
            return Task.CompletedTask ;
        }

        if ( TargetHeight == 0u )
        {
            _logger.Debug ( "*** TargetHeight = 0" ) ;
        }

        _heightMonitor.AddHeight ( Height ) ;

        // Only consider a stall if we are already commanding movement, and it has persisted.
        var activelyCommanding = _currentCommandedDirection != Direction.None ;

        if ( ! _heightMonitor.IsHeightChanging ( ) )
        {
            _noMovementPolls++ ;

            // Treat as stall only when we have been commanding and the stall persisted long enough.
            var longStall = _noMovementPolls >= 20 ;

            if ( activelyCommanding && longStall )
            {
                _logger.Warning ( "Failed, desk not moving during last " +
                                  "{MinimumNumberOfItems} polls." ,
                                  DeskHeightMonitor.MinimumNumberOfItems ) ;
                IssueStopIfNotPending ( ) ;
                return Task.CompletedTask ;
            }
        }
        else
        {
            // Reset stall counter as soon as we detect movement again
            _noMovementPolls = 0 ;
        }

        _calculator.Height = Height ;
        _calculator.Speed = Speed ;
        _calculator.TargetHeight = TargetHeight ;
        _calculator.StartMovingIntoDirection = StartMovingIntoDirection ;
        _calculator.Calculate ( ) ;

        var desired = _calculator.MoveIntoDirection ;

        // Compute tolerance band and predicted crossing using MovementUntilStop
        var movementAbs = ( uint ) Math.Abs ( _calculator.MovementUntilStop ) ;
        var toleranceDynamic = Math.Min ( _settings.NearTargetMaxDynamicTolerance ,
                                          movementAbs ) ;
        var tolerance = Math.Max ( _settings.NearTargetBaseTolerance ,
                                   toleranceDynamic ) ;

        // If within near-target band, stop immediately to avoid hunting
        var diff = Height > TargetHeight
                       ? Height - TargetHeight
                       : TargetHeight - Height ;

        if ( diff <= tolerance )
        {
            IssueStopIfNotPending ( ) ;
            return Task.CompletedTask ;
        }

        // Predictive crossing-stop to avoid overshoot
        if ( desired == Direction.Up )
        {
            if ( movementAbs > 0 && Height < TargetHeight )
            {
                var predictedStop = Height + movementAbs ;

                if ( predictedStop >= TargetHeight )
                {
                    IssueStopIfNotPending ( ) ;
                    return Task.CompletedTask ;
                }
            }
        }
        else if ( desired == Direction.Down )
        {
            if ( movementAbs > 0 && Height > TargetHeight )
            {
                var predictedStop = Height <= movementAbs
                                        ? 0u
                                        : Height - movementAbs ;

                if ( predictedStop <= TargetHeight )
                {
                    IssueStopIfNotPending ( ) ;
                    return Task.CompletedTask ;
                }
            }
        }

        if ( desired == Direction.None || _calculator.HasReachedTargetHeight )
        {
            // Only stop on timer ticks or if we were moving already.
            if ( fromTimer || _currentCommandedDirection != Direction.None )
            {
                IssueStopIfNotPending ( ) ;
            }

            return Task.CompletedTask ;
        }

        if ( desired != _currentCommandedDirection )
        {
            if ( _currentCommandedDirection != Direction.None )
            {
                // Direction change while already moving -> stop first
                IssueStopIfNotPending ( ) ;
                return Task.CompletedTask ;
            }

            // Start moving in the desired direction
            IssueMoveCommand ( desired ) ;
            return Task.CompletedTask ;
        }

        // desired == _currentCommandedDirection:
        // Re-issue the same command to keep the desk moving (LINAK requires periodic keep-alive).
        IssueMoveCommand ( desired ) ;

        return Task.CompletedTask ;
    }
}