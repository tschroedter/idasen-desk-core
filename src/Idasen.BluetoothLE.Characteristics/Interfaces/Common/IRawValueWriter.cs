

// ReSharper disable UnusedMemberInSuper.Global

namespace Idasen.BluetoothLE.Characteristics.Interfaces.Common ;

using Windows.Storage.Streams ;
using Core.Interfaces.ServicesDiscovery.Wrappers ;

/// <summary>
///     Writes raw byte values to GATT characteristics.
/// </summary>
public interface IRawValueWriter
{
    /// <summary>
    ///     Attempts to write the given value to the characteristic using the default write behavior.
    /// </summary>
    /// <param name="characteristic">The characteristic to write to.</param>
    /// <param name="buffer">The value as a Windows <see cref="IBuffer" />.</param>
    /// <returns><c>true</c> on success; otherwise, <c>false</c>.</returns>
    Task < bool > TryWriteValueAsync (
        IGattCharacteristicWrapper characteristic ,
        IBuffer buffer ) ;

    /// <summary>
    ///     Attempts to write the given value using the WritableAuxiliaries capability.
    /// </summary>
    /// <param name="characteristic">The characteristic to write to.</param>
    /// <param name="buffer">The value as a Windows <see cref="IBuffer" />.</param>
    /// <returns><c>true</c> on success; otherwise, <c>false</c>.</returns>
    Task < bool > TryWritableAuxiliariesValueAsync (
        IGattCharacteristicWrapper characteristic ,
        IBuffer buffer ) ;

    /// <summary>
    ///     Writes without requiring a response and returns the detailed write result.
    /// </summary>
    /// <param name="characteristic">The characteristic to write to.</param>
    /// <param name="buffer">The value as a Windows <see cref="IBuffer" />.</param>
    /// <returns>The platform write result wrapper.</returns>
    Task < IGattWriteResultWrapper > TryWriteWithoutResponseAsync (
        IGattCharacteristicWrapper characteristic ,
        IBuffer buffer ) ;
}
