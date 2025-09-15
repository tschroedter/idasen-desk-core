using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery.Wrappers ;

namespace Idasen.BluetoothLE.Characteristics.Interfaces.Characteristics.Customs ;

public interface IGattCharacteristicsProviderFactory
{
    IGattCharacteristicProvider Create ( IGattCharacteristicsResultWrapper wrapper ) ;
}