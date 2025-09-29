using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery.Wrappers ;

namespace Idasen.BluetoothLE.Characteristics.Interfaces.Common ;

/// <summary>
///     Reads raw byte values from a GATT characteristic.
/// </summary>
public interface IRawValueReader
{
    /// <summary>
    ///     Attempts to read the current value of the specified GATT characteristic.
    /// </summary>
    /// <param name="characteristic">The characteristic to read from.</param>
    /// <returns>
    ///     A tuple where the first item indicates success, and the second item contains the value bytes when successful.
    /// </returns>
    Task < (bool , byte [ ]) > TryReadValueAsync ( IGattCharacteristicWrapper characteristic ) ;
}