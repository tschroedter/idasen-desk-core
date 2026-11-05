using System.Globalization;
using Autofac.Extras.DynamicProxy;
using CsvHelper;
using Idasen.Aop.Aspects;

namespace Idasen.BluetoothLE.Core.ServicesDiscovery;

[Intercept(typeof(LogAspect))]
public sealed class OfficialGattServiceConverter
    : ICsvConverter<ushort>
{
    public ushort Read(CsvReader reader, int index)
    {
        ArgumentNullException.ThrowIfNull(reader);

        var text = reader.GetString(index);

        var number = text?.Replace("0x",
                                   "",
                                   StringComparison.InvariantCulture) ??
                     string.Empty;

        if (string.IsNullOrWhiteSpace(text))
            return 0;

        return ushort.TryParse(number,
                               NumberStyles.HexNumber,
                               CultureInfo.InvariantCulture,
                               out var value)
                   ? value
                   : ushort.MaxValue;
    }

    public void Write(CsvWriter writer, ushort value)
    {
        ArgumentNullException.ThrowIfNull(writer);

        writer.WriteField(value.ToString(CultureInfo.InvariantCulture));
    }
}
