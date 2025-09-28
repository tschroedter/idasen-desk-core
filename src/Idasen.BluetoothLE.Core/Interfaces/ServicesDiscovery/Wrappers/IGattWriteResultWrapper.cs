

// ReSharper disable UnusedMember.Global

namespace Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery.Wrappers ;

using Windows.Devices.Bluetooth.GenericAttributeProfile ;

public interface IGattWriteResultWrapper
{
    GattCommunicationStatus Status { get ; }
    byte? ProtocolError { get ; }
}
