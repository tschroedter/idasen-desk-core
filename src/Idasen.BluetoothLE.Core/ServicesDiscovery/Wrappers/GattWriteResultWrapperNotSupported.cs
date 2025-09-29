using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery.Wrappers;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace Idasen.BluetoothLE.Core.ServicesDiscovery.Wrappers;

public class GattWriteResultWrapperNotSupported(
    byte? protocolError = null,
    GattCommunicationStatus status = GattCommunicationStatus.Unreachable)
    : IGattWriteResultWrapper
{
    public GattCommunicationStatus Status { get; } = status;

    public byte? ProtocolError { get; } = protocolError;
}
