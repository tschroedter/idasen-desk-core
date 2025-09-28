namespace Idasen.BluetoothLE.Characteristics.Tests.Characteristics.Factories.ConstructorNullTester ;

using BluetoothLE.Characteristics.Characteristics ;
using BluetoothLE.Common.Tests ;

[ TestClass ]
public class CharacteristicBaseFactoryConstructorNullTester
    : BaseConstructorNullTester < CharacteristicBaseFactory >
{
    public override int NumberOfConstructorsPassed { get ; } = 1 ;
}
