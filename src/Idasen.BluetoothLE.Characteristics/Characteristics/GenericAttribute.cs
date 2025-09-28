namespace Idasen.BluetoothLE.Characteristics.Characteristics ;

using System.Reactive.Concurrency ;
using Core ;
using Core.Interfaces.ServicesDiscovery ;
using Interfaces.Characteristics ;
using Interfaces.Characteristics.Customs ;
using Interfaces.Common ;
using Serilog ;

/// <summary>
///     Implements the Generic Attribute service and exposes raw values.
/// </summary>
public class GenericAttribute
    : CharacteristicBase ,
      IGenericAttribute
{
    /// <summary>
    ///     Factory delegate used by DI to create instances per device.
    /// </summary>
    public delegate IGenericAttribute Factory ( IDevice device ) ;

    internal const string CharacteristicServiceChanged = "Service Changed" ;

    private readonly IAllGattCharacteristicsProvider _allGattCharacteristicsProvider ;

    public GenericAttribute (
        ILogger logger ,
        IScheduler scheduler ,
        IDevice device ,
        IGattCharacteristicsProviderFactory providerFactory ,
        IRawValueReader rawValueReader ,
        IRawValueWriter rawValueWriter ,
        ICharacteristicBaseToStringConverter toStringConverter ,
        IDescriptionToUuid descriptionToUuid ,
        IAllGattCharacteristicsProvider allGattCharacteristicsProvider )
        : base ( logger ,
                 scheduler ,
                 device ,
                 providerFactory ,
                 rawValueReader ,
                 rawValueWriter ,
                 toStringConverter ,
                 descriptionToUuid )
    {
        Guard.ArgumentNotNull ( allGattCharacteristicsProvider ,
                                nameof ( allGattCharacteristicsProvider ) ) ;

        _allGattCharacteristicsProvider = allGattCharacteristicsProvider ;
    }

    /// <summary>
    ///     Gets the UUID of the Generic Attribute GATT service.
    /// </summary>
    public override Guid GattServiceUuid { get ; } = Guid.Parse ( "00001801-0000-1000-8000-00805f9b34fb" ) ;

    /// <summary>
    ///     Gets the raw Service Changed value.
    /// </summary>
    public IEnumerable < byte > RawServiceChanged => GetValueOrEmpty ( CharacteristicServiceChanged ) ;

    /// <inheritdoc />
    protected override T WithMapping<T> ( ) where T : class
    {
        if ( _allGattCharacteristicsProvider.TryGetUuid ( CharacteristicServiceChanged ,
                                                          out Guid uuid ) )
        {
            DescriptionToUuid[CharacteristicServiceChanged] = uuid ;
        }

        return this as T ?? throw new Exception ( $"Can't cast {this} to {typeof ( T )}" ) ;
    }
}
