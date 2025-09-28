namespace Idasen.BluetoothLE.Linak.Tests.ConstructorNullTester ;

using Common.Tests ;
using Linak.Control ;

[ TestClass ]
public class DeskMoverConstructorNullTests
    : BaseConstructorNullTester < DeskMover >
{
    public override int NumberOfConstructorsPassed { get ; } = 1 ;
    public override int NumberOfConstructorsFailed { get ; } = 0 ;
}
