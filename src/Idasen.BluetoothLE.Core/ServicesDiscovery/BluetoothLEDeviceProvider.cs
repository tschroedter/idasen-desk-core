using Windows.Devices.Bluetooth;
using Autofac.Extras.DynamicProxy;
using Idasen.Aop.Aspects;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery;

namespace Idasen.BluetoothLE.Core.ServicesDiscovery
{
    [Intercept(typeof(LogAspect))]
    public class BluetoothLEDeviceProvider : IBluetoothLEDeviceProvider
    {
        public async Task<BluetoothLEDevice?> FromBluetoothAddressAsync(ulong address)
        {
            return await BluetoothLEDevice.FromBluetoothAddressAsync(address);
        }
    }
}
