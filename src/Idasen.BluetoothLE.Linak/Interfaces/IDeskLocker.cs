// ReSharper disable UnusedMemberInSuper.Global

namespace Idasen.BluetoothLE.Linak.Interfaces;

/// <summary>
///     Manages a logical movement lock. When locked, manual motion is detected and stopped automatically.
/// </summary>
public interface IDeskLocker
    : IDisposable
{
    /// <summary>
    ///     Indicates whether the desk is currently locked against manual movement.
    /// </summary>
    bool IsLocked { get; }

    /// <summary>
    ///     Enables the lock and starts monitoring for manual motion to issue a stop.
    /// </summary>
    /// <returns>The current instance.</returns>
    IDeskLocker Lock();

    /// <summary>
    ///     Disables the lock.
    /// </summary>
    /// <returns>The current instance.</returns>
    IDeskLocker Unlock();

    /// <summary>
    ///     Initializes the locker and required subscriptions.
    /// </summary>
    /// <returns>The current instance.</returns>
    IDeskLocker Initialize();
}
