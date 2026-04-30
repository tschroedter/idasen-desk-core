using Idasen.BluetoothLE.Linak.Interfaces ;
using Serilog ;

namespace Idasen.BluetoothLE.Linak.Control ;

/// <summary>
///     Issues repeated move commands every DelayInterval until stopped.
/// </summary>
internal class DeskMoveEngine
    : IDeskMoveEngine
{
    private const int MaxConsecutiveFailures = 3 ;

    private readonly IDeskCommandExecutor _executor ;
    private readonly ILogger              _logger ;

    public DeskMoveEngine ( ILogger              logger ,
                            IDeskCommandExecutor executor )
    {
        ArgumentNullException.ThrowIfNull ( logger ) ;
        ArgumentNullException.ThrowIfNull ( executor ) ;

        _logger   = logger ;
        _executor = executor ;
    }

    /// <summary>
    ///     Gets or sets the interval between repeated move commands. Default is 100ms.
    /// </summary>
    public TimeSpan DelayInterval { get ; set ; } = TimeSpan.FromMilliseconds ( 100 ) ;

    /// <summary>
    ///     Indicates the current movement direction, or None if not moving.
    /// </summary>
    public Direction CurrentDirection { get ; private set ; } = Direction.None ;

    /// <summary>
    ///     Indicates whether the desk is currently moving.
    /// </summary>
    public bool IsMoving => CurrentDirection != Direction.None ;

    /// <summary>
    ///     Starts moving in the desired direction, issuing commands every DelayInterval until the cancellation token is
    ///     triggered.
    /// </summary>
    /// <param name="desired">The direction to move.</param>
    /// <param name="cancellationToken">Token to cancel the repeated move commands.</param>
    public async Task StartMoveAsync ( Direction         desired ,
                                       CancellationToken cancellationToken )
    {
        if ( desired == Direction.None )
            return ;

        if ( CurrentDirection == desired )
            return ;

        CurrentDirection = desired ;
        _logger.Debug ( "Starting repeated move commands: {Desired}" ,
                        desired ) ;

        var consecutiveFailures = 0 ;

        try
        {
            while ( ! cancellationToken.IsCancellationRequested &&
                    CurrentDirection == desired )
            {
                var moveTask = desired == Direction.Up
                                   ? _executor.Up ( )
                                   : _executor.Down ( ) ;
                try
                {
                    var ok = await moveTask.ConfigureAwait ( false ) ;

                    if ( ! ok )
                    {
                        consecutiveFailures++ ;
                        _logger.Debug ( "StartMoveAsync command failed: {Desired} (consecutive failures: {Count})" ,
                                        desired ,
                                        consecutiveFailures ) ;

                        if ( consecutiveFailures >= MaxConsecutiveFailures )
                        {
                            _logger.Warning ( "Stopping move due to {Count} consecutive failures" ,
                                              consecutiveFailures ) ;
                            break ;
                        }
                    }
                    else
                    {
                        consecutiveFailures = 0 ; // Reset on success
                    }
                }
                catch ( Exception ex )
                {
                    consecutiveFailures++ ;
                    _logger.Error ( ex ,
                                    "StartMoveAsync command threw exception: {Desired} (consecutive failures: {Count})" ,
                                    desired ,
                                    consecutiveFailures ) ;

                    if ( consecutiveFailures >= MaxConsecutiveFailures )
                    {
                        _logger.Warning ( "Stopping move due to {Count} consecutive failures" ,
                                          consecutiveFailures ) ;
                        break ;
                    }
                }

                await Task.Delay ( DelayInterval ,
                                   cancellationToken ).ConfigureAwait ( false ) ;
            }
        }
        catch ( TaskCanceledException ex )
        {
            _logger.Debug ( ex ,
                            "Start move was canceled: {Desired}" ,
                            desired ) ;
        }
        finally
        {
            _logger.Debug ( "Stopped repeated move commands: {Desired}" ,
                            desired ) ;

            CurrentDirection = Direction.None ;
        }
    }

    /// <summary>
    ///     Stops issuing move commands. (No longer needed, kept for compatibility)
    /// </summary>
    public async Task StopMoveAsync ( )
    {
        CurrentDirection = Direction.None ;

        await _executor.StopMovement ( )
                       .ConfigureAwait ( false ) ;
    }
}
