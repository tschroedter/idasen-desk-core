using Idasen.BluetoothLE.Common.Tests ;

namespace Idasen.BluetoothLE.Linak.Tests.ConstructorNullTester ;

[ TestClass ]
public class DeskDetectorNullTests
    : BaseConstructorNullTester < Desk>
{
    public override int NumberOfConstructorsPassed => 1;
    public override int NumberOfConstructorsFailed => 0;
}
