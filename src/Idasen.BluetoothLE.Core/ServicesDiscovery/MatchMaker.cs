using Autofac.Extras.DynamicProxy ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Core.Interfaces ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery.Wrappers ;
using Serilog ;

namespace Idasen.BluetoothLE.Core.ServicesDiscovery ;

[ Intercept ( typeof ( LogAspect ) ) ]
public class MatchMaker
    : IMatchMaker
{
    private readonly IDeviceFactory _deviceFactory ;
    private readonly ILogger        _logger ;

    public MatchMaker ( ILogger                         logger ,
                        IOfficialGattServicesCollection bluetoothGattServicesCollection ,
                        IDeviceFactory                  deviceFactory )
    {
        Guard.ArgumentNotNull ( logger ,
                                nameof ( logger ) ) ;
        Guard.ArgumentNotNull ( bluetoothGattServicesCollection ,
                                nameof ( bluetoothGattServicesCollection ) ) ;
        Guard.ArgumentNotNull ( deviceFactory ,
                                nameof ( deviceFactory ) ) ;

        _logger        = logger ;
        _deviceFactory = deviceFactory ;
    }

    /// <summary>
    ///     Attempts to pair to BLE device by address.
    /// </summary>
    /// <param name="address">The BLE device address.</param>
    /// <returns></returns>
    public async Task < IDevice > PairToDeviceAsync ( ulong address )
    {
        var device = await _deviceFactory.FromBluetoothAddressAsync ( address ) ;

        var macAddress = address.ToMacAddress ( ) ;

        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if ( device == null )
        {
            var message = $"Failed to find device with MAC Address '{macAddress}' " +
                          $"(Address {address})" ;

            throw new ArgumentNullException ( message ) ;
        }

        _logger.Information ( "[{MacAddress}] DeviceId after FromBluetoothAddressAsync: {DeviceId}" ,
                              macAddress ,
                              device.Id ) ;
        _logger.Information ( "[{MacAddress}] ConnectionStatus after FromBluetoothAddressAsync: {BluetoothConnectionStatus}" ,
                              macAddress ,
                              device.ConnectionStatus ) ;

        return device ;
    }
}