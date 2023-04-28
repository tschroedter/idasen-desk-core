using System.Collections.Generic ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery ;

// ReSharper disable UnusedMemberInSuper.Global

namespace Idasen.BluetoothLE.Characteristics.Interfaces.Characteristics
{
    public interface IDpg
        : ICharacteristicBase
    {
        IEnumerable < byte > RawDpg { get ; }

        delegate IDpg Factory ( IDevice device ) ;
    }
}