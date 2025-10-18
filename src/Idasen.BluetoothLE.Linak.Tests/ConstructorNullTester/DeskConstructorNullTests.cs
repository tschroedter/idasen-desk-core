using Idasen.BluetoothLE.Common.Tests ;

namespace Idasen.BluetoothLE.Linak.Tests.ConstructorNullTester ;

[ TestClass ]
public class DeskConstructorNullTests
    : BaseConstructorNullTester < Desk >
{
    public DeskConstructorNullTests()
        : base( 1 , 0 ) // Pass default values for the base class constructor
    {
    }
}
