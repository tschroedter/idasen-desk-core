namespace Idasen.BluetoothLE.Core.Tests.ServicesDiscovery.ConstructorNullTesters ;

using Common.Tests ;
using Core.ServicesDiscovery ;

[ TestClass ]
public class GattServicesDictionaryConstructorNullTests
    : BaseConstructorNullTester < GattServicesDictionary >
{
    public override int NumberOfConstructorsPassed { get ; } = 0 ;
}
