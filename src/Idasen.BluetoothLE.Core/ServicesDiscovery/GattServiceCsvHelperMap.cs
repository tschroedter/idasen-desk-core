namespace Idasen.BluetoothLE.Core.ServicesDiscovery ;

using CsvHelper.Configuration ;

public sealed class GattServiceCsvHelperMap
    : ClassMap < OfficialGattService >
{
    public GattServiceCsvHelperMap ( )
    {
        Map ( m => m.Name ) ;
        Map ( m => m.UniformTypeIdentifier ) ;
        Map ( m => m.AssignedNumber ).TypeConverter < OfficialGattServiceConverter > ( ) ;
        Map ( m => m.ProfileSpecification ) ;
    }
}
