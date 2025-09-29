using System.Diagnostics.CodeAnalysis;
using Autofac.Extras.DynamicProxy;
using Idasen.Aop.Aspects;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery.Wrappers;
using Windows.Devices.Bluetooth;

namespace Idasen.BluetoothLE.Core.ServicesDiscovery.Wrappers;

/// <inheritdoc />
[ExcludeFromCodeCoverage]
[Intercept(typeof(LogAspect))]
public class BluetoothLeDeviceWrapperFactory
    : IBluetoothLeDeviceWrapperFactory
{
    private readonly BluetoothLeDeviceWrapper.Factory _factory;

    public BluetoothLeDeviceWrapperFactory(BluetoothLeDeviceWrapper.Factory factory)
    {
        Guard.ArgumentNotNull(
            factory,
            nameof(factory));

        _factory = factory;
    }

    /// <inheritdoc />
    public IBluetoothLeDeviceWrapper Create(BluetoothLEDevice device)
    {
        // With delegate factories, Autofac will resolve all other dependencies,
        // and we only need to pass the varying parameter
        return _factory(device);
    }
}
