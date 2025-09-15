using FluentAssertions;
using Idasen.BluetoothLE.Characteristics.Characteristics;
using Idasen.BluetoothLE.Characteristics.Common;
using Microsoft.Reactive.Testing;
using NSubstitute;
using Serilog;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace Idasen.BluetoothLE.Characteristics.Tests.Characteristics
{
    [TestClass]
    public class GattCharacteristicsResultWrapperTests
    {
        [TestMethod]
        public async Task Initialize_UnsuccessfulStatus_SetsEmptyCharacteristics()
        {
            // GattCharacteristicsResult is a sealed WinRT type and cannot be proxied/substituted.
            // This test requires a real instance or an adapter to mock. Marking inconclusive for now.
            Assert.Inconclusive("Cannot mock sealed WinRT type GattCharacteristicsResult. Provide an adapter or helper to create a real instance for testing.");
        }
    }
}
