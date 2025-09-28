using Windows.Devices.Bluetooth.GenericAttributeProfile ;
using Autofac.Extras.DynamicProxy ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery.Wrappers ;

namespace Idasen.BluetoothLE.Core.ServicesDiscovery.Wrappers ;

/// <inheritdoc />
[ Intercept ( typeof ( LogAspect ) ) ]
public class GattReadResultWrapperFactory
    : IGattReadResultWrapperFactory
{
    private readonly GattReadResultWrapper.Factory _factory ;

    public GattReadResultWrapperFactory ( GattReadResultWrapper.Factory factory )
    {
        Guard.ArgumentNotNull (
                               factory ,
                               nameof ( factory ) ) ;

        _factory = factory ;
    }

    /// <inheritdoc />
    public IGattReadResultWrapper Create ( GattReadResult result )
    {
        Guard.ArgumentNotNull (
                               result ,
                               nameof ( result ) ) ;

        return _factory ( result ) ;
    }
}
