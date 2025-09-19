using System.Text.RegularExpressions ;

namespace Idasen.BluetoothLE.Core ;

/// <summary>
///     Extensions for <see cref="ulong" /> values.
/// </summary>
public static class ULongExtensions
{
    private const string Grouping = "(.{2})(.{2})(.{2})(.{2})(.{2})(.{2})" ;
    private const string Replace = "$1:$2:$3:$4:$5:$6" ;

    /// <summary>
    ///     Converts an ulong value into a MAC address formatted as 6 octets (AA:BB:CC:DD:EE:FF).
    ///     - Values shorter than 12 hex digits are left-padded with zeros.
    ///     - Values longer than 12 hex digits use only the least significant 12 digits (lower 48 bits).
    /// </summary>
    public static string ToMacAddress ( this ulong value )
    {
        // Take lower 48 bits to ensure 6 octets
        var lower48 = value & 0x0000FFFFFFFFFFFFUL ;

        // Upper-case, zero-padded to 12 hex digits
        var hex = lower48.ToString ( "X12" ) ;

        // Insert colons between octets
        var macAddress = Regex.Replace ( hex ,
                                         Grouping ,
                                         Replace ) ;

        return macAddress ;
    }
}