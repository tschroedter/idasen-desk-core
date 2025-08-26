using Idasen.BluetoothLE.Common.Tests ;

namespace Idasen.BluetoothLE.Linak.Tests.ConstructorNullTester
{
    [ TestClass ]
    public class DeskDetectorNullTests
        : BaseConstructorNullTester < Desk >
    {
        public override int NumberOfConstructorsPassed { get ; } = 1 ;
    }
}