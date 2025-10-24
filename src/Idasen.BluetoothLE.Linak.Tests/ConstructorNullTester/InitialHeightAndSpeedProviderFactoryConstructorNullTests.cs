using Idasen.BluetoothLE.Common.Tests ;
using Idasen.BluetoothLE.Linak.Control ;

namespace Idasen.BluetoothLE.Linak.Tests.ConstructorNullTester ;

[ TestClass ]
public class InitialHeightAndSpeedProviderFactoryConstructorNullTests
    : BaseConstructorNullTester < InitialHeightAndSpeedProviderFactory>
{
    public override int NumberOfConstructorsPassed => 1;
    public override int NumberOfConstructorsFailed => 0;
}
