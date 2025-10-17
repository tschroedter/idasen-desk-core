using Idasen.BluetoothLE.Common.Tests ;
using Idasen.BluetoothLE.Linak.Control ;

namespace Idasen.BluetoothLE.Linak.Tests.ConstructorNullTester ;

[ TestClass ]
public class DeskCommandExecutorFactoryConstructorNullTests
    : BaseConstructorNullTester < DeskCommandExecutorFactory >
{
    public DeskCommandExecutorFactoryConstructorNullTests()
        : base( 1 , 0 ) // Pass default values for the base class constructor
    {
    }
}
