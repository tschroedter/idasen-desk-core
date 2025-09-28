namespace Idasen.BluetoothLE.Characteristics.Tests.Characteristics.ConstructorNullTester ;

using BluetoothLE.Common.Tests ;

[ TestClass ]
public class CharacteristicBaseNullTests
    : BaseConstructorNullTester < TestCharacteristicBase >
{
    public override int NumberOfConstructorsPassed { get ; } = 1 ;
}
