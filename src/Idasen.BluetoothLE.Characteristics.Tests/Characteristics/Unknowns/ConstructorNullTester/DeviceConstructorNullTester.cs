namespace Idasen.BluetoothLE.Characteristics.Tests.Characteristics.Unknowns.ConstructorNullTester ;

using BluetoothLE.Characteristics.Characteristics.Unknowns ;
using BluetoothLE.Common.Tests ;

[ TestClass ]
public class GenericAccessConstructorNullTester
    : BaseConstructorNullTester < GenericAccess >
{
    public override int NumberOfConstructorsPassed { get ; } = 0 ;
}
