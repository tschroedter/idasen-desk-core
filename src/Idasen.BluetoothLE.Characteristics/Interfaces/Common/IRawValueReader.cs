using System.Threading.Tasks ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery.Wrappers ;

namespace Idasen.BluetoothLE.Characteristics.Interfaces.Common
{
    public interface IRawValueReader
    {
        /// <summary>
        /// </summary>
        /// <param name="characteristic"></param>
        /// <returns></returns>
        Task < (bool , byte [ ]) > TryReadValueAsync (
            IGattCharacteristicWrapper characteristic ) ;
    }
}