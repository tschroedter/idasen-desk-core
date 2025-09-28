namespace Idasen.BluetoothLE.Core.Tests.ServicesDiscovery.ConstructorNullTesters ;

using Common.Tests ;

[ TestClass ]
public class ResourceNotFoundExceptionConstructorNullTests
    : BaseConstructorNullTester < ResourceNotFoundException >
{
    public override int NumberOfConstructorsPassed { get ; } = 1 ;
}
