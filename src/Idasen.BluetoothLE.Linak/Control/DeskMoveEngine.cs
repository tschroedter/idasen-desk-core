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
    private readonly ILogger _logger ;

    private Task < bool >? _pendingMoveCommandTask ;
    private Task < bool >? _pendingStopTask ;

    public DeskMoveEngine ( ILogger logger , IDeskCommandExecutor executor )
    {
        ArgumentNullException.ThrowIfNull ( logger ) ;
        ArgumentNullException.ThrowIfNull ( executor ) ;

        _logger = logger ;
        _executor = executor ;
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
        {
            return ;
        }

        // If a different direction is currently commanded, manager must stop first.
        if ( CurrentDirection != Direction.None && desired != CurrentDirection )
        {
            return ;
        }

        // If same direction and from timer, re-issue as keep-alive; if idle, start.
        if ( CurrentDirection == desired )
        {
            if ( fromTimer )
            {
                IssueMoveCommand ( desired ) ;
            }

            return ;
        }

        // Start moving in desired direction from idle
        IssueMoveCommand ( desired ) ;
    }

    /// <summary>
    ///     Issues a stop command if not already pending and resets current direction on success.
    /// </summary>
    public Task < bool > StopAsync ( )
    {
        if ( _pendingStopTask is { IsCompleted: false } )
        {
            return _pendingStopTask ;
        }

        _logger.Debug ( "Engine stopping..." ) ;

        var task = _executor.Stop ( ) ;

        if ( task.IsCompleted )
        {
            var ok = task.Status == TaskStatus.RanToCompletion && task.Result ;

            if ( ok )
            {
                CurrentDirection = Direction.None ;
            }

            return task ;
        }

        _pendingStopTask = task ;

        _ = task.ContinueWith ( t =>
                                {
                                    if ( t.IsFaulted )
                                    {
                                        _logger.Error ( t.Exception ,
                                                        "Stop command faulted" ) ;
                                    }

                                    var ok = t is { Status: TaskStatus.RanToCompletion , Result: true } ;

                                    if ( ok )
                                    {
                                        CurrentDirection = Direction.None ;
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
            return ;
        }

        // optimistic set to avoid duplicate sends
        CurrentDirection = desired ;

        var task = desired == Direction.Up
                       ? _executor.Up ( )
                       : _executor.Down ( ) ;

        if ( task.IsCompleted )
        {
            var ok = task.Status == TaskStatus.RanToCompletion && task.Result ;

            if ( ! ok && CurrentDirection == desired )
            {
                CurrentDirection = Direction.None ;
            }

            return ;
        }

        _pendingMoveCommandTask = task ;

        _ = task.ContinueWith ( t =>
                                {
                                    if ( t.IsFaulted )
                                    {
                                        _logger.Error ( t.Exception ,
                                                        "Move command faulted" ) ;
                                    }

                                    var ok = t is { Status: TaskStatus.RanToCompletion , Result: true } ;

                                    if ( ! ok && CurrentDirection == desired )
                                    {
                                        CurrentDirection = Direction.None ;
                                    }

                                    Interlocked.Exchange ( ref _pendingMoveCommandTask ,
                                                           null ) ;
                                } ,
                                CancellationToken.None ,
                                TaskContinuationOptions.ExecuteSynchronously ,
                                TaskScheduler.Default ) ;
    }
}