namespace Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery.Wrappers ;

using Windows.Devices.Bluetooth.GenericAttributeProfile ;

public interface IGattWriteResultWrapperFactory
{
    /// <summary>
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    IGattWriteResultWrapper Create ( GattWriteResult result ) ;
}
