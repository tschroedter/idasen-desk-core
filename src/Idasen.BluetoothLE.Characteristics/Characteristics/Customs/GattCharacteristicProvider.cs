using Windows.Devices.Bluetooth.GenericAttributeProfile ;
using Autofac.Extras.DynamicProxy ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Characteristics.Interfaces.Characteristics.Customs ;
using Idasen.BluetoothLE.Core ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery.Wrappers ;
using Serilog ;

namespace Idasen.BluetoothLE.Characteristics.Characteristics.Customs ;

/// <summary>
///     Provides discovered GATT characteristics mapped by friendly keys.
/// </summary>
[Intercept ( typeof ( LogAspect ) ) ]
public class GattCharacteristicProvider
    : IGattCharacteristicProvider
{
    /// <summary>
    ///     Factory delegate for creating <see cref="GattCharacteristicProvider" /> instances.
    /// </summary>
    public delegate IGattCharacteristicProvider
        Factory ( IGattCharacteristicsResultWrapper gattCharacteristics ) ;

    private readonly Dictionary < string , IGattCharacteristicWrapper > _characteristics = new ( ) ;

    private readonly IGattCharacteristicsResultWrapper _gattCharacteristics ;
    private readonly ILogger _logger ;

    private readonly Dictionary < string , GattCharacteristicProperties > _properties = new ( ) ;

    private readonly List < string > _unavailable = [] ;

    /// <summary>
    ///     Initializes a new instance of the <see cref="GattCharacteristicProvider" /> class.
    /// </summary>
    /// <param name="logger">Logger used for discovery diagnostics.</param>
    /// <param name="gattCharacteristics">Discovered characteristics wrapper.</param>
    public GattCharacteristicProvider (
        ILogger logger ,
        IGattCharacteristicsResultWrapper gattCharacteristics )
    {
        Guard.ArgumentNotNull ( logger ,
                                nameof ( logger ) ) ;
        Guard.ArgumentNotNull ( gattCharacteristics ,
                                nameof ( gattCharacteristics ) ) ;

        _logger = logger ;
        _gattCharacteristics = gattCharacteristics ;
    }

    /// <inheritdoc />
    public IReadOnlyDictionary < string , IGattCharacteristicWrapper > Characteristics => _characteristics ;

    /// <inheritdoc />
    public IReadOnlyCollection < string > UnavailableCharacteristics => _unavailable ;

    /// <inheritdoc />
    public IReadOnlyDictionary < string , GattCharacteristicProperties > Properties => _properties ;

    /// <inheritdoc />
    public virtual void Refresh (
        IReadOnlyDictionary < string , Guid > customCharacteristic )
    {
        Guard.ArgumentNotNull ( customCharacteristic ,
                                nameof ( customCharacteristic ) ) ;

        _logger.Information ( "{GattCharacteristicsResultWrapper}" ,
                              _gattCharacteristics ) ;

        _characteristics.Clear ( ) ;
        _unavailable.Clear ( ) ;

        foreach (var keyValuePair in customCharacteristic)
        {
            var characteristic = _gattCharacteristics.Characteristics
                                                     .FirstOrDefault ( x => x.Uuid == keyValuePair.Value ) ;

            if ( characteristic != null )
            {
                _logger.Information ( $"Found characteristic {characteristic.Uuid} " +
                                      $"for description '{keyValuePair.Key}'" ) ;

                _characteristics[keyValuePair.Key] = characteristic ;
                _properties[keyValuePair.Key] = characteristic.CharacteristicProperties ;
            }
            else
            {
                _logger.Information ( "Did not find characteristic " +
                                      $"for description '{keyValuePair.Key}' and Uuid '{keyValuePair.Value}'" ) ;

                _unavailable.Add ( keyValuePair.Key ) ;
            }
        }
    }
}