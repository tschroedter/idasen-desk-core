using Idasen.BluetoothLE.Common.Tests ;

namespace Idasen.BluetoothLE.Linak.Tests.ConstructorNullTester ;

[ TestClass ]
public class DeskHeightAndSpeedConstructorNullTests
    : BaseConstructorNullTester < DeskHeightAndSpeed >
{
    public DeskHeightAndSpeedConstructorNullTests()
        : base(1, 0) // Pass default values for the base class constructor
    {
    }
}
