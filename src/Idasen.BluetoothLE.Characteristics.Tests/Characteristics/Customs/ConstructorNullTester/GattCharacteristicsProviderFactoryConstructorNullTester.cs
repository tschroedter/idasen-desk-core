using Idasen.BluetoothLE.Characteristics.Characteristics.Customs ;
using Idasen.BluetoothLE.Common.Tests ;
using JetBrains.Annotations ;

namespace Idasen.BluetoothLE.Characteristics.Tests.Characteristics.Customs.ConstructorNullTester ;

[ TestClass ]
[ UsedImplicitly ]
public class GattCharacteristicsProviderFactoryConstructorNullTester
    : BaseConstructorNullTester < GattCharacteristicsProviderFactory >
{
    public override int NumberOfConstructorsPassed => 1 ;
    public override int NumberOfConstructorsFailed => 0 ;
}