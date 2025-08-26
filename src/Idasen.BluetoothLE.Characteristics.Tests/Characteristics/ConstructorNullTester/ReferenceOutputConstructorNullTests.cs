using Idasen.BluetoothLE.Characteristics.Characteristics ;
using Idasen.BluetoothLE.Common.Tests ;

namespace Idasen.BluetoothLE.Characteristics.Tests.Characteristics.ConstructorNullTester
{
    [ TestClass ]
    public class ReferenceOutputConstructorNullTests
        : BaseConstructorNullTester < ReferenceOutput >
    {
        public override int NumberOfConstructorsPassed { get ; } = 1 ;
    }
}