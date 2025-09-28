using Autofac.Extras.DynamicProxy ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Characteristics.Interfaces.Characteristics.Customs ;
using Idasen.BluetoothLE.Core ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery.Wrappers ;

namespace Idasen.BluetoothLE.Characteristics.Characteristics.Customs ;

/// <summary>
///     Default factory for creating <see cref="IGattCharacteristicProvider" /> instances.
/// </summary>
[ Intercept ( typeof ( LogAspect ) ) ]
public class GattCharacteristicsProviderFactory
    : IGattCharacteristicsProviderFactory
{
    private readonly GattCharacteristicProvider.Factory _factory ;

    /// <summary>
    ///     Initializes a new instance of the <see cref="GattCharacteristicsProviderFactory" /> class.
    /// </summary>
    /// <param name="factory">The delegate factory to create providers.</param>
    public GattCharacteristicsProviderFactory (
        GattCharacteristicProvider.Factory factory )
    {
        Guard.ArgumentNotNull (
                               factory ,
                               nameof ( factory ) ) ;

        _factory = factory ;
    }

    /// <inheritdoc />
    public IGattCharacteristicProvider Create (
        IGattCharacteristicsResultWrapper wrapper ) =>
        _factory ( wrapper ) ;
}
