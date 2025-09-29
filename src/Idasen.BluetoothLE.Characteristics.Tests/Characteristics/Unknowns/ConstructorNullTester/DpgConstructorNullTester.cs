using Idasen.BluetoothLE.Characteristics.Characteristics.Unknowns;
using Idasen.BluetoothLE.Common.Tests;

namespace Idasen.BluetoothLE.Characteristics.Tests.Characteristics.Unknowns.ConstructorNullTester;

[TestClass]
public class DpgConstructorNullTester(int numberOfConstructorsPassed = 0)
    : BaseConstructorNullTester<Dpg>
{
    public override int NumberOfConstructorsPassed { get; } = numberOfConstructorsPassed;
}
