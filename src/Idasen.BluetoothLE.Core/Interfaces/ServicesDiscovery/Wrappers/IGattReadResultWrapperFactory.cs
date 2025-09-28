namespace Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery.Wrappers ;

using Windows.Devices.Bluetooth.GenericAttributeProfile ;

public interface IGattReadResultWrapperFactory
{
    /// <summary>
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    IGattReadResultWrapper Create ( GattReadResult result ) ;
}
