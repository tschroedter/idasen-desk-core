using Idasen.BluetoothLE.Characteristics.Characteristics.Unknowns;
using Idasen.BluetoothLE.Common.Tests;

namespace Idasen.BluetoothLE.Characteristics.Tests.Characteristics.Unknowns.ConstructorNullTester;

[TestClass]
public class GenericAccessConstructorNullTester(int numberOfConstructorsPassed = 0)
    : BaseConstructorNullTester<GenericAccess>
{
    public override int NumberOfConstructorsPassed { get; } = numberOfConstructorsPassed;
}
