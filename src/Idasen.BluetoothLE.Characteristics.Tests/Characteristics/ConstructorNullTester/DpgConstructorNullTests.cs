namespace Idasen.BluetoothLE.Characteristics.Tests.Characteristics.ConstructorNullTester ;

using BluetoothLE.Characteristics.Characteristics ;
using BluetoothLE.Common.Tests ;

[ TestClass ]
public class DpgConstructorNullTests
    : BaseConstructorNullTester < Dpg >
{
    public override int NumberOfConstructorsPassed { get ; } = 1 ;
}
