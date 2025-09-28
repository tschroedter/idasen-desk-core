namespace Idasen.BluetoothLE.Linak.Tests.ConstructorNullTester ;

using Common.Tests ;
using Linak.Control ;

[ TestClass ]
public class DeskCommandExecutorFactoryConstructorNullTests
    : BaseConstructorNullTester < DeskCommandExecutorFactory >
{
    public override int NumberOfConstructorsPassed { get ; } = 1 ;
}
