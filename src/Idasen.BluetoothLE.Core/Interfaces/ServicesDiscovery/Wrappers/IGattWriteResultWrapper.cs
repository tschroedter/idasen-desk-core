using Windows.Devices.Bluetooth.GenericAttributeProfile ;

// ReSharper disable UnusedMember.Global

namespace Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery.Wrappers ;

public interface IGattWriteResultWrapper
{
    GattCommunicationStatus Status { get ; }
    byte? ProtocolError { get ; }
}