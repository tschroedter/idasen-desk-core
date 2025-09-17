using Idasen.BluetoothLE.Characteristics.Interfaces.Characteristics ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery ;

// ReSharper disable UnusedMemberInSuper.Global

namespace Idasen.BluetoothLE.Linak.Interfaces ;

/// <summary>
///     Aggregates LINAK desk characteristics and provides typed accessors.
/// </summary>
public interface IDeskCharacteristics
{
    /// <summary>
    ///     Gets the Generic Access characteristic.
    /// </summary>
    IGenericAccess GenericAccess { get ; }

    /// <summary>
    ///     Gets the Generic Attribute characteristic.
    /// </summary>
    IGenericAttribute GenericAttribute { get ; }

    /// <summary>
    ///     Gets the Reference Input characteristic.
    /// </summary>
    IReferenceInput ReferenceInput { get ; }

    /// <summary>
    ///     Gets the Reference Output characteristic.
    /// </summary>
    IReferenceOutput ReferenceOutput { get ; }

    /// <summary>
    ///     Gets the DPG (Data Processing Group) characteristic.
    /// </summary>
    IDpg Dpg { get ; }

    /// <summary>
    ///     Gets the Control characteristic.
    /// </summary>
    IControl Control { get ; }

    /// <summary>
    ///     Initializes the characteristics by discovering them on the specified device.
    /// </summary>
    /// <param name="device">The device to initialize characteristics for.</param>
    /// <returns>The current instance of <see cref="IDeskCharacteristics"/>.</returns>
    IDeskCharacteristics Initialize ( IDevice device ) ;

    /// <summary>
    ///     Refreshes all characteristics by reading their values.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task Refresh ( ) ;

    /// <summary>
    ///     Adds a characteristic with the specified key.
    /// </summary>
    /// <param name="key">The key of the characteristic to add.</param>
    /// <param name="characteristic">The characteristic instance to add.</param>
    /// <returns>
    ///     The current instance of <see cref="IDeskCharacteristics"/>.
    /// </returns>
    IDeskCharacteristics WithCharacteristics (
        DeskCharacteristicKey key ,
        ICharacteristicBase characteristic ) ;
}