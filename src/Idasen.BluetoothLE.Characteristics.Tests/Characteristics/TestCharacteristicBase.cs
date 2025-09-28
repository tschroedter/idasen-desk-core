namespace Idasen.BluetoothLE.Characteristics.Tests.Characteristics ;

using System.Reactive.Concurrency ;
using BluetoothLE.Characteristics.Characteristics ;
using Core.Interfaces.ServicesDiscovery ;
using Interfaces.Characteristics ;
using Interfaces.Characteristics.Customs ;
using Interfaces.Common ;
using Serilog ;

public class TestCharacteristicBase ( ILogger logger ,
                                      IScheduler scheduler ,
                                      IDevice device ,
                                      IGattCharacteristicsProviderFactory providerFactory ,
                                      IRawValueReader rawValueReader ,
                                      IRawValueWriter rawValueWriter ,
                                      ICharacteristicBaseToStringConverter toStringConverter ,
                                      IDescriptionToUuid descriptionToUuid )
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

    public const string RawValueKey = "RawValueKey" ;
    public override Guid GattServiceUuid { get ; } = Guid.Parse ( "11111111-1111-1111-1111-111111111111" ) ;

    public IEnumerable < byte > RawValue => GetValueOrEmpty ( RawValueKey ) ;

    public async Task < bool > TryWriteRawValue ( IEnumerable < byte > bytes )
    {
        return await TryWriteValueAsync ( RawValueKey ,
                                          bytes ) ;
    }

    protected override T WithMapping<T> ( ) where T : class
    {
        DescriptionToUuid[RawValueKey] = Guid.Parse ( "22222222-2222-2222-2222-222222222222" ) ;

        return this as T ?? throw new Exception ( $"Can't cast {this} to {typeof ( T )}" ) ;
    }
}
