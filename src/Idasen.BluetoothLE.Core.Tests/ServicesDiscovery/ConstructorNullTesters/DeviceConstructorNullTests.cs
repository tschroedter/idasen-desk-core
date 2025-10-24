using Idasen.BluetoothLE.Common.Tests ;
using Idasen.BluetoothLE.Core.ServicesDiscovery ;

namespace Idasen.BluetoothLE.Core.Tests.ServicesDiscovery.ConstructorNullTesters ;

[ TestClass ]
public class DeviceConstructorNullTests
    : BaseConstructorNullTester < Device >
{
    public override int NumberOfConstructorsPassed => 1;
    public override int NumberOfConstructorsFailed => 0;
}
