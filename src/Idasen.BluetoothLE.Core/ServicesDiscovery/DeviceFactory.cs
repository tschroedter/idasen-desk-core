using System.Diagnostics.CodeAnalysis ;
using Autofac.Extras.DynamicProxy ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery.Wrappers ;

namespace Idasen.BluetoothLE.Core.ServicesDiscovery ;

/// <inheritdoc />
[ Intercept ( typeof ( LogAspect ) ) ]
public class DeviceFactory
    : IDeviceFactory
{
    private readonly Device.Factory _deviceFactory ;
    private readonly IBluetoothLEDeviceProvider _deviceProvider ;
    private readonly IBluetoothLeDeviceWrapperFactory _deviceWrapperFactory ;

    public DeviceFactory (
        Device.Factory deviceFactory ,
        IBluetoothLeDeviceWrapperFactory deviceWrapperFactory ,
        IBluetoothLEDeviceProvider deviceProvider )
    {
        Guard.ArgumentNotNull ( deviceFactory ,
                                nameof ( deviceFactory ) ) ;
        Guard.ArgumentNotNull ( deviceWrapperFactory ,
                                nameof ( deviceWrapperFactory ) ) ;
        Guard.ArgumentNotNull ( deviceProvider ,
                                nameof ( deviceProvider ) ) ;

        _deviceFactory = deviceFactory ;
        _deviceWrapperFactory = deviceWrapperFactory ;
        _deviceProvider = deviceProvider ;
    }

    /// <inheritdoc />
    [ ExcludeFromCodeCoverage ]
    public async Task < IDevice > FromBluetoothAddressAsync ( ulong address )
    {
        var device = await _deviceProvider.FromBluetoothAddressAsync ( address ) ;

        return device is null
                   ? throw new InvalidOperationException ( $"Failed to get BluetoothLEDevice for address {address}" )
                   : _deviceFactory ( _deviceWrapperFactory.Create ( device ) ) ;
    }
}