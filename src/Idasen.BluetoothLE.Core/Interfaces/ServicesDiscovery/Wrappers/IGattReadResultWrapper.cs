namespace Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery.Wrappers ;

using Windows.Devices.Bluetooth.GenericAttributeProfile ;
using Windows.Storage.Streams ;

public interface IGattReadResultWrapper
{
    GattCommunicationStatus Status { get ; }
    byte? ProtocolError { get ; }
    IBuffer? Value { get ; }
}
