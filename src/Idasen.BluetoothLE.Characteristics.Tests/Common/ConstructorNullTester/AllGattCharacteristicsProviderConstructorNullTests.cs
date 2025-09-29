using Idasen.BluetoothLE.Characteristics.Common;
using Idasen.BluetoothLE.Common.Tests;

namespace Idasen.BluetoothLE.Characteristics.Tests.Common.ConstructorNullTester;

[TestClass]
public class AllGattCharacteristicsProviderConstructorNullTests(int numberOfConstructorsPassed = 0)
    : BaseConstructorNullTester<AllGattCharacteristicsProvider>
{
    public override int NumberOfConstructorsPassed { get; } = numberOfConstructorsPassed;
}
