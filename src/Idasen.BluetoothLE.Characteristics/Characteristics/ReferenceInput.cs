using System.Reactive.Concurrency ;
using Idasen.BluetoothLE.Characteristics.Interfaces.Characteristics ;
using Idasen.BluetoothLE.Characteristics.Interfaces.Characteristics.Customs ;
using Idasen.BluetoothLE.Characteristics.Interfaces.Common ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery ;
using Serilog ;

namespace Idasen.BluetoothLE.Characteristics.Characteristics ;

public class ReferenceInput ( ILogger                              logger ,
                              IScheduler                           scheduler ,
                              IDevice                              device ,
                              IGattCharacteristicsProviderFactory  providerFactory ,
                              IRawValueHandler                     valueHandler,
                              ICharacteristicBaseToStringConverter toStringConverter ,
                              IDescriptionToUuid                   descriptionToUuid )
    : CharacteristicBase ( logger ,
                           scheduler ,
                           device ,
                           providerFactory ,
                           valueHandler ,
                           toStringConverter ,
                           descriptionToUuid ) ,
      IReferenceInput
{
    public delegate IReferenceInput Factory ( IDevice device ) ;

    internal const string Ctrl1Key = "Ctrl1" ;

    public override Guid GattServiceUuid { get ; } = Guid.Parse ( "99FA0030-338A-1024-8A49-009C0215F78A" ) ;
    public          IEnumerable < byte > Ctrl1 => GetValueOrEmpty ( Ctrl1Key ) ;

    protected override T WithMapping < T > ( )
        where T : class
    {
        DescriptionToUuid [ Ctrl1Key ] = Guid.Parse ( "99FA0031-338A-1024-8A49-009C0215F78A" ) ;

        return this as T ?? throw new InvalidCastException ( $"Can't cast {this} to {typeof ( T )}" ) ;
    }
}
