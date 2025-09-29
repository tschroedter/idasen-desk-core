using Idasen.BluetoothLE.Characteristics.Characteristics;
using Idasen.BluetoothLE.Common.Tests;

namespace Idasen.BluetoothLE.Characteristics.Tests.Characteristics.ConstructorNullTester;

[TestClass]
public class GenericAccessConstructorNullTests
    : BaseConstructorNullTester<GenericAccess>
{
    public override int NumberOfConstructorsPassed { get; } = 1;
}
