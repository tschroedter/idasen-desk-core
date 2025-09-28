namespace Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery ;

using Windows.Devices.Bluetooth ;

/// <summary>
///     Abstraction over platform BluetoothLEDevice retrieval for testability.
/// </summary>
// ReSharper disable once InconsistentNaming
public interface IBluetoothLEDeviceProvider
{
    Task < BluetoothLEDevice? > FromBluetoothAddressAsync ( ulong address ) ;
}
