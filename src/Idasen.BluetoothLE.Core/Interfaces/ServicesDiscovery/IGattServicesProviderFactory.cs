using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery.Wrappers ;

namespace Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery ;

public interface IGattServicesProviderFactory
{
    IGattServicesProvider Create ( IBluetoothLeDeviceWrapper wrapper ) ;
}