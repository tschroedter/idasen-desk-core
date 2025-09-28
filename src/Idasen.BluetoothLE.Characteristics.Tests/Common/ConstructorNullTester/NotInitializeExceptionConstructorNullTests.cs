namespace Idasen.BluetoothLE.Characteristics.Tests.Common.ConstructorNullTester ;

using BluetoothLE.Characteristics.Common ;
using BluetoothLE.Common.Tests ;

[ TestClass ]
public class NotInitializeExceptionConstructorNullTests
    : BaseConstructorNullTester < NotInitializeException >
{
    public override int NumberOfConstructorsPassed { get ; } = 1 ;
}
