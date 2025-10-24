using Idasen.BluetoothLE.Characteristics.Common ;
using Idasen.BluetoothLE.Common.Tests ;

namespace Idasen.BluetoothLE.Characteristics.Tests.Common.ConstructorNullTester ;

[ TestClass ]
public class RawValueReaderConstructorNullTests
    : BaseConstructorNullTester < RawValueReader>
{
    public override int NumberOfConstructorsPassed => 1;
    public override int NumberOfConstructorsFailed => 0;
}
