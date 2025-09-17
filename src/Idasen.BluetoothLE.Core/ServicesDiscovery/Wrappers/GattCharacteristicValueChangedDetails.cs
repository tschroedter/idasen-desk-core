namespace Idasen.BluetoothLE.Core.ServicesDiscovery.Wrappers ;

/// <summary>
///     Details for a GATT characteristic value change event.
/// </summary>
public class GattCharacteristicValueChangedDetails
{
    /// <summary>
    ///     Initializes a new instance with the characteristic UUID, value, and timestamp.
    /// </summary>
    /// <param name="uuid">The UUID of the characteristic.</param>
    /// <param name="value">The raw value of the characteristic.</param>
    /// <param name="timestamp">The time at which the value was changed.</param>
    public GattCharacteristicValueChangedDetails (
        Guid uuid ,
        IEnumerable < byte > value ,
        DateTimeOffset timestamp )
    {
        Guard.ArgumentNotNull ( value ,
                                nameof ( value ) ) ;

        Uuid = uuid ;
        Value = value ;
        Timestamp = timestamp ;
    }

    /// <summary>
    ///     Gets the characteristic UUID.
    /// </summary>
    public Guid Uuid { get ; }

    /// <summary>
    ///     Gets the raw characteristic value.
    /// </summary>
    public IEnumerable < byte > Value { get ; }

    /// <summary>
    ///     Gets the timestamp associated with the value change.
    /// </summary>
    public DateTimeOffset Timestamp { get ; }
}