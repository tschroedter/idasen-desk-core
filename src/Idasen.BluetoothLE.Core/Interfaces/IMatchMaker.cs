using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery ;
using JetBrains.Annotations ;

namespace Idasen.BluetoothLE.Core.Interfaces ;

/// <summary>
///     Pairs with a BLE device based on its address.
/// </summary>
public interface IMatchMaker
{
    /// <summary>
    ///     Attempts to pair to BLE device by address.
    /// </summary>
    /// <param name="address">The BLE device address.</param>
    /// <returns>A task that completes with the paired <see cref="IDevice" />.</returns>
    [ UsedImplicitly ]
    Task < IDevice > PairToDeviceAsync ( ulong address ) ;
}