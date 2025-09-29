using Windows.Devices.Bluetooth.GenericAttributeProfile ;
using Windows.Storage.Streams ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery.Wrappers ;

namespace Idasen.BluetoothLE.Core.ServicesDiscovery.Wrappers ;

public class GattReadResultWrapperNotSupported ( byte ?    protocolError = null ,
                                                 IBuffer ? value         = null )
    : IGattReadResultWrapper
{
    public GattCommunicationStatus Status { get ; } = GattCommunicationStatus.Unreachable ;

    public byte ?    ProtocolError { get ; } = protocolError ;
    public IBuffer ? Value         { get ; } = value ;
}
