using Autofac.Extras.DynamicProxy ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Linak.Interfaces ;
using Serilog ;

namespace Idasen.BluetoothLE.Linak.Control ;

/// <summary>
///     Handles issuing movement commands to the desk and keeps track of the current commanded direction.
///     This type is intentionally lightweight: concurrency and orchestration are owned by the manager.
/// </summary>
[ Intercept ( typeof ( LogAspect ) ) ]
internal class DeskMoveEngine : IDeskMoveEngine
{
    private readonly IDeskCommandExecutor _executor ;
    private readonly TimeSpan             _keepAliveInterval ;
    private readonly ILogger              _logger ;
    private          DateTime             _lastKeepAlive = DateTime.MinValue ;

    private Task < bool > ? _pendingMoveCommandTask ;
    private Task < bool > ? _pendingStopTask ;

    public DeskMoveEngine ( ILogger logger , IDeskCommandExecutor executor , DeskMoverSettings ? settings = null )
    {
        ArgumentNullException.ThrowIfNull ( logger ) ;
        ArgumentNullException.ThrowIfNull ( executor ) ;

        _logger            = logger ;
        _executor          = executor ;
        _keepAliveInterval = ( settings ?? DeskMoverSettings.Default ).KeepAliveInterval ;
    }

    /// <summary>
    ///     The last direction we attempted to command. None means not moving.
    /// </summary>
    public Direction CurrentDirection { get ; private set ; } = Direction.None ;

    /// <summary>
    ///     Returns true if a movement command is currently active (best-effort based on last issued direction).
    /// </summary>
    public bool IsMoving => CurrentDirection != Direction.None ;

    /// <summary>
    ///     Request to move (or keep moving) in a desired direction. No-ops if a different direction is active;
    ///     the manager must stop first before switching directions.
    /// </summary>
    public void Move ( Direction desired , bool fromTimer )
    {
        if ( desired == Direction.None )
            return ;

        // If a different direction is currently commanded, manager must stop first.
        if ( CurrentDirection != Direction.None &&
             desired          != CurrentDirection )
        {
            _logger.Debug ( "Ignoring move request: already moving {Current} cannot switch to {Desired} without stop" ,
                            CurrentDirection ,
                            desired ) ;
            return ;
        }

        // If same direction and from timer, re-issue as keep-alive but throttle.
        if ( CurrentDirection == desired )
        {
            if ( fromTimer )
            {
                var now = DateTime.UtcNow ;

                if ( now - _lastKeepAlive >= _keepAliveInterval )
                {
                    _lastKeepAlive = now ;
                    _logger.Debug ( "Re-issuing keep-alive move {Dir}" ,
                                    desired ) ;
                    IssueMoveCommand ( desired ) ;
                }
                else
                {
                    _logger.Debug ( "Skipping keep-alive (throttled) dir={Dir} remainingMs={Remaining}" ,
                                    desired ,
                                    ( _keepAliveInterval - ( now - _lastKeepAlive ) ).TotalMilliseconds ) ;
                }
            }

            return ;
        }

        // Start moving in desired direction from idle
        _logger.Debug ( "Issuing initial move {Dir}" ,
                        desired ) ;
        _lastKeepAlive = DateTime.UtcNow ;
        IssueMoveCommand ( desired ) ;
    }

    /// <summary>
    ///     Issues a stop command if not already pending and resets current direction on success.
    /// </summary>
    public Task < bool > StopAsync ( )
    {
        if ( _pendingStopTask is { IsCompleted: false } )
        {
            _logger.Debug ( "Stop already pending (coalesced)" ) ;
            return _pendingStopTask ;
        }

        _logger.Debug ( "Engine stopping (currentDir={Dir})" ,
                        CurrentDirection ) ;

        var task = _executor.Stop ( ) ;

        if ( task.IsCompleted )
        {
            var ok = task is { Status: TaskStatus.RanToCompletion , Result: true } ;

            if ( ok )
            {
                _logger.Debug ( "Stop completed synchronously" ) ;
                CurrentDirection = Direction.None ;
            }
            else
            {
                _logger.Debug ( "Stop failed synchronously" ) ;
            }

            return task ;
        }

        _pendingStopTask = task ;

        _ = task.ContinueWith ( t =>
                                {
                                    if ( t.IsFaulted )
                                        _logger.Error ( t.Exception ,
                                                        "Stop command faulted" ) ;

                                    var ok = t is { Status: TaskStatus.RanToCompletion , Result: true } ;

                                    if ( ok )
                                    {
                                        _logger.Debug ( "Stop completed (async)" ) ;
                                        CurrentDirection = Direction.None ;
                                    }
                                    else
                                    {
                                        _logger.Debug ( "Stop failed (async)" ) ;
                                    }

                                    Interlocked.Exchange ( ref _pendingStopTask ,
                                                           null ) ;
                                } ,
                                CancellationToken.None ,
                                TaskContinuationOptions.ExecuteSynchronously ,
                                TaskScheduler.Default ) ;

        return task ;
    }

    private void IssueMoveCommand ( Direction desired )
    {
        if ( _pendingMoveCommandTask is { IsCompleted: false } )
        {
            _logger.Debug ( "Move command already pending (dir={Dir})" ,
                            desired ) ;
            return ;
        }

        // optimistic set to avoid duplicate sends
        CurrentDirection = desired ;

        _logger.Debug ( "Sending move command {Dir}" ,
                        desired ) ;
        var task = desired == Direction.Up
                       ? _executor.Up ( )
                       : _executor.Down ( ) ;

        if ( task.IsCompleted )
        {
            var ok = task is { Status: TaskStatus.RanToCompletion , Result: true } ;

            if ( ! ok &&
                 CurrentDirection == desired )
            {
                _logger.Debug ( "Move command failed synchronously -> reset direction" ) ;
                CurrentDirection = Direction.None ;
            }

            return ;
        }

        _pendingMoveCommandTask = task ;

        _ = task.ContinueWith ( t =>
                                {
                                    if ( t.IsFaulted )
                                        _logger.Error ( t.Exception ,
                                                        "Move command faulted" ) ;

                                    var ok = t is { Status: TaskStatus.RanToCompletion , Result: true } ;

                                    if ( ! ok &&
                                         CurrentDirection == desired )
                                    {
                                        _logger.Debug ( "Move command failed (async) -> reset direction" ) ;
                                        CurrentDirection = Direction.None ;
                                    }
                                    else if ( ok )
                                    {
                                        _logger.Debug ( "Move command completed (async) ok={Ok}" ,
                                                        ok ) ;
                                    }

                                    Interlocked.Exchange ( ref _pendingMoveCommandTask ,
                                                           null ) ;
                                } ,
                                CancellationToken.None ,
                                TaskContinuationOptions.ExecuteSynchronously ,
                                TaskScheduler.Default ) ;
    }
}