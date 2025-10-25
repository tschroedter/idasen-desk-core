using Idasen.BluetoothLE.Common.Tests ;
using JetBrains.Annotations ;

namespace Idasen.BluetoothLE.Characteristics.Tests.Characteristics.ConstructorNullTester ;

[ TestClass ]
[ UsedImplicitly ]
public class CharacteristicBaseNullTests
    : BaseConstructorNullTester < TestCharacteristicBase >
{
    public override int NumberOfConstructorsPassed => 1 ;
    public override int NumberOfConstructorsFailed => 0 ;
}
