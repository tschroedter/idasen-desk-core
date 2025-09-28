using System.Text.RegularExpressions ;

namespace Idasen.BluetoothLE.Core ;

public static partial class ULongExtensions
{
    private const string Replace = "$1:$2:$3:$4:$5:$6" ;

    [ GeneratedRegex (
                         "(.{2})(.{2})(.{2})(.{2})(.{2})(.{2})" ,
                         RegexOptions.CultureInvariant | RegexOptions.Compiled ) ]
    private static partial Regex MacGroupingRegex ( ) ;

    /// <summary>
    ///     Converts an ulong value into a MAC address formatted as 6 octets (AA:BB:CC:DD:EE:FF).
    ///     - Values shorter than 12 hex digits are left-padded with zeros.
    ///     - Values longer than 12 hex digits use only the least significant 12 digits (lower 48 bits).
    /// </summary>
    public static string ToMacAddress ( this ulong value )
    {
        var lower48 = value & 0x0000FFFFFFFFFFFFUL ;
        var hex     = lower48.ToString ( "X12" ) ;
        return MacGroupingRegex ( ).Replace (
                                             hex ,
                                             Replace ) ;
    }
}
