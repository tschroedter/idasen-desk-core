

// ReSharper disable UnusedMember.Global

namespace Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery.Wrappers ;

using Windows.Devices.Bluetooth.GenericAttributeProfile ;

/// <summary>
///     Wrapper for <see cref="GattCharacteristicsResult" />.
/// </summary>
public interface IGattCharacteristicsResultWrapper : IDisposable
{
    /// <summary>
    ///     Gets the status.
    /// </summary>
    GattCommunicationStatus Status { get ; }

    /// <summary>
    ///     Gets the protocol error, if there is one.
    /// </summary>
    byte? ProtocolError { get ; }

    /// <summary>
    ///     Gets the characteristics .
    /// </summary>
    IReadOnlyList < IGattCharacteristicWrapper > Characteristics { get ; }

    /// <summary>
    ///     Initialize the instance.
    /// </summary>
    /// <returns></returns>
    Task < IGattCharacteristicsResultWrapper > Initialize ( ) ;
}
