using Idasen.BluetoothLE.Common.Tests ;

namespace Idasen.BluetoothLE.Characteristics.Tests.Characteristics.ConstructorNullTester ;

[ TestClass ]
public class CharacteristicBaseNullTests
    : BaseConstructorNullTester < TestCharacteristicBase >
{
    public override int NumberOfConstructorsPassed { get ; } = 1 ;
}
