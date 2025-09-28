namespace Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery.Wrappers ;

using Windows.Devices.Bluetooth.GenericAttributeProfile ;
using Core.ServicesDiscovery.Wrappers ;

public interface IGattCharacteristicValueChangedObservables
    : IDisposable
{
    IObservable < GattCharacteristicValueChangedDetails > ValueChanged { get ; }
    Task Initialise ( GattCharacteristic characteristic ) ;
}
