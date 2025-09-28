namespace Idasen.BluetoothLE.Core.ServicesDiscovery.Wrappers ;

using System.Diagnostics.CodeAnalysis ;
using Windows.Devices.Bluetooth.GenericAttributeProfile ;
using Aop.Aspects ;
using Autofac.Extras.DynamicProxy ;
using Interfaces.ServicesDiscovery.Wrappers ;

/// <inheritdoc />
[ ExcludeFromCodeCoverage ]
[ Intercept ( typeof ( LogAspect ) ) ]
public class GattDeviceServicesResultWrapperFactory
    : IGattDeviceServicesResultWrapperFactory
{
    private readonly GattDeviceServicesResultWrapper.Factory _servicesFactory ;

    public GattDeviceServicesResultWrapperFactory (
        GattDeviceServicesResultWrapper.Factory servicesFactory )
    {
        Guard.ArgumentNotNull ( servicesFactory ,
                                nameof ( servicesFactory ) ) ;

        _servicesFactory = servicesFactory ;
    }

    /// <inheritdoc />
    public IGattDeviceServicesResultWrapper Create ( GattDeviceServicesResult result )
    {
        Guard.ArgumentNotNull ( result ,
                                nameof ( result ) ) ;

        return _servicesFactory.Invoke ( result ) ;
    }
}
