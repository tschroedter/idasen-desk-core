namespace Idasen.BluetoothLE.Characteristics.Common ;

using System.Globalization ;
using System.Reflection ;
using Aop.Aspects ;
using Autofac.Extras.DynamicProxy ;
using Core ;
using CsvHelper ;
using CsvHelper.Configuration ;
using Interfaces.Common ;

[ Intercept ( typeof ( LogAspect ) ) ]
public sealed class AllGattCharacteristicsProvider
    : IAllGattCharacteristicsProvider
{
    internal const string Filename = "OfficialGattCharacteristics.txt" ;

    private readonly Dictionary < string , Guid > _descriptionToUuid = new ( ) ;
    private readonly Dictionary < Guid , string > _uuidToDescription = new ( ) ;

    public AllGattCharacteristicsProvider ( )
    {
        OfficialGattCharacteristics = GetType ( ).Namespace +
                                      "." +
                                      Filename ;

        Populate ( ReadCsvFile ( ) ) ;
    }

    public string OfficialGattCharacteristics { get ; }

    public bool TryGetDescription ( Guid uuid ,
                                    out string description )
    {
        var success = _uuidToDescription.TryGetValue ( uuid ,
                                                       out var tempDescription ) ;

        description = tempDescription ?? string.Empty ;

        return success ;
    }

    public bool TryGetUuid ( string description ,
                             out Guid uuid )
    {
        return _descriptionToUuid.TryGetValue ( description ,
                                                out uuid ) ;
    }

    private void Populate ( IEnumerable < CsvGattCharacteristic > records )
    {
        foreach (CsvGattCharacteristic record in records)
        {
            _uuidToDescription[Guid.Parse ( record.Uuid )] = record.Name ;
        }

        foreach (( Guid uuid , var description ) in _uuidToDescription)
        {
            _descriptionToUuid[description] = uuid ;
        }
    }

    private IEnumerable < CsvGattCharacteristic > ReadCsvFile ( )
    {
        Stream? stream = Assembly.GetExecutingAssembly ( )
                                 .GetManifestResourceStream ( OfficialGattCharacteristics ) ;

        if ( stream == null )
        {
            throw new ResourceNotFoundException ( OfficialGattCharacteristics ,
                                                  $"Can't find resource {OfficialGattCharacteristics}" ) ;
        }

        using var reader = new StreamReader ( stream ) ;

        var config = new CsvConfiguration ( CultureInfo.InvariantCulture )
        {
            Delimiter = "," ,
            HasHeaderRecord = true
        } ;

        using var csv = new CsvReader ( reader ,
                                        config ) ;

        CsvGattCharacteristic [ ] readCsvFile = csv.GetRecords < CsvGattCharacteristic > ( )
                                                   .ToArray ( ) ;

        return readCsvFile ;
    }
}
