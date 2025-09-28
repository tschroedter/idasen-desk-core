namespace Idasen.BluetoothLE.Characteristics.Tests.Common.ConstructorNullTester ;

using BluetoothLE.Common.Tests ;
using Core ;

[ TestClass ]
public class ResourceNotFoundExceptionConstructorNullTests
    : BaseConstructorNullTester < ResourceNotFoundException >
{
    public override int NumberOfConstructorsPassed { get ; } = 1 ;
}
