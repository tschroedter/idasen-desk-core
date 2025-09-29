using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery.Wrappers ;

namespace Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery ;

/// <summary>
///     Factory for creating <see cref="IGattServicesProvider" /> instances.
/// </summary>
public interface IGattServicesProviderFactory
{
    /// <summary>
    ///     Creates a provider for the specified Bluetooth LE device wrapper.
    /// </summary>
    IGattServicesProvider Create ( IBluetoothLeDeviceWrapper wrapper ) ;
}