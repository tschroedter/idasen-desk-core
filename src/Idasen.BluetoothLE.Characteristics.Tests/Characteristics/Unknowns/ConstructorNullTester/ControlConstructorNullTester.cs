using Idasen.BluetoothLE.Characteristics.Characteristics.Unknowns ;
using Idasen.BluetoothLE.Common.Tests ;

namespace Idasen.BluetoothLE.Characteristics.Tests.Characteristics.Unknowns.ConstructorNullTester ;

[ TestClass ]
public class ControlConstructorNullTester ( )
    : BaseConstructorNullTester < Control>
{
    public override int NumberOfConstructorsPassed => 0;
    public override int NumberOfConstructorsFailed => 0;
}
