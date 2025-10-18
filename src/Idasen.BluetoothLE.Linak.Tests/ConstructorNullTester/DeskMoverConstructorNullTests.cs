using Idasen.BluetoothLE.Common.Tests ;
using Idasen.BluetoothLE.Linak.Control ;

namespace Idasen.BluetoothLE.Linak.Tests.ConstructorNullTester ;

[ TestClass ]
public class DeskMoverConstructorNullTests
    : BaseConstructorNullTester < DeskMover >
{
    public DeskMoverConstructorNullTests()
        : base( 1 , 0 ) // Pass default values for the base class constructor
    {
    }
}
