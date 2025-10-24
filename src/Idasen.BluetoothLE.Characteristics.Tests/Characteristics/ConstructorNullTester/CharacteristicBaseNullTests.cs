using Idasen.BluetoothLE.Common.Tests ;

namespace Idasen.BluetoothLE.Characteristics.Tests.Characteristics.ConstructorNullTester ;

[ TestClass ]
public class CharacteristicBaseNullTests
    : BaseConstructorNullTester < TestCharacteristicBase >
{
    public override int NumberOfConstructorsPassed => 1 ;
    public override int NumberOfConstructorsFailed => 0 ;
}