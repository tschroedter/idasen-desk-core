namespace Idasen.BluetoothLE.Core.Tests.ServicesDiscovery.Wrapper.ConstructorNullTesters ;

using Common.Tests ;
using Core.ServicesDiscovery.Wrappers ;

[ TestClass ]
public class GattCharacteristicValueChangedDetailsConstructorNullTests
    : BaseConstructorNullTester < GattCharacteristicValueChangedDetails >
{
    public override int NumberOfConstructorsPassed { get ; } = 1 ;
}
