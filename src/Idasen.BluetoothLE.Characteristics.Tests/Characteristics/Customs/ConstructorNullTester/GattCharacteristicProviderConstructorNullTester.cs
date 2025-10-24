using Idasen.BluetoothLE.Characteristics.Characteristics.Customs ;
using Idasen.BluetoothLE.Common.Tests ;

namespace Idasen.BluetoothLE.Characteristics.Tests.Characteristics.Customs.ConstructorNullTester ;

[ TestClass ]
public class GattCharacteristicProviderConstructorNullTester
    : BaseConstructorNullTester < GattCharacteristicProvider>
{
    public override int NumberOfConstructorsPassed => 1;
    public override int NumberOfConstructorsFailed => 0;
}
