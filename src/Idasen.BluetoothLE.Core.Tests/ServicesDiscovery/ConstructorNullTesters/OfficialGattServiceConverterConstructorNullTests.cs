using Idasen.BluetoothLE.Common.Tests;
using Idasen.BluetoothLE.Core.ServicesDiscovery;

namespace Idasen.BluetoothLE.Core.Tests.ServicesDiscovery.ConstructorNullTesters;

[TestClass]
public class OfficialGattServiceConverterConstructorNullTests(int numberOfConstructorsPassed = 0)
    : BaseConstructorNullTester<OfficialGattServiceConverter>
{
    public override int NumberOfConstructorsPassed { get; } = numberOfConstructorsPassed;
}
