namespace Idasen.BluetoothLE.Linak.Interfaces;

/// <summary>
///     Controls desk movement and exposes state and completion notifications.
/// </summary>
public interface IDeskMover
    : IDisposable
{
    /// <summary>
    ///     Gets the latest height value.
    /// </summary>
    uint Height { get; }

    /// <summary>
    ///     Gets the latest speed value.
    /// </summary>
    int Speed { get; }

    /// <summary>
    ///     Gets or sets the movement target height.
    /// </summary>
    uint TargetHeight { get; set; }

    /// <summary>
    ///     Notifies when a movement cycle has finished, emitting the final height.
    /// </summary>
    IObservable<uint> Finished { get; }

    /// <summary>
    ///     Indicates whether movement commands are currently allowed.
    /// </summary>
    bool IsAllowedToMove { get; }

    /// <summary>
    ///     Requests moving up.
    /// </summary>
    Task<bool> Up();

    /// <summary>
    ///     Requests moving down.
    /// </summary>
    Task<bool> Down();

    /// <summary>
    ///     Requests stopping movement.
    /// </summary>
    Task<bool> StopMovement();

    /// <summary>
    ///     Starts a movement cycle using the current target height.
    /// </summary>
    void Start();

    /// <summary>
    ///     Initializes the mover and required subscriptions.
    /// </summary>
    void Initialize();
}
