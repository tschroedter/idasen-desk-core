namespace Idasen.BluetoothLE.Core.ServicesDiscovery.Wrappers ;

using Windows.Devices.Bluetooth.GenericAttributeProfile ;
using Aop.Aspects ;
using Autofac.Extras.DynamicProxy ;
using Interfaces.ServicesDiscovery.Wrappers ;

/// <inheritdoc />
[ Intercept ( typeof ( LogAspect ) ) ]
public class GattWriteResultWrapperFactory
    : IGattWriteResultWrapperFactory
{
    private readonly GattWriteResultWrapper.Factory _factory ;

    public GattWriteResultWrapperFactory ( GattWriteResultWrapper.Factory factory )
    {
        Guard.ArgumentNotNull ( factory ,
                                nameof ( factory ) ) ;

        _factory = factory ;
    }

    /// <inheritdoc />
    public IGattWriteResultWrapper Create ( GattWriteResult result )
    {
        Guard.ArgumentNotNull ( result ,
                                nameof ( result ) ) ;

        return _factory.Invoke ( result ) ;
    }
}
