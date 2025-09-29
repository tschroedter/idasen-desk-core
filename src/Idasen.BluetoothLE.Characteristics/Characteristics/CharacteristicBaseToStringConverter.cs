using System.Text ;
using Autofac.Extras.DynamicProxy ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Characteristics.Common ;
using Idasen.BluetoothLE.Characteristics.Interfaces.Characteristics ;
using Idasen.BluetoothLE.Core ;

namespace Idasen.BluetoothLE.Characteristics.Characteristics ;

/// <summary>
///     Default implementation of <see cref="ICharacteristicBaseToStringConverter" /> that renders
///     a characteristic's known keys, their raw values (in hex), and available properties.
/// </summary>
/// <inheritdoc cref="ICharacteristicBaseToStringConverter" />
[ Intercept ( typeof ( LogAspect ) ) ]
public class CharacteristicBaseToStringConverter
    : ICharacteristicBaseToStringConverter
{
    internal static readonly IEnumerable < byte > RawArrayEmpty = Enumerable.Empty < byte > ( )
                                                                            .ToArray ( ) ;

    /// <inheritdoc />
    public string ToString ( CharacteristicBase characteristic )
    {
        Guard.ArgumentNotNull ( characteristic ,
                                nameof ( characteristic ) ) ;

        var builder = new StringBuilder ( ) ;

        builder.AppendLine ( $"{characteristic.GetType ( ).Name}" ) ;

        foreach ( var key in characteristic.DescriptionToUuid.Keys )
        {
            var value = TryGetValueOrEmpty ( characteristic ,
                                             key ) ;

            var rawValueOrUnavailable = RawValueOrUnavailable ( characteristic ,
                                                                key ,
                                                                value ) ;

            builder.Append ( rawValueOrUnavailable ) ;

            if ( characteristic.Characteristics != null &&
                 characteristic.Characteristics.Properties.TryGetValue ( key ,
                                                                         out var properties )
               )
                builder.AppendLine ( $" ({properties.ToCsv ( )})" ) ;
            else
                builder.AppendLine ( ) ;
        }

        return builder.ToString ( ) ;
    }

    /// <summary>
    ///     Returns the cached raw value for the given key, or an empty array if not available.
    /// </summary>
    protected static IEnumerable < byte > TryGetValueOrEmpty ( CharacteristicBase characteristic ,
                                                               string             key )
    {
        return characteristic.RawValues.GetValueOrDefault ( key ,
                                                            RawArrayEmpty ) ;
    }

    /// <summary>
    ///     Formats a line for the given key, showing either the hex-encoded value or "Unavailable" if the
    ///     characteristic is not present.
    /// </summary>
    protected static string RawValueOrUnavailable ( CharacteristicBase   characteristic ,
                                                    string               key ,
                                                    IEnumerable < byte > value )
    {
        return characteristic.Characteristics != null &&
               characteristic.Characteristics.Characteristics
                             .ContainsKey ( key )
                   ? $"{key} = [{value.ToHex ( )}]"
                   : $"{key} = Unavailable" ;
    }
}