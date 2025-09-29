using Idasen.BluetoothLE.Common.Tests ;
using Idasen.BluetoothLE.Core.DevicesDiscovery ;

namespace Idasen.BluetoothLE.Core.Tests.DevicesDiscovery.ConstructorNullTesters ;

[ TestClass ]
public class DeviceComparerConstructorTests ( int numberOfConstructorsPassed = 0 )
    : BaseConstructorNullTester < DeviceComparer >
{
    public override int NumberOfConstructorsPassed { get ; } = numberOfConstructorsPassed ;
}