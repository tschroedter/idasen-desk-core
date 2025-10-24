using Idasen.BluetoothLE.Common.Tests ;
using Idasen.BluetoothLE.Core.DevicesDiscovery ;

namespace Idasen.BluetoothLE.Core.Tests.DevicesDiscovery.ConstructorNullTesters ;

[ TestClass ]
public class DeviceConstructorTests
    : BaseConstructorNullTester < Device >
{
    public override int NumberOfConstructorsPassed => 2 ;
    public override int NumberOfConstructorsFailed => 0 ;
}
