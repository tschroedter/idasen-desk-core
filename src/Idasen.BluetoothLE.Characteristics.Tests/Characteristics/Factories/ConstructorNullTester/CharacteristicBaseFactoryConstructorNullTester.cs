using Idasen.BluetoothLE.Characteristics.Characteristics ;
using Idasen.BluetoothLE.Common.Tests ;
using JetBrains.Annotations ;

namespace Idasen.BluetoothLE.Characteristics.Tests.Characteristics.Factories.ConstructorNullTester ;

[ TestClass ]
[ UsedImplicitly ]
public class CharacteristicBaseFactoryConstructorNullTester
    : BaseConstructorNullTester < CharacteristicBaseFactory >
{
    public override int NumberOfConstructorsPassed => 1 ;
    public override int NumberOfConstructorsFailed => 0 ;
}
