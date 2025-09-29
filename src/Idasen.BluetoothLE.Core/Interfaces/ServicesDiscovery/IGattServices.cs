using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery.Wrappers;

namespace Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery;

/// <summary>
///     Thread-safe mapping between GATT device services and their characteristics.
/// </summary>
public interface IGattServices
    : IDisposable
{
    /// <summary>
    ///     Gets a read-only view of the underlying dictionary.
    /// </summary>
    IReadOnlyDictionary<IGattDeviceServiceWrapper, IGattCharacteristicsResultWrapper> ReadOnlyDictionary { get; }

    /// <summary>
    ///     Gets or sets the characteristics result for the specified service.
    /// </summary>
    IGattCharacteristicsResultWrapper this[IGattDeviceServiceWrapper service] { get; set; }

    /// <summary>
    ///     Clears the mapping and disposes entries.
    /// </summary>
    void Clear();
}
