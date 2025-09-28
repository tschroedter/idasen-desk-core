namespace Idasen.BluetoothLE.Core.Tests.ServicesDiscovery.ConstructorNullTesters ;

using Common.Tests ;
using Core.ServicesDiscovery.Wrappers ;

[ TestClass ]
public class GattCharacteristicWrapperFactoryConstructorNullTests
    : BaseConstructorNullTester < GattCharacteristicWrapperFactory >
{
    public override int NumberOfConstructorsPassed { get ; } = 1 ;
}
