namespace Idasen.BluetoothLE.Core.ServicesDiscovery ;

using Windows.Devices.Bluetooth ;
using Aop.Aspects ;
using Autofac.Extras.DynamicProxy ;
using Interfaces.ServicesDiscovery ;

[ Intercept ( typeof ( LogAspect ) ) ]
// ReSharper disable once InconsistentNaming
public class BluetoothLEDeviceProvider : IBluetoothLEDeviceProvider
{
    public async Task < BluetoothLEDevice? > FromBluetoothAddressAsync ( ulong address ) =>
        await BluetoothLEDevice.FromBluetoothAddressAsync ( address ) ;
}
