// ReSharper disable UnusedMemberInSuper.Global

using Windows.Devices.Bluetooth.GenericAttributeProfile ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery.Wrappers ;

namespace Idasen.BluetoothLE.Characteristics.Interfaces.Characteristics.Customs ;

/// <summary>
///     Organizes discovered GATT characteristics and exposes their properties by friendly keys.
/// </summary>
public interface IGattCharacteristicProvider
{
    /// <summary>
    ///     Gets the mapping of description keys to characteristic wrappers.
    /// </summary>
    IReadOnlyDictionary < string , IGattCharacteristicWrapper > Characteristics { get ; }

    /// <summary>
    ///     Gets the list of keys that were not found on the device during discovery.
    /// </summary>
    IReadOnlyCollection < string > UnavailableCharacteristics { get ; }

    /// <summary>
    ///     Gets the characteristic property flags per key.
    /// </summary>
    IReadOnlyDictionary < string , GattCharacteristicProperties > Properties { get ; }

    /// <summary>
    ///     Refreshes the internal mapping using the specified key-to-UUID mapping.
    /// </summary>
    void Refresh ( IReadOnlyDictionary < string , Guid > customCharacteristic ) ;
}
