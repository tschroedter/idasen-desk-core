using System.Collections ;
using System.Diagnostics.CodeAnalysis ;
using System.Globalization ;
using System.Reflection ;
using Autofac.Extras.DynamicProxy ;
using CsvHelper ;
using CsvHelper.Configuration ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery.Wrappers ;

namespace Idasen.BluetoothLE.Core.ServicesDiscovery ;

/// <inheritdoc />
[ Intercept ( typeof ( LogAspect ) ) ]
public class OfficialGattServices
    : IOfficialGattServices
{
    private const string FileName = "OfficialGattServices.txt" ;

    private readonly Dictionary < ushort , OfficialGattService > _dictionary = new ( ) ;

    public OfficialGattServices ( )
    {
        ResourceName = GetType ( ).Namespace + "." + FileName ;

        Populate ( ReadCsvFile ( ResourceName ) ) ;
    }

    /// <summary>
    ///     Gets the manifest resource name of the embedded CSV file.
    /// </summary>
    public string ResourceName { get ; }

    /// <inheritdoc />
    public IEnumerator < OfficialGattService > GetEnumerator ( ) => _dictionary.Values.GetEnumerator ( ) ;

    /// <inheritdoc />
    [ ExcludeFromCodeCoverage ]
    IEnumerator IEnumerable.GetEnumerator ( ) => GetEnumerator ( ) ;

    /// <inheritdoc />
    public int Count => _dictionary.Count ;

    /// <inheritdoc />
    public bool TryFindByUuid (
        Guid                      guid ,
        out OfficialGattService ? gattService )
    {
        gattService = null ;

        var n = guid.ToString ( "N" ) ;

        if ( n.Length < 8 )
            return false ;

        var number = n.Substring (
                                  4 ,
                                  4 ) ;

        // Avoid exception-based control flow; validate parse
        if ( ! ushort.TryParse (
                                number ,
                                NumberStyles.HexNumber ,
                                CultureInfo.InvariantCulture ,
                                out var assignedNumber ) )
            return false ;

        return _dictionary.TryGetValue (
                                        assignedNumber ,
                                        out gattService ) ;
    }

    private void Populate ( IEnumerable < OfficialGattService > records )
    {
        foreach ( var record in records ) {
            _dictionary [ record.AssignedNumber ] = record ;
        }
    }

    private static IEnumerable < OfficialGattService > ReadCsvFile ( string resourceName )
    {
        var stream = Assembly.GetExecutingAssembly ( )
                             .GetManifestResourceStream ( resourceName ) ;

        if ( stream == null ) {
            throw new ResourceNotFoundException (
                                                 resourceName ,
                                                 $"Can't find resource {resourceName}" ) ;
        }

        using var reader = new StreamReader ( stream ) ;

        var config = new CsvConfiguration ( CultureInfo.InvariantCulture ) {
                                                                               Delimiter       = "," ,
                                                                               HasHeaderRecord = true
                                                                           } ;

        using var csv = new CsvReader (
                                       reader ,
                                       config ) ;

        csv.Context.RegisterClassMap < GattServiceCsvHelperMap > ( ) ;

        var readCsvFile = csv.GetRecords < OfficialGattService > ( )
                             .ToArray ( ) ;

        return readCsvFile ;
    }
}
