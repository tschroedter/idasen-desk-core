using System.Text;
using Autofac.Extras.DynamicProxy;
using Idasen.Aop.Aspects;
using Idasen.BluetoothLE.Characteristics.Interfaces.Characteristics;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery;
using Idasen.BluetoothLE.Linak.Interfaces;
using Serilog;

namespace Idasen.BluetoothLE.Linak;

/// <inheritdoc />
[Intercept(typeof(LogAspect))]
public class DeskCharacteristics
    : IDeskCharacteristics
{
    private readonly Dictionary<DeskCharacteristicKey, ICharacteristicBase> _available = new();

    private readonly IDeskCharacteristicsCreator _creator;
    private readonly ILogger _logger;

    public DeskCharacteristics(
        ILogger logger,
        IDeskCharacteristicsCreator creator)
    {
        ArgumentNullException.ThrowIfNull(creator);
        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;
        _creator = creator;
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<DeskCharacteristicKey, ICharacteristicBase> Characteristics => _available;

    /// <inheritdoc />
    public async Task Refresh()
    {
        foreach (var characteristicBase in _available.Values) {
            await characteristicBase.Refresh().ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public IDeskCharacteristics Initialize(IDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);

        _creator.Create(
            this,
            device);

        return this;
    }

    /// <inheritdoc />
    public IGenericAccess GenericAccess => _available.As<IGenericAccess>(DeskCharacteristicKey.GenericAccess);

    /// <inheritdoc />
    public IGenericAttributeService GenericAttributeService =>
        _available.As<IGenericAttributeService>(DeskCharacteristicKey.GenericAttribute);

    /// <inheritdoc />
    public IReferenceInput ReferenceInput =>
        _available.As<IReferenceInput>(DeskCharacteristicKey.ReferenceInput);

    /// <inheritdoc />
    public IReferenceOutput ReferenceOutput =>
        _available.As<IReferenceOutput>(DeskCharacteristicKey.ReferenceOutput);

    /// <inheritdoc />
    public IDpg Dpg => _available.As<IDpg>(DeskCharacteristicKey.Dpg);

    /// <inheritdoc />
    public IControl Control => _available.As<IControl>(DeskCharacteristicKey.Control);

    /// <inheritdoc />
    public IDeskCharacteristics WithCharacteristics(
        DeskCharacteristicKey key,
        ICharacteristicBase characteristic)
    {
        ArgumentNullException.ThrowIfNull(characteristic);

        characteristic.Initialize<ICharacteristicBase>();

        if (_available.TryGetValue(
                key,
                out var oldCharacteristic))
            oldCharacteristic.Dispose();

        _available[key] = characteristic;

        _logger.Debug(
            "Added characteristic {Characteristic} for key {Key}",
            characteristic,
            key);

        return this;
    }

    /// <summary>
    ///     Returns a multi-line representation of the current characteristics and their values.
    /// </summary>
    public override string ToString()
    {
        var builder = new StringBuilder();

        builder.AppendLine(GenericAccess.ToString());
        builder.AppendLine(GenericAttributeService.ToString());
        builder.AppendLine(ReferenceInput.ToString());
        builder.AppendLine(ReferenceOutput.ToString());
        builder.AppendLine(Dpg.ToString());
        builder.AppendLine(Control.ToString());

        return builder.ToString();
    }
}
