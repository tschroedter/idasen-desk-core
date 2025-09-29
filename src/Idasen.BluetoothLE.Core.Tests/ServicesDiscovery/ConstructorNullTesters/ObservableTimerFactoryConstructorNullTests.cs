using Idasen.BluetoothLE.Common.Tests ;

namespace Idasen.BluetoothLE.Core.Tests.ServicesDiscovery.ConstructorNullTesters ;

[ TestClass ]
public class ObservableTimerFactoryConstructorNullTests(int numberOfConstructorsPassed = 0)
    : BaseConstructorNullTester < ObservableTimerFactory >
{
    public override int NumberOfConstructorsPassed { get ; } = numberOfConstructorsPassed ;
}
