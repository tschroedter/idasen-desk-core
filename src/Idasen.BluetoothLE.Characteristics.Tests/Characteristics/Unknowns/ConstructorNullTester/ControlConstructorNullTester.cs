using Idasen.BluetoothLE.Characteristics.Characteristics.Unknowns;
using Idasen.BluetoothLE.Common.Tests;

namespace Idasen.BluetoothLE.Characteristics.Tests.Characteristics.Unknowns.ConstructorNullTester;

[TestClass]
public class ControlConstructorNullTester(int numberOfConstructorsPassed = 0)
    : BaseConstructorNullTester<Control>
{
    public override int NumberOfConstructorsPassed { get; } = numberOfConstructorsPassed;
}
