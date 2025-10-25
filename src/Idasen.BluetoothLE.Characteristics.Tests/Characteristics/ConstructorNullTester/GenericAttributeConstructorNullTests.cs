using Idasen.BluetoothLE.Characteristics.Characteristics ;
using Idasen.BluetoothLE.Common.Tests ;
using JetBrains.Annotations ;

namespace Idasen.BluetoothLE.Characteristics.Tests.Characteristics.ConstructorNullTester ;

[ TestClass ]
[ UsedImplicitly ]
public class GenericAttributeConstructorNullTests
    : BaseConstructorNullTester < GenericAttributeService >
{
    public override int NumberOfConstructorsPassed => 1 ;
    public override int NumberOfConstructorsFailed => 0 ;
}
