using Idasen.BluetoothLE.Common.Tests ;
using Idasen.BluetoothLE.Linak.Control ;

namespace Idasen.BluetoothLE.Linak.Tests.ConstructorNullTester ;

[ TestClass ]
public class DeskMoverConstructorNullTests
    : BaseConstructorNullTester < DeskMover >
{
    public override int NumberOfConstructorsPassed => 1 ;
    public override int NumberOfConstructorsFailed => 0 ;
}