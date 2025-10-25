using Idasen.BluetoothLE.Common.Tests ;
using Idasen.BluetoothLE.Core.ServicesDiscovery.Wrappers ;

namespace Idasen.BluetoothLE.Core.Tests.ServicesDiscovery.Wrapper.ConstructorNullTesters ;

[ TestClass ]
// ReSharper disable once InconsistentNaming
public class GattCharacteristicWrapperFactoryConstructorNullTests
    : BaseConstructorNullTester < GattCharacteristicWrapperFactory >
{
    public override int NumberOfConstructorsPassed => 1 ;
    public override int NumberOfConstructorsFailed => 0 ;
}