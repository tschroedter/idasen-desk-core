using Idasen.BluetoothLE.Common.Tests;
using Idasen.BluetoothLE.Core;

namespace Idasen.BluetoothLE.Characteristics.Tests.Common.ConstructorNullTester;

[TestClass]
public class ResourceNotFoundExceptionConstructorNullTests
    : BaseConstructorNullTester<ResourceNotFoundException>
{
    public override int NumberOfConstructorsPassed { get; } = 1;
}
