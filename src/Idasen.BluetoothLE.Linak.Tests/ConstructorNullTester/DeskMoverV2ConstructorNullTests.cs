using Idasen.BluetoothLE.Common.Tests ;
using Idasen.BluetoothLE.Linak.Control ;

namespace Idasen.BluetoothLE.Linak.Tests.ConstructorNullTester ;

[ TestClass ]
public class DeskMoverV2ConstructorNullTests ( int numberOfConstructorsPassed = 1 ,
                                               int numberOfConstructorsFailed = 0 )
    : BaseConstructorNullTester < DeskMoverV2 >
{
    public override int NumberOfConstructorsPassed { get ; } = numberOfConstructorsPassed ;
    public override int NumberOfConstructorsFailed { get ; } = numberOfConstructorsFailed ;
}
