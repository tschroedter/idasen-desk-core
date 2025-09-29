using Idasen.BluetoothLE.Common.Tests ;
using Idasen.BluetoothLE.Core.ServicesDiscovery ;

namespace Idasen.BluetoothLE.Core.Tests.ServicesDiscovery.ConstructorNullTesters ;

[ TestClass ]
public class OfficialGattServicesConstructorNullTests ( int numberOfConstructorsPassed = 0 )
    : BaseConstructorNullTester < OfficialGattServicesCollection >
{
    public override int NumberOfConstructorsPassed { get ; } = numberOfConstructorsPassed ;
}