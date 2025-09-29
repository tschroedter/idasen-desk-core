using System.Diagnostics.CodeAnalysis ;
using Windows.Devices.Bluetooth.GenericAttributeProfile ;

namespace Idasen.BluetoothLE.Core ;

[ ExcludeFromCodeCoverage ]
public static class GattCharacteristicPropertiesExtensions
{
    /// <summary>
    ///     Converts the <see cref="GattCharacteristicProperties" /> into a
    ///     string containing the property names separated by comma.
    /// </summary>
    /// <param name="properties">
    ///     This <see cref="GattCharacteristicProperties" /> to convert into a string.
    /// </param>
    /// <returns>
    ///     A string containing the property names separated by a comma.
    /// </returns>
    public static string ToCsv ( this GattCharacteristicProperties properties )
    {
        var list = Enum.GetValues < GattCharacteristicProperties > ( )
                       .Where ( property => ( properties & property ) == property )
                       .ToList ( ) ;

        return string.Join ( ", " ,
                             list ) ;
    }
}