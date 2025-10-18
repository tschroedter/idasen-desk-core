using Idasen.BluetoothLE.Common.Tests ;
using Idasen.BluetoothLE.Linak.Control ;

namespace Idasen.BluetoothLE.Linak.Tests.ConstructorNullTester ;

[ TestClass ]
public class DeskCommandsProviderConstructorNullTests
    : BaseConstructorNullTester < DeskCommandsProvider >
{
    public DeskCommandsProviderConstructorNullTests()
        : base(0, 0) // Pass default values for the base class constructor
    {
    }

    public override int NumberOfConstructorsPassed { get ; }
}
