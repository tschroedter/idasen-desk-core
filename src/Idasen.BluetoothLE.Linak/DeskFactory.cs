namespace Idasen.BluetoothLE.Linak ;

using Aop.Aspects ;
using Autofac.Extras.DynamicProxy ;
using Core.Interfaces.ServicesDiscovery ;
using Interfaces ;

/// <inheritdoc />
[ Intercept ( typeof ( LogAspect ) ) ]
public class DeskFactory
    : IDeskFactory
{
    private readonly Func < IDevice , IDeskConnector > _deskConnectorFactory ;
    private readonly Func < IDeskConnector , IDesk > _deskFactory ;
    private readonly IDeviceFactory _deviceFactory ;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DeskFactory" /> class.
    /// </summary>
    /// <param name="deviceFactory">The device factory.</param>
    /// <param name="deskConnectorFactory">The desk connector factory.</param>
    /// <param name="deskFactory">The desk factory.</param>
    public DeskFactory (
        IDeviceFactory deviceFactory ,
        Func < IDevice , IDeskConnector > deskConnectorFactory ,
        Func < IDeskConnector , IDesk > deskFactory )
    {
        ArgumentNullException.ThrowIfNull ( deskConnectorFactory ) ;
        ArgumentNullException.ThrowIfNull ( deviceFactory ) ;
        ArgumentNullException.ThrowIfNull ( deskFactory ) ;

        _deviceFactory = deviceFactory ;
        _deskConnectorFactory = deskConnectorFactory ;
        _deskFactory = deskFactory ;
    }

    /// <inheritdoc />
    public async Task < IDesk > CreateAsync ( ulong address )
    {
        IDevice device = await _deviceFactory.FromBluetoothAddressAsync ( address )
                                             .ConfigureAwait ( false ) ;
        IDeskConnector connector = _deskConnectorFactory.Invoke ( device ) ;

        return _deskFactory.Invoke ( connector ) ;
    }
}
