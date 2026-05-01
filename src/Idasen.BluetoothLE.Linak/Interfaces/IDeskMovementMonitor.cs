using Idasen.BluetoothLE.Linak.Control ;

namespace Idasen.BluetoothLE.Linak.Interfaces ;

/// <summary>
///     Observes height changes to detect whether the desk is moving.
/// </summary>
public interface IDeskMovementMonitor
    : IDisposable
{
    /// <summary>
    ///     Initializes the monitor with the specified ring buffer capacity.
    ///     This sets up the history buffer but does not start the inactivity timer.
    /// </summary>
    void Initialize ( int capacity = DeskMovementMonitor.DefaultCapacity ) ;

    /// <summary>
    ///     Starts the inactivity watchdog timer for a new movement cycle.
    ///     This should be called at the beginning of each movement attempt.
    /// </summary>
    void Start ( ) ;

    /// <summary>
    ///     Stops the inactivity watchdog timer.
    ///     This should be called when a movement cycle completes or is cancelled.
    /// </summary>
    void StopWatchdog ( ) ;

    /// <summary>
    ///     Observable that emits when the desk has stopped responding (no height updates).
    /// </summary>
    IObservable < string > InactivityDetected { get ; }
}
