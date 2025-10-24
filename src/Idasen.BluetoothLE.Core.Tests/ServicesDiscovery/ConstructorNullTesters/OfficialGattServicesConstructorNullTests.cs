using Idasen.BluetoothLE.Common.Tests ;
using Idasen.BluetoothLE.Core.ServicesDiscovery ;

namespace Idasen.BluetoothLE.Core.Tests.ServicesDiscovery.ConstructorNullTesters ;

[ TestClass ]
public class OfficialGattServicesConstructorNullTests ( )
    : BaseConstructorNullTester < OfficialGattServicesCollection>
{
    public override int NumberOfConstructorsPassed => 0;
    public override int NumberOfConstructorsFailed => 0;
}
