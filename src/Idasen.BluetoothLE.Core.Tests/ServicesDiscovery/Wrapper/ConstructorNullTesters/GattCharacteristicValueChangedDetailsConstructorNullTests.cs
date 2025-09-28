using Idasen.BluetoothLE.Common.Tests ;
using Idasen.BluetoothLE.Core.ServicesDiscovery.Wrappers ;

namespace Idasen.BluetoothLE.Core.Tests.ServicesDiscovery.Wrapper.ConstructorNullTesters ;

[ TestClass ]
public class GattCharacteristicValueChangedDetailsConstructorNullTests
    : BaseConstructorNullTester < GattCharacteristicValueChangedDetails >
{
    public override int NumberOfConstructorsPassed { get ; } = 1 ;
}
