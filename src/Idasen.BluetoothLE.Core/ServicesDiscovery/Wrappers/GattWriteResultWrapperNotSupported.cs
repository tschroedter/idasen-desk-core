namespace Idasen.BluetoothLE.Core.ServicesDiscovery.Wrappers ;

using Windows.Devices.Bluetooth.GenericAttributeProfile ;
using Interfaces.ServicesDiscovery.Wrappers ;

public class GattWriteResultWrapperNotSupported
    : IGattWriteResultWrapper
{
    public GattCommunicationStatus Status { get ; } = GattCommunicationStatus.Unreachable ;

    public byte? ProtocolError { get ; } = null ;
}
