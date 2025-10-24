using Idasen.BluetoothLE.Common.Tests ;
using Idasen.BluetoothLE.Core.DevicesDiscovery ;

namespace Idasen.BluetoothLE.Core.Tests.DevicesDiscovery.ConstructorNullTesters ;

[ TestClass ]
public class DevicesConstructorTests
    : BaseConstructorNullTester < Devices >
{
    public override int NumberOfConstructorsPassed => 1 ;
    public override int NumberOfConstructorsFailed => 0 ;
}