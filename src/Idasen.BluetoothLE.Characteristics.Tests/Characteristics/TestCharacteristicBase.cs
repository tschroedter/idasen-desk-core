using System.Reactive.Concurrency ;
using Idasen.BluetoothLE.Characteristics.Characteristics ;
using Idasen.BluetoothLE.Characteristics.Interfaces.Characteristics ;
using Idasen.BluetoothLE.Characteristics.Interfaces.Characteristics.Customs ;
using Idasen.BluetoothLE.Characteristics.Interfaces.Common ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery ;
using Serilog ;

namespace Idasen.BluetoothLE.Characteristics.Tests.Characteristics ;

public class TestCharacteristicBase ( ILogger                              logger ,
                                      IScheduler                           scheduler ,
                                      IDevice                              device ,
                                      IGattCharacteristicsProviderFactory  providerFactory ,
                                      IRawValueReader                      rawValueReader ,
                                      IRawValueWriter                      rawValueWriter ,
                                      ICharacteristicBaseToStringConverter toStringConverter ,
                                      IDescriptionToUuid                   descriptionToUuid )
    : CharacteristicBase ( logger ,
                           scheduler ,
                           device ,
                           providerFactory ,
                           rawValueReader ,
                           rawValueWriter ,
                           toStringConverter ,
                           descriptionToUuid )
{
    public delegate ITestCharacteristicBase Factory ( IDevice device ) ;

    public const    string RawValueKey = "RawValueKey" ;
    public override Guid   GattServiceUuid { get ; } = Guid.Parse ( "11111111-1111-1111-1111-111111111111" ) ;

    public IEnumerable < byte > RawValue => GetValueOrEmpty ( RawValueKey ) ;

    public async Task < bool > TryWriteRawValue ( IEnumerable < byte > bytes )
    {
        return await TryWriteValueAsync ( RawValueKey ,
                                          bytes ) ;
    }

    protected override T WithMapping < T > ( )
        where T : class
    {
        DescriptionToUuid [ RawValueKey ] = Guid.Parse ( "22222222-2222-2222-2222-222222222222" ) ;

        return this as T ?? throw new InvalidCastException ( $"Can't cast {this} to {typeof ( T )}" ) ;
    }
}
