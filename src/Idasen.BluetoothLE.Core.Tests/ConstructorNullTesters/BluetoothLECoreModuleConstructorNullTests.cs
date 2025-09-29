using Idasen.BluetoothLE.Common.Tests ;

namespace Idasen.BluetoothLE.Core.Tests.ConstructorNullTesters ;

[ TestClass ]
// ReSharper disable once InconsistentNaming
public class BluetoothLECoreModuleConstructorNullTests ( int numberOfConstructorsPassed = 0 )
    : BaseConstructorNullTester < BluetoothLECoreModule >
{
    public override int NumberOfConstructorsPassed { get ; } = numberOfConstructorsPassed ;
}