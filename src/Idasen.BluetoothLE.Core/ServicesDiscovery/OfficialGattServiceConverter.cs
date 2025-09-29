using System.Globalization ;
using Autofac.Extras.DynamicProxy ;
using CsvHelper ;
using CsvHelper.Configuration ;
using CsvHelper.TypeConversion ;
using Idasen.Aop.Aspects ;

namespace Idasen.BluetoothLE.Core.ServicesDiscovery ;

[ Intercept ( typeof ( LogAspect ) ) ]
public class OfficialGattServiceConverter
    : DefaultTypeConverter
{
    public override object ConvertFromString ( string ?      text ,
                                               IReaderRow    row ,
                                               MemberMapData memberMapData )
    {
        Guard.ArgumentNotNull ( row ,
                                nameof ( row ) ) ;
        Guard.ArgumentNotNull ( memberMapData ,
                                nameof ( memberMapData ) ) ;

        var number = text?.Replace ( "0x" ,
                                     "" ,
                                     StringComparison.InvariantCulture ) ??
                     string.Empty ;

        if ( string.IsNullOrWhiteSpace ( text ) )
            return ( ushort )0 ;

        return ushort.TryParse ( number ,
                                 NumberStyles.HexNumber ,
                                 CultureInfo.InvariantCulture ,
                                 out var value )
                   ? value
                   : ushort.MaxValue ;
    }

    public override string ConvertToString ( object ?      value ,
                                             IWriterRow    row ,
                                             MemberMapData memberMapData )
    {
        Guard.ArgumentNotNull ( row ,
                                nameof ( row ) ) ;
        Guard.ArgumentNotNull ( memberMapData ,
                                nameof ( memberMapData ) ) ;

        return value != null
                   ? value.ToString ( ) ?? "null"
                   : "null" ;
    }
}