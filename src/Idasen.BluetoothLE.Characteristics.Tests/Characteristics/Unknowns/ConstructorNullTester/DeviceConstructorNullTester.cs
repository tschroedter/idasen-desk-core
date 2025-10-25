using Idasen.BluetoothLE.Characteristics.Characteristics.Unknowns ;
using Idasen.BluetoothLE.Common.Tests ;
using JetBrains.Annotations ;

namespace Idasen.BluetoothLE.Characteristics.Tests.Characteristics.Unknowns.ConstructorNullTester ;

[ TestClass ]
[ UsedImplicitly ]
public class GenericAccessConstructorNullTester
    : BaseConstructorNullTester < GenericAccess >
{
    public override int NumberOfConstructorsPassed => 0 ;
    public override int NumberOfConstructorsFailed => 0 ;
}