namespace Idasen.BluetoothLE.Characteristics.Tests.Characteristics.Customs.ConstructorNullTester ;

using BluetoothLE.Characteristics.Characteristics.Customs ;
using BluetoothLE.Common.Tests ;

[ TestClass ]
public class GattCharacteristicsProviderFactoryConstructorNullTester
    : BaseConstructorNullTester < GattCharacteristicsProviderFactory >
{
    public override int NumberOfConstructorsPassed { get ; } = 1 ;
}
