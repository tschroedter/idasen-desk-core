using Idasen.BluetoothLE.Common.Tests;
using Idasen.BluetoothLE.Core.ServicesDiscovery.Wrappers;

namespace Idasen.BluetoothLE.Core.Tests.ServicesDiscovery.Wrapper.ConstructorNullTesters;

[TestClass]
public class GattCharacteristicValueChangedObservablesConstructorNullTests
    : BaseConstructorNullTester<GattCharacteristicValueChangedObservables>
{
    public override int NumberOfConstructorsPassed { get; } = 1;
}
