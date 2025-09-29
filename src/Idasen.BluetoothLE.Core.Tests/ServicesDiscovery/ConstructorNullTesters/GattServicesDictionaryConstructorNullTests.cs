using Idasen.BluetoothLE.Common.Tests ;
using Idasen.BluetoothLE.Core.ServicesDiscovery ;

namespace Idasen.BluetoothLE.Core.Tests.ServicesDiscovery.ConstructorNullTesters ;

[ TestClass ]
public class GattServicesDictionaryConstructorNullTests(int numberOfConstructorsPassed = 0)
    : BaseConstructorNullTester < GattServices >
{
    public override int NumberOfConstructorsPassed { get ; } = numberOfConstructorsPassed ;
}
