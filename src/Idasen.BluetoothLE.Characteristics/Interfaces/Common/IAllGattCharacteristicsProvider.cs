using JetBrains.Annotations ;

namespace Idasen.BluetoothLE.Characteristics.Interfaces.Common ;

/// <summary>
///     Provides lookup between official GATT characteristic UUIDs and their descriptions.
/// </summary>
public interface IAllGattCharacteristicsProvider
{
    /// <summary>
    ///     Tries to get the official description for the specified UUID.
    /// </summary>
    /// <param name="uuid">The characteristic UUID.</param>
    /// <param name="description">Outputs the description if found; otherwise an empty string.</param>
    /// <returns><c>true</c> if found; otherwise, <c>false</c>.</returns>
    [ UsedImplicitly ]
    bool TryGetDescription (
        Guid       uuid ,
        out string description ) ;

    /// <summary>
    ///     Tries to get the UUID for the specified official description.
    /// </summary>
    /// <param name="description">The characteristic description.</param>
    /// <param name="uuid">Outputs the UUID if found.</param>
    /// <returns><c>true</c> if found; otherwise, <c>false</c>.</returns>
    [ UsedImplicitly ]
    bool TryGetUuid (
        string   description ,
        out Guid uuid ) ;
}
