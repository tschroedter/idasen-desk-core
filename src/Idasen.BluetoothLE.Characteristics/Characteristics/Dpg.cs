using System.Reactive.Concurrency ;
using Idasen.BluetoothLE.Characteristics.Interfaces.Characteristics ;
using Idasen.BluetoothLE.Characteristics.Interfaces.Characteristics.Customs ;
using Idasen.BluetoothLE.Characteristics.Interfaces.Common ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery ;
using Serilog ;

namespace Idasen.BluetoothLE.Characteristics.Characteristics ;

/// <summary>
///     Implements the DPG (desk panel) service and exposes raw value(s).
/// </summary>
public class Dpg ( ILogger                              logger ,
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
                           descriptionToUuid ) ,
      IDpg
{
    /// <summary>
    ///     Factory delegate used by DI to create instances per device.
    /// </summary>
    public delegate IDpg Factory ( IDevice device ) ;

    public const string DpgKey = "Dpg" ;

    /// <summary>
    ///     Gets the UUID of the DPG GATT service.
    /// </summary>
    public override Guid GattServiceUuid { get ; } = Guid.Parse ( "99FA0010-338A-1024-8A49-009C0215F78A" ) ;

    /// <summary>
    ///     Gets the raw DPG value.
    /// </summary>
    public IEnumerable < byte > RawDpg => GetValueOrEmpty ( DpgKey ) ;

    /// <inheritdoc />
    /// <summary>
    ///     Provides the mapping of the characteristic.
    /// </summary>
    protected override T WithMapping < T > ( )
        where T : class
    {
        DescriptionToUuid [ DpgKey ] = Guid.Parse ( "99FA0011-338A-1024-8A49-009C0215F78A" ) ;

        return this as T ?? throw new InvalidCastException ( $"Can't cast {this} to {typeof ( T )}" ) ;
    }
}