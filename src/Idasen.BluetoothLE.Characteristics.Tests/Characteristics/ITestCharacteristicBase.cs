namespace Idasen.BluetoothLE.Characteristics.Tests.Characteristics ;

using Core.Interfaces.ServicesDiscovery ;
using Interfaces.Characteristics ;

public interface ITestCharacteristicBase
    : ICharacteristicBase
{
    delegate ITestCharacteristicBase Factory ( IDevice device ) ;
}
