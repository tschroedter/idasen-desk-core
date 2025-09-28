namespace Idasen.BluetoothLE.Core.ServicesDiscovery.Wrappers ;

using System.Diagnostics.CodeAnalysis ;
using Windows.Devices.Bluetooth.GenericAttributeProfile ;
using Aop.Aspects ;
using Autofac.Extras.DynamicProxy ;
using Interfaces.ServicesDiscovery.Wrappers ;

/// <inheritdoc />
[ ExcludeFromCodeCoverage ]
[ Intercept ( typeof ( LogAspect ) ) ]
public class GattCharacteristicsResultWrapperFactory
    : IGattCharacteristicsResultWrapperFactory
{
    private readonly GattCharacteristicsResultWrapper.Factory _factory ;

    public GattCharacteristicsResultWrapperFactory (
        GattCharacteristicsResultWrapper.Factory factory )
    {
        Guard.ArgumentNotNull ( factory ,
                                nameof ( factory ) ) ;

        _factory = factory ;
    }

    /// <inheritdoc />
    public IGattCharacteristicsResultWrapper Create (
        GattCharacteristicsResult result )
    {
        Guard.ArgumentNotNull ( result ,
                                nameof ( result ) ) ;

        return _factory ( result ) ;
    }
}
