using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery ;

namespace Idasen.BluetoothLE.Characteristics.Interfaces.Characteristics ;

/// <summary>
///     Creates characteristic instances with a supplied device.
/// </summary>
public interface ICharacteristicBaseFactory
{
    /// <summary>
    ///     Creates an instance of <typeparamref name="T" /> using the provided device.
    /// </summary>
    /// <typeparam name="T">The type to resolve.</typeparam>
    /// <param name="device">The device to inject.</param>
    /// <returns>An instance of <typeparamref name="T" />.</returns>
    T Create < T > ( IDevice device ) where T : notnull ;
}
