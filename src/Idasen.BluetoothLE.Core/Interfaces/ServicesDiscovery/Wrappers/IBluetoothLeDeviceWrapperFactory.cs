namespace Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery.Wrappers ;

using Windows.Devices.Bluetooth ;

/// <summary>
///     Factory to create <see cref="IBluetoothLeDeviceWrapper" /> instances.
/// </summary>
public interface IBluetoothLeDeviceWrapperFactory
{
    /// <summary>
    ///     Create a <see cref="IBluetoothLeDeviceWrapper" /> instance using
    ///     the given <see cref="BluetoothLEDevice" /> instance.
    /// </summary>
    /// <param name="device">
    ///     The instance to wrap.
    /// </param>
    /// <returns>
    ///     The wrapper.
    /// </returns>
    IBluetoothLeDeviceWrapper Create ( BluetoothLEDevice device ) ;
}
