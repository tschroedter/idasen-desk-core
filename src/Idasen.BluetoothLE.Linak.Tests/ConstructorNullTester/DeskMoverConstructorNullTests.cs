using Idasen.BluetoothLE.Common.Tests;
using Idasen.BluetoothLE.Linak.Control;

namespace Idasen.BluetoothLE.Linak.Tests.ConstructorNullTester;

[TestClass]
public class DeskMoverConstructorNullTests(int numberOfConstructorsPassed = 1, int numberOfConstructorsFailed = 0)
    : BaseConstructorNullTester<DeskMover>
{
    public override int NumberOfConstructorsPassed { get; } = numberOfConstructorsPassed;
    public override int NumberOfConstructorsFailed { get; } = numberOfConstructorsFailed;
}
