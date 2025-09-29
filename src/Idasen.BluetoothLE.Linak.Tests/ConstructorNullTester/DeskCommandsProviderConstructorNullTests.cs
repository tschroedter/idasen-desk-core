using Idasen.BluetoothLE.Common.Tests;
using Idasen.BluetoothLE.Linak.Control;

namespace Idasen.BluetoothLE.Linak.Tests.ConstructorNullTester;

[TestClass]
public class DeskCommandsProviderConstructorNullTests(int numberOfConstructorsPassed = 0)
    : BaseConstructorNullTester<DeskCommandsProvider>
{
    public override int NumberOfConstructorsPassed { get; } = numberOfConstructorsPassed;
}
