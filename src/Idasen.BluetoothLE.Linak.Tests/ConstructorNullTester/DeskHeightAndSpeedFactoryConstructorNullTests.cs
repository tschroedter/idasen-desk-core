using Idasen.BluetoothLE.Common.Tests ;

namespace Idasen.BluetoothLE.Linak.Tests.ConstructorNullTester ;

[ TestClass ]
public class DeskHeightAndSpeedFactoryConstructorNullTests
    : BaseConstructorNullTester < DeskHeightAndSpeedFactory >
{
    public DeskHeightAndSpeedFactoryConstructorNullTests()
        : base(1, 0) // Pass default values for the base class constructor
    {
    }

    public override int NumberOfConstructorsPassed { get ; } = 1 ;
}
