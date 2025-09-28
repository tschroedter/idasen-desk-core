namespace Idasen.BluetoothLE.Characteristics.Tests.Characteristics.ConstructorNullTester ;

using BluetoothLE.Characteristics.Characteristics ;
using BluetoothLE.Common.Tests ;

[ TestClass ]
public class ReferenceOutputConstructorNullTests
    : BaseConstructorNullTester < ReferenceOutput >
{
    public override int NumberOfConstructorsPassed { get ; } = 1 ;
}
