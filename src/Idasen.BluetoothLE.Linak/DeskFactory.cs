using Autofac.Extras.DynamicProxy ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Core ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery ;
using Idasen.BluetoothLE.Linak.Interfaces ;

namespace Idasen.BluetoothLE.Linak ;

[ Intercept ( typeof ( LogAspect ) ) ]
/// <summary>
///     Factory that composes a desk from a device, connector, and facade.
/// </summary>
public class DeskFactory
    : IDeskFactory
{
    private readonly Func < IDevice , IDeskConnector > _deskConnectorFactory ;
    private readonly Func < IDeskConnector , IDesk > _deskFactory ;
    private readonly IDeviceFactory _deviceFactory ;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DeskFactory"/> class.
    /// </summary>
    /// <param name="deviceFactory">The device factory.</param>
    /// <param name="deskConnectorFactory">The desk connector factory.</param>
    /// <param name="deskFactory">The desk factory.</param>
    public DeskFactory (
        IDeviceFactory deviceFactory ,
        Func < IDevice , IDeskConnector > deskConnectorFactory ,
        Func < IDeskConnector , IDesk > deskFactory )
    {
        Guard.ArgumentNotNull ( deskConnectorFactory ,
                                nameof ( deskConnectorFactory ) ) ;
        Guard.ArgumentNotNull ( deviceFactory ,
                                nameof ( deviceFactory ) ) ;
        Guard.ArgumentNotNull ( deskFactory ,
                                nameof ( deskFactory ) ) ;

        _deviceFactory = deviceFactory ;
        _deskConnectorFactory = deskConnectorFactory ;
        _deskFactory = deskFactory ;
    }

    /// <inheritdoc />
    public async Task < IDesk > CreateAsync ( ulong address )
    {
        var device = await _deviceFactory.FromBluetoothAddressAsync ( address ) ;
        var connector = _deskConnectorFactory.Invoke ( device ) ;

        return _deskFactory.Invoke ( connector ) ;
    }
}