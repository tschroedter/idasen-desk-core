namespace Idasen.BluetoothLE.Core.ServicesDiscovery.Wrappers ;

using Windows.Devices.Bluetooth.GenericAttributeProfile ;
using Aop.Aspects ;
using Autofac.Extras.DynamicProxy ;
using Interfaces.ServicesDiscovery.Wrappers ;

/// <inheritdoc />
[ Intercept ( typeof ( LogAspect ) ) ]
public class GattReadResultWrapperFactory
    : IGattReadResultWrapperFactory
{
    private readonly GattReadResultWrapper.Factory _factory ;

    public GattReadResultWrapperFactory ( GattReadResultWrapper.Factory factory )
    {
        Guard.ArgumentNotNull ( factory ,
                                nameof ( factory ) ) ;

        _factory = factory ;
    }

    /// <inheritdoc />
    public IGattReadResultWrapper Create ( GattReadResult result )
    {
        Guard.ArgumentNotNull ( result ,
                                nameof ( result ) ) ;

        return _factory ( result ) ;
    }
}
