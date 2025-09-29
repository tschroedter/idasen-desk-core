using System.Diagnostics.CodeAnalysis;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery.Wrappers;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace Idasen.BluetoothLE.Core.ServicesDiscovery.Wrappers;

[ExcludeFromCodeCoverage]
public class GattWriteResultWrapper
    : IGattWriteResultWrapper
{
    public delegate IGattWriteResultWrapper Factory(GattWriteResult result);

    public static readonly IGattWriteResultWrapper NotSupported = new GattWriteResultWrapperNotSupported();

    private readonly GattWriteResult _result;

    public GattWriteResultWrapper(GattWriteResult result)
    {
        Guard.ArgumentNotNull(
            result,
            nameof(result));

        _result = result;
    }

    public GattCommunicationStatus Status => _result.Status;

    public byte? ProtocolError => _result.ProtocolError;
}
