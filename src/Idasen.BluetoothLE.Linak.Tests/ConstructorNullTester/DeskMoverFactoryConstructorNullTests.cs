namespace Idasen.BluetoothLE.Linak.Tests.ConstructorNullTester ;

using Common.Tests ;
using Linak.Control ;

[ TestClass ]
public class DeskMoverFactoryConstructorNullTests
    : BaseConstructorNullTester < DeskMoverFactory >
{
    public override int NumberOfConstructorsPassed { get ; } = 1 ;
}
