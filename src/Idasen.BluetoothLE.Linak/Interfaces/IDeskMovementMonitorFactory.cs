namespace Idasen.BluetoothLE.Linak.Interfaces;

/// <summary>
///     Factory for creating movement monitors bound to height/speed sources.
/// </summary>
public interface IDeskMovementMonitorFactory
{
    /// <summary>
    ///     Creates a movement monitor for the given height/speed source.
    /// </summary>
    IDeskMovementMonitor Create(IDeskHeightAndSpeed heightAndSpeed);
}
