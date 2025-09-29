using System.Reactive.Subjects ;
using Windows.Devices.Bluetooth ;
using Windows.Devices.Bluetooth.GenericAttributeProfile ;
using Autofac.Extras.DynamicProxy ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery.Wrappers ;
using Serilog ;

namespace Idasen.BluetoothLE.Core.ServicesDiscovery ;

/// <inheritdoc />
[ Intercept ( typeof ( LogAspect ) ) ]
public class GattServicesProvider
    : IGattServicesProvider
{
    public delegate IGattServicesProvider Factory ( IBluetoothLeDeviceWrapper device ) ;

    private readonly IBluetoothLeDeviceWrapper            _device ;
    private readonly ILogger                              _logger ;
    private readonly ISubject < GattCommunicationStatus > _refreshed ;
    private readonly IGattServices                        _services ;
    private          IGattDeviceServicesResultWrapper ?   _gattResult ;

    public GattServicesProvider ( ILogger                              logger ,
                                  IGattServices                        services ,
                                  ISubject < GattCommunicationStatus > refreshed ,
                                  IBluetoothLeDeviceWrapper            device )
    {
        Guard.ArgumentNotNull ( logger ,
                                nameof ( logger ) ) ;
        Guard.ArgumentNotNull ( services ,
                                nameof ( services ) ) ;
        Guard.ArgumentNotNull ( refreshed ,
                                nameof ( refreshed ) ) ;
        Guard.ArgumentNotNull ( device ,
                                nameof ( device ) ) ;

        _logger    = logger ;
        _services  = services ;
        _refreshed = refreshed ;
        _device    = device ;
    }

    /// <inheritdoc />
    public GattCommunicationStatus GattCommunicationStatus =>
        _gattResult?.Status ?? GattCommunicationStatus.Unreachable ;

    /// <inheritdoc />
    public async Task Refresh ( )
    {
        _services.Clear ( ) ;

        if ( _device.ConnectionStatus == BluetoothConnectionStatus.Disconnected )
        {
            _logger.Error ( "[{DeviceId}] {Status}" ,
                            _device.DeviceId ,
                            _device.ConnectionStatus ) ;

            _refreshed.OnNext ( GattCommunicationStatus.Unreachable ) ;

            return ;
        }

        try
        {
            _gattResult = await _device.GetGattServicesAsync ( ) ;
        }
        catch ( Exception ex )
        {
            _logger.Error ( ex ,
                            "[{DeviceId}] Failed to get GATT services" ,
                            _device.DeviceId ) ;
            _refreshed.OnNext ( GattCommunicationStatus.Unreachable ) ;
            return ;
        }

        if ( _gattResult.Status == GattCommunicationStatus.Success )
            await GetCharacteristicsAsync ( _gattResult ) ;
        else
            _logger.Error ( "[{DeviceId}] Gatt communication status '{Status}'" ,
                            _device.DeviceId ,
                            _gattResult.Status ) ;

        _refreshed.OnNext ( _gattResult.Status ) ;
    }

    /// <inheritdoc />
    public IObservable < GattCommunicationStatus > Refreshed => _refreshed ;

    /// <inheritdoc />
    public IReadOnlyDictionary < IGattDeviceServiceWrapper , IGattCharacteristicsResultWrapper > Services =>
        _services.ReadOnlyDictionary ;

    /// <inheritdoc />
    public void Dispose ( )
    {
        _services.Dispose ( ) ;
        GC.SuppressFinalize(this);
    }

    private async Task GetCharacteristicsAsync ( IGattDeviceServicesResultWrapper gatt )
    {
        foreach ( var service in gatt.Services )
        {
            IGattCharacteristicsResultWrapper characteristics ;

            try
            {
                characteristics = await service.GetCharacteristicsAsync ( ) ;
            }
            catch ( Exception ex )
            {
                _logger.Error ( ex ,
                                "[{DeviceId}] Exception getting Characteristics for device '{ServiceDeviceId}' and service '{ServiceUuid}'" ,
                                _device.DeviceId ,
                                service.DeviceId ,
                                service.Uuid ) ;
                continue ;
            }

            if ( characteristics.Status != GattCommunicationStatus.Success )
            {
                _logger.Error ( "[{DeviceId}] Could not get Characteristics for device '{ServiceDeviceId}' and service '{ServiceUuid}'" ,
                                _device.DeviceId ,
                                service.DeviceId ,
                                service.Uuid ) ;

                continue ;
            }

            _services [ service ] = characteristics ;
        }
    }
}
