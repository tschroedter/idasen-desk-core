namespace Idasen.BluetoothLE.Linak.Interfaces ;

/// <summary>
///     Factory for creating <see cref="IDesk" /> instances from a device address.
/// </summary>
public interface IDeskFactory
{
    /// <summary>
    ///     Creates a desk for the given BLE device address.
    /// </summary>
    Task < IDesk > CreateAsync ( ulong address ) ;
}