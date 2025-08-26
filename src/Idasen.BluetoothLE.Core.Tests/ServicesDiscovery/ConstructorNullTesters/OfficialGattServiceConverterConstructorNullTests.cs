using Idasen.BluetoothLE.Common.Tests ;
using Idasen.BluetoothLE.Core.ServicesDiscovery ;

namespace Idasen.BluetoothLE.Core.Tests.ServicesDiscovery.ConstructorNullTesters
{
    [ TestClass ]
    public class OfficialGattServiceConverterConstructorNullTests
        : BaseConstructorNullTester < OfficialGattServiceConverter >
    {
        public override int NumberOfConstructorsPassed { get ; } = 0 ;
    }
}