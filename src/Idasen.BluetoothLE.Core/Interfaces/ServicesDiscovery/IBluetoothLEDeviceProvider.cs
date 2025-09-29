using Windows.Devices.Bluetooth ;

namespace Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery ;

/// <summary>
///     Abstraction over platform BluetoothLEDevice retrieval for testability.
/// </summary>
// ReSharper disable once InconsistentNaming
public interface IBluetoothLEDeviceProvider
{
    Task < BluetoothLEDevice ? > FromBluetoothAddressAsync ( ulong address ) ;
}