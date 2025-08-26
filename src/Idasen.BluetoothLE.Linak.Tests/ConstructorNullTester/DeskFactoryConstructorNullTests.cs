using Idasen.BluetoothLE.Common.Tests ;

namespace Idasen.BluetoothLE.Linak.Tests.ConstructorNullTester
{
    [ TestClass ]
    public class DeskFactoryConstructorNullTests
        : BaseConstructorNullTester < DeskFactory >
    {
        public override int NumberOfConstructorsPassed { get ; } = 1 ;
    }
}