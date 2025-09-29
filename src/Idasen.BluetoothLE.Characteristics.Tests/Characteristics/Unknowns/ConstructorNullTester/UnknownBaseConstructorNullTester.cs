using Idasen.BluetoothLE.Characteristics.Characteristics.Unknowns;
using Idasen.BluetoothLE.Common.Tests;

namespace Idasen.BluetoothLE.Characteristics.Tests.Characteristics.Unknowns.ConstructorNullTester;

[TestClass]
public class UnknownBaseConstructorNullTester(int numberOfConstructorsPassed = 0)
    : BaseConstructorNullTester<UnknownBase>
{
    public override int NumberOfConstructorsPassed { get; } = numberOfConstructorsPassed;
}
