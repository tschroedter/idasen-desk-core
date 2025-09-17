using Idasen.BluetoothLE.Characteristics.Characteristics.Unknowns ;
using Idasen.BluetoothLE.Characteristics.Interfaces.Characteristics ;

namespace Idasen.BluetoothLE.Linak ;

/// <summary>
///     Helper methods for accessing desk characteristics from a dictionary with safe fallbacks.
/// </summary>
public static class DeskCharacteristicDictionaryExtensions
{
    /// <summary>
    ///     A map of default unknown characteristic instances used when a specific characteristic is missing.
    /// </summary>
    public static readonly Dictionary < DeskCharacteristicKey , ICharacteristicBase > UnknownBases =
        new ( )
        {
            { DeskCharacteristicKey.GenericAccess , new GenericAccess ( ) } ,
            { DeskCharacteristicKey.GenericAttribute , new GenericAttribute ( ) } ,
            { DeskCharacteristicKey.ReferenceInput , new ReferenceInput ( ) } ,
            { DeskCharacteristicKey.ReferenceOutput , new ReferenceOutput ( ) } ,
            { DeskCharacteristicKey.Dpg , new Dpg ( ) } ,
            { DeskCharacteristicKey.Control , new Characteristics.Characteristics.Unknowns.Control ( ) }
        } ;

    /// <summary>
    ///     Attempts to retrieve a typed characteristic from the dictionary. Falls back to an unknown instance if not present.
    /// </summary>
    /// <typeparam name="T">The expected characteristic interface type.</typeparam>
    /// <param name="dictionary">The dictionary of available characteristics.</param>
    /// <param name="key">The characteristic key to look up.</param>
    /// <returns>The characteristic instance.</returns>
    /// <exception cref="ArgumentException">Thrown when the key is not recognized.</exception>
    public static T As<T> (
        this Dictionary < DeskCharacteristicKey , ICharacteristicBase > dictionary ,
        DeskCharacteristicKey key )
    {
        if ( dictionary.TryGetValue ( key ,
                                      out var characteristicBase ) )
        {
            return ( T ) characteristicBase ;
        }

        if ( UnknownBases.TryGetValue ( key ,
                                        out characteristicBase ) )
        {
            return ( T ) characteristicBase ;
        }

        throw new ArgumentException ( "" ,
                                      nameof ( key ) ) ;
    }
}