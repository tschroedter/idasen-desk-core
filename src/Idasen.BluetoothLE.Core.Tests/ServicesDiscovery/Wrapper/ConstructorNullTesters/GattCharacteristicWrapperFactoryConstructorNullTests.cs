namespace Idasen.BluetoothLE.Core.Tests.ServicesDiscovery.Wrapper.ConstructorNullTesters ;

using Common.Tests ;
using Core.ServicesDiscovery.Wrappers ;

[ TestClass ]
// ReSharper disable once InconsistentNaming
public class GattCharacteristicWrapperFactoryConstructorNullTests
    : BaseConstructorNullTester < GattCharacteristicWrapperFactory >
{
    public override int NumberOfConstructorsPassed { get ; } = 1 ;
}
