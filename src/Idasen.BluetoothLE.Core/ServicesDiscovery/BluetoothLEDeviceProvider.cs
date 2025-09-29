using Autofac.Extras.DynamicProxy;
using Idasen.Aop.Aspects;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery;
using Windows.Devices.Bluetooth;

namespace Idasen.BluetoothLE.Core.ServicesDiscovery;

[Intercept(typeof(LogAspect))]
// ReSharper disable once InconsistentNaming
public class BluetoothLEDeviceProvider : IBluetoothLEDeviceProvider
{
    public async Task<BluetoothLEDevice?> FromBluetoothAddressAsync(ulong address) =>
        await BluetoothLEDevice.FromBluetoothAddressAsync(address);
}
