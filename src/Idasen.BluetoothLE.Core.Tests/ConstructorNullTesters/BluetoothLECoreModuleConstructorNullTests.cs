using Idasen.BluetoothLE.Common.Tests ;

namespace Idasen.BluetoothLE.Core.Tests.ConstructorNullTesters ;

[ TestClass ]
// ReSharper disable once InconsistentNaming
public class BluetoothLECoreModuleConstructorNullTests ( )
    : BaseConstructorNullTester < BluetoothLECoreModule >
{
    public override int NumberOfConstructorsPassed => 0;
    public override int NumberOfConstructorsFailed => 0;
}
