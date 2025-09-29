using Autofac.Extras.DynamicProxy;
using Idasen.Aop.Aspects;
using Idasen.BluetoothLE.Characteristics.Interfaces.Common;
using Idasen.BluetoothLE.Core;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery.Wrappers;
using Serilog;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace Idasen.BluetoothLE.Characteristics.Common;

/// <summary>
///     Reads raw values from GATT characteristics and exposes the last status and protocol error.
/// </summary>
[Intercept(typeof(LogAspect))]
public sealed class RawValueReader
    : IRawValueReader
{
    private static readonly byte[] ArrayEmpty = [];

    private readonly ILogger _logger;
    private readonly IBufferReader _reader;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RawValueReader" /> class.
    /// </summary>
    /// <param name="logger">Logger used for warnings and diagnostics.</param>
    /// <param name="reader">Helper used to extract bytes from the platform buffer.</param>
    public RawValueReader(
        ILogger logger,
        IBufferReader reader)
    {
        Guard.ArgumentNotNull(
            logger,
            nameof(logger));
        Guard.ArgumentNotNull(
            reader,
            nameof(reader));

        _logger = logger;
        _reader = reader;
    }

    /// <summary>
    ///     Gets the last protocol error from a read operation, if any.
    /// </summary>
    public byte? ProtocolError { get; private set; }

    /// <summary>
    ///     Gets the last GATT communication status from a read operation.
    /// </summary>
    public GattCommunicationStatus Status { get; private set; } = GattCommunicationStatus.Unreachable;

    /// <inheritdoc />
    public async Task<(bool, byte[])> TryReadValueAsync(IGattCharacteristicWrapper characteristic)
    {
        Guard.ArgumentNotNull(
            characteristic,
            nameof(characteristic));

        if (SupportsNotify(characteristic)) {
            _logger.Warning(
                "GattCharacteristic {CharacteristicUuid} doesn't support {UnsupportedOperation} but supports {SupportedOperation}",
                characteristic.Uuid,
                "Read",
                "Notify");

            return (false, ArrayEmpty); // need to subscribe to value change
        }

        if (SupportsRead(characteristic))
            return await ReadValue(characteristic);

        _logger.Information(
            "GattCharacteristic {CharacteristicUuid} doesn't support {UnsupportedOperation}",
            characteristic.Uuid,
            "Read");

        return (false, ArrayEmpty);
    }

    private static bool SupportsRead(IGattCharacteristicWrapper characteristic)
    {
        return (characteristic.CharacteristicProperties & GattCharacteristicProperties.Read) ==
               GattCharacteristicProperties.Read;
    }

    private static bool SupportsNotify(IGattCharacteristicWrapper characteristic)
    {
        return (characteristic.CharacteristicProperties & GattCharacteristicProperties.Notify) ==
               GattCharacteristicProperties.Notify;
    }

    private async Task<(bool, byte[])> ReadValue(IGattCharacteristicWrapper characteristic)
    {
        Guard.ArgumentNotNull(
            characteristic,
            nameof(characteristic));

        var readValue = await characteristic.ReadValueAsync();

        ProtocolError = readValue.ProtocolError;
        Status = readValue.Status;

        if (GattCommunicationStatus.Success != Status)
            return (false, ArrayEmpty);

        if (readValue.Value == null)
            return (false, ArrayEmpty);

        return _reader.TryReadValue(
            readValue.Value,
            out var bytes)
            ? (true, bytes)
            : (false, ArrayEmpty);
    }
}
