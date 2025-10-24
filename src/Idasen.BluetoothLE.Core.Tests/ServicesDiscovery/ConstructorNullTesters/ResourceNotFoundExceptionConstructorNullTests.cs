using Idasen.BluetoothLE.Common.Tests ;

namespace Idasen.BluetoothLE.Core.Tests.ServicesDiscovery.ConstructorNullTesters ;

[ TestClass ]
public class ResourceNotFoundExceptionConstructorNullTests
    : BaseConstructorNullTester < ResourceNotFoundException >
{
    public override int NumberOfConstructorsPassed => 1 ;
    public override int NumberOfConstructorsFailed => 0 ;
}