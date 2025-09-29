using Idasen.BluetoothLE.Common.Tests ;

namespace Idasen.BluetoothLE.Core.Tests.ServicesDiscovery.ConstructorNullTesters ;

[ TestClass ]
public class DateTimeOffsetWrapperConstructorNullTests ( int numberOfConstructorsPassed = 0 )
    : BaseConstructorNullTester < DateTimeOffsetWrapper >
{
    public override int NumberOfConstructorsPassed { get ; } = numberOfConstructorsPassed ;
}