namespace Idasen.BluetoothLE.Characteristics.Tests.Characteristics.Customs.ConstructorNullTester ;

using BluetoothLE.Characteristics.Characteristics.Customs ;
using BluetoothLE.Common.Tests ;

[ TestClass ]
public class GattCharacteristicProviderConstructorNullTester
    : BaseConstructorNullTester < GattCharacteristicProvider >
{
    public override int NumberOfConstructorsPassed { get ; } = 1 ;
}
