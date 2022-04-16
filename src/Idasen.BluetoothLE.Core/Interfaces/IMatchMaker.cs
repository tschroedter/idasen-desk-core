using System.Threading.Tasks ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery ;
using JetBrains.Annotations ;

namespace Idasen.BluetoothLE.Core.Interfaces
{
    public interface IMatchMaker
    {
        /// <summary>
        ///     Attempts to pair to BLE device by address.
        /// </summary>
        /// <param name="address">The BLE device address.</param>
        /// <returns></returns>
        [UsedImplicitly]
        Task < IDevice > PairToDeviceAsync ( ulong address ) ;
    }
}