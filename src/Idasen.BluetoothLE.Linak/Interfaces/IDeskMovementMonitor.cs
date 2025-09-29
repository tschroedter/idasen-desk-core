using Idasen.BluetoothLE.Linak.Control;

namespace Idasen.BluetoothLE.Linak.Interfaces;

/// <summary>
///     Observes height changes to detect whether the desk is moving.
/// </summary>
public interface IDeskMovementMonitor
    : IDisposable
{
    /// <summary>
    ///     Initializes the monitor with the specified ring buffer capacity.
    /// </summary>
    void Initialize(int capacity = DeskMovementMonitor.DefaultCapacity);
}
