using CsvHelper;

namespace Idasen.BluetoothLE.Core.ServicesDiscovery;

[CsvRecordMap(typeof(OfficialGattService))]
public sealed partial class GattServiceCsvHelperMap
{
    public string Name                  { get; set; } = string.Empty;
    public string UniformTypeIdentifier { get; set; } = string.Empty;

    [CsvConverter(typeof(OfficialGattServiceConverter))]
    public ushort AssignedNumber { get; set; }

    public string ProfileSpecification { get; set; } = string.Empty;
}
