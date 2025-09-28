namespace Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery.Wrappers ;

using Windows.Devices.Bluetooth.GenericAttributeProfile ;

public interface IGattCharacteristicWrapperFactory
{
    IGattCharacteristicWrapper Create ( GattCharacteristic characteristic ) ;
}
