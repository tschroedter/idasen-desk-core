namespace Idasen.BluetoothLE.Core.DevicesDiscovery ;

using Aop.Aspects ;
using Autofac.Extras.DynamicProxy ;
using Interfaces ;
using Interfaces.DevicesDiscovery ;

/// <inheritdoc />
[ Intercept ( typeof ( LogAspect ) ) ]
public class DeviceFactory
    : IDeviceFactory
{
    private readonly Device.Factory _factory ;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DeviceFactory" /> class.
    /// </summary>
    /// <param name="factory">
    ///     The factory delegate used to create devices.
    /// </param>
    public DeviceFactory ( Device.Factory factory )
    {
        Guard.ArgumentNotNull ( factory ,
                                nameof ( factory ) ) ;

        _factory = factory ;
    }

    /// <inheritdoc />
    public IDevice Create ( IDateTimeOffset broadcastTime ,
                            ulong address ,
                            string? name ,
                            short rawSignalStrengthInDBm )
    {
        Guard.ArgumentNotNull ( broadcastTime ,
                                nameof ( broadcastTime ) ) ;

        return _factory.Invoke ( broadcastTime ,
                                 address ,
                                 name ,
                                 rawSignalStrengthInDBm ) ;
    }
}
