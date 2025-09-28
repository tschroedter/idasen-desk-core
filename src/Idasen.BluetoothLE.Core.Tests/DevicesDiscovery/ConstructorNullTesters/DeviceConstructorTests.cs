namespace Idasen.BluetoothLE.Core.Tests.DevicesDiscovery.ConstructorNullTesters ;

using Common.Tests ;
using Core.DevicesDiscovery ;

[ TestClass ]
public class DeviceConstructorTests
    : BaseConstructorNullTester < Device >
{
    public override int NumberOfConstructorsPassed { get ; } = 2 ;
}
