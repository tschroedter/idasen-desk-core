using Autofac.Extras.DynamicProxy;
using Idasen.Aop.Aspects;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery.Wrappers;

namespace Idasen.BluetoothLE.Core.ServicesDiscovery;

/// <inheritdoc />
[Intercept(typeof(LogAspect))]
public class GattServicesProviderFactory
    : IGattServicesProviderFactory
{
    private readonly GattServicesProvider.Factory _factory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="GattServicesProviderFactory" /> class.
    /// </summary>
    public GattServicesProviderFactory(GattServicesProvider.Factory factory)
    {
        Guard.ArgumentNotNull(
            factory,
            nameof(factory));

        _factory = factory;
    }

    /// <inheritdoc />
    /// <summary>
    ///     Creates an instance of <see cref="IGattServicesProvider" /> for the specified Bluetooth Low Energy device.
    /// </summary>
    public IGattServicesProvider Create(IBluetoothLeDeviceWrapper wrapper)
    {
        Guard.ArgumentNotNull(
            wrapper,
            nameof(wrapper));

        return _factory.Invoke(wrapper);
    }
}
