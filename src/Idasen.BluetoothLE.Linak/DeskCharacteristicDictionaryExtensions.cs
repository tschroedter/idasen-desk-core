using Idasen.BluetoothLE.Characteristics.Characteristics.Unknowns ;
using Idasen.BluetoothLE.Characteristics.Interfaces.Characteristics ;
using JetBrains.Annotations ;

namespace Idasen.BluetoothLE.Linak ;

/// <summary>
///     Helper methods for accessing desk characteristics from a dictionary with safe fallbacks.
/// </summary>
public static class DeskCharacteristicDictionaryExtensions
{
    /// <summary>
    ///     A map of default unknown characteristic instances used when a specific characteristic is missing.
    ///     Kept for backward compatibility; prefer the internal factories in this class.
    /// </summary>
    [ UsedImplicitly ]
    public static readonly IReadOnlyDictionary < DeskCharacteristicKey , ICharacteristicBase > UnknownBases =
        new Dictionary < DeskCharacteristicKey , ICharacteristicBase >
        {
            { DeskCharacteristicKey.GenericAccess , new GenericAccess ( ) } ,
            { DeskCharacteristicKey.GenericAttribute , new GenericAttributeService ( ) } ,
            { DeskCharacteristicKey.ReferenceInput , new ReferenceInput ( ) } ,
            { DeskCharacteristicKey.ReferenceOutput , new ReferenceOutput ( ) } ,
            { DeskCharacteristicKey.Dpg , new Dpg ( ) } ,
            { DeskCharacteristicKey.Control , new Characteristics.Characteristics.Unknowns.Control ( ) }
        } ;

    private static readonly Dictionary < DeskCharacteristicKey , Func < ICharacteristicBase > >
        UnknownFactories =
            new( )
            {
                { DeskCharacteristicKey.GenericAccess , static ( ) => new GenericAccess ( ) } ,
                { DeskCharacteristicKey.GenericAttribute , static ( ) => new GenericAttributeService ( ) } ,
                { DeskCharacteristicKey.ReferenceInput , static ( ) => new ReferenceInput ( ) } ,
                { DeskCharacteristicKey.ReferenceOutput , static ( ) => new ReferenceOutput ( ) } ,
                { DeskCharacteristicKey.Dpg , static ( ) => new Dpg ( ) } ,
                {
                    DeskCharacteristicKey.Control , static ( ) =>
                                                        new Characteristics.Characteristics.Unknowns.Control ( )
                }
            } ;

    /// <summary>
    ///     Attempts to retrieve a typed characteristic from the dictionary. Falls back to an unknown instance if not present.
    /// </summary>
    /// <typeparam name="T">The expected characteristic interface type.</typeparam>
    /// <param name="dictionary">The dictionary of available characteristics.</param>
    /// <param name="key">The characteristic key to look up.</param>
    /// <returns>The characteristic instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the dictionary is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the key is invalid or not recognized.</exception>
    /// <exception cref="InvalidCastException">Thrown when the characteristic does not implement the requested type.</exception>
    public static T As < T > ( this IReadOnlyDictionary < DeskCharacteristicKey , ICharacteristicBase > dictionary ,
                               DeskCharacteristicKey                                                    key )
    {
        ArgumentNullException.ThrowIfNull ( dictionary ) ;

        if ( key == DeskCharacteristicKey.None )
            throw new ArgumentOutOfRangeException ( nameof ( key ) ,
                                                    key ,
                                                    "Key must not be None." ) ;

        if ( dictionary.TryGetValue ( key ,
                                      out var characteristicBase ) )
        {
            if ( characteristicBase is T typed )
                return typed ;

            throw new
                InvalidCastException ( $"Characteristic for key '{key}' is not of type {typeof ( T ).Name} (actual: {characteristicBase.GetType ( ).Name})." ) ;
        }

        if ( UnknownFactories.TryGetValue ( key ,
                                            out var factory ) )
        {
            var unknown = factory ( ) ;

            if ( unknown is T typedUnknown )
                return typedUnknown ;

            throw new
                InvalidCastException ( $"Unknown characteristic for key '{key}' is not of type {typeof ( T ).Name} (actual: {unknown.GetType ( ).Name})." ) ;
        }

        throw new ArgumentOutOfRangeException ( nameof ( key ) ,
                                                key ,
                                                "Unknown characteristic key." ) ;
    }
}
