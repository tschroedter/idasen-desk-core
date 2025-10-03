using System.Diagnostics.CodeAnalysis ;
using System.Reactive ;
using System.Reactive.Concurrency ;
using System.Reactive.Linq ;
using System.Reactive.Subjects ;
using Windows.Devices.Bluetooth ;
using Windows.Devices.Bluetooth.GenericAttributeProfile ;
using Autofac.Extras.DynamicProxy ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery.Wrappers ;
using Serilog ;

namespace Idasen.BluetoothLE.Core.ServicesDiscovery.Wrappers ;

/// <inheritdoc />
[ ExcludeFromCodeCoverage ]
[ Intercept ( typeof ( LogAspect ) ) ]
public class BluetoothLeDeviceWrapper : IBluetoothLeDeviceWrapper
{
    public delegate IBluetoothLeDeviceWrapper Factory ( BluetoothLEDevice device ) ;

    private readonly ISubject < BluetoothConnectionStatus >  _connectionStatusChanged ;
    private readonly BluetoothLEDevice                       _device ;
    private readonly IGattServices                           _gattServices ;
    private readonly ILogger                                 _logger ;
    private readonly IGattServicesProviderFactory            _providerFactory ;
    private readonly IGattDeviceServicesResultWrapperFactory _servicesFactory ;
    private readonly IDisposable                             _subscriberConnectionStatus ;
    private          bool                                    _disposed ;
    private          IGattServicesProvider ?                 _provider ;
    private          GattSession ?                           _session ;

    public BluetoothLeDeviceWrapper ( ILogger                                 logger ,
                                      IScheduler                              scheduler ,
                                      IGattServicesProviderFactory            providerFactory ,
                                      IGattDeviceServicesResultWrapperFactory servicesFactory ,
                                      IGattServices                           gattServices ,
                                      ISubject < BluetoothConnectionStatus >  connectionStatusChanged ,
                                      BluetoothLEDevice                       device )
    {
        Guard.ArgumentNotNull ( logger ,
                                nameof ( logger ) ) ;
        Guard.ArgumentNotNull ( scheduler ,
                                nameof ( scheduler ) ) ;
        Guard.ArgumentNotNull ( providerFactory ,
                                nameof ( providerFactory ) ) ;
        Guard.ArgumentNotNull ( servicesFactory ,
                                nameof ( servicesFactory ) ) ;
        Guard.ArgumentNotNull ( gattServices ,
                                nameof ( gattServices ) ) ;
        Guard.ArgumentNotNull ( connectionStatusChanged ,
                                nameof ( connectionStatusChanged ) ) ;
        Guard.ArgumentNotNull ( device ,
                                nameof ( device ) ) ;

        _logger                  = logger ;
        _providerFactory         = providerFactory ;
        _servicesFactory         = servicesFactory ;
        _gattServices            = gattServices ;
        _connectionStatusChanged = connectionStatusChanged ;
        _device                  = device ;

        var statusChanged =
            Observable.FromEventPattern < object > ( _device ,
                                                     nameof ( BluetoothLEDevice.ConnectionStatusChanged ) ) ;

        _subscriberConnectionStatus = statusChanged
                                     .ObserveOn ( scheduler )
                                     .Subscribe ( OnConnectionStatusChanged ) ;
    }

    /// <inheritdoc />
    public IObservable < BluetoothConnectionStatus > ConnectionStatusChanged => _connectionStatusChanged ;

    /// <inheritdoc />
    public IObservable < GattCommunicationStatus > GattServicesRefreshed => GetOrCreateProvider ( ).Refreshed ;

    /// <inheritdoc />
    public GattCommunicationStatus GattCommunicationStatus => GetOrCreateProvider ( ).GattCommunicationStatus ;

    /// <inheritdoc />
    public ulong BluetoothAddress => _device.BluetoothAddress ;

    /// <inheritdoc />
    public string BluetoothAddressType => _device.BluetoothAddressType.ToString ( ) ;

    /// <inheritdoc />
    public async void Connect ( )
    {
        try
        {
            await ConnectAsync ( ) ;
        }
        catch ( Exception e )
        {
            _logger.Error ( e ,
                            "Failed to connect to device {BluetoothAddress}" ,
                            _device.BluetoothAddress ) ;
        }
    }

    /// <inheritdoc />
    public async Task < IGattDeviceServicesResultWrapper > GetGattServicesAsync ( )
    {
        var gattServicesResult = await _device.GetGattServicesAsync ( ).AsTask ( ) ;
        return _servicesFactory.Create ( gattServicesResult ) ;
    }

    /// <inheritdoc />
    public string Name => _device.Name ?? string.Empty ;

    /// <inheritdoc />
    public string DeviceId => _device.DeviceId ?? string.Empty ;

    /// <inheritdoc />
    public bool IsPaired => _device.DeviceInformation?.Pairing?.IsPaired ?? false ;

    /// <inheritdoc />
    public BluetoothConnectionStatus ConnectionStatus => _device.ConnectionStatus ;

    /// <inheritdoc />
    public IReadOnlyDictionary < IGattDeviceServiceWrapper , IGattCharacteristicsResultWrapper > GattServices =>
        GetOrCreateProvider ( ).Services ;

    /// <inheritdoc />
    public void Dispose ( )
    {
        Dispose ( true ) ;
        GC.SuppressFinalize ( this ) ;
    }

    public async Task ConnectAsync ( )
    {
        try
        {
            if ( ConnectionStatus == BluetoothConnectionStatus.Connected )
            {
                _logger.Debug ( "[{DeviceId}] Already connected" ,
                                DeviceId ) ;
                return ;
            }

            if ( ! IsPaired )
            {
                _logger.Debug ( "[{DeviceId}] Not paired" ,
                                DeviceId ) ;
                return ;
            }

            await CreateSession ( ) ;
        }
        catch ( Exception e )
        {
            _logger.Error ( e ,
                            "Failed to connect to device {BluetoothAddress}" ,
                            _device.BluetoothAddress ) ;
        }
    }

    protected virtual void Dispose ( bool disposing )
    {
        if ( _disposed )
            return ;

        if ( disposing )
        {
            _provider?.Dispose ( ) ;
            _gattServices.Dispose ( ) ;
            _session?.Dispose ( ) ;
            _subscriberConnectionStatus.Dispose ( ) ;
            _device.Dispose ( ) ;
        }

        _disposed = true ;
    }

    private async Task CreateSession ( )
    {
        _session?.Dispose ( ) ;

        _session = await GattSession.FromDeviceIdAsync ( _device.BluetoothDeviceId ) ;

        if ( _session != null )
            _session.MaintainConnection = true ;
        else
            _logger.Warning ( "[{DeviceId}] Failed to create GATT session" ,
                              DeviceId ) ;
    }

    // ReSharper disable once AsyncVoidMethod
    private async void OnConnectionStatusChanged ( EventPattern < object > _ )
    {
        try
        {
            if ( ConnectionStatus == BluetoothConnectionStatus.Connected )
            {
                _logger.Debug ( "[{DeviceId}] BluetoothConnectionStatus = {BluetoothConnectionStatus}" ,
                                DeviceId ,
                                BluetoothConnectionStatus.Connected ) ;

                await GetOrCreateProvider ( ).Refresh ( ) ;
            }

            _connectionStatusChanged.OnNext ( _device.ConnectionStatus ) ;
        }
        catch ( Exception ex )
        {
            _logger.Error ( ex ,
                            "[{DeviceId}] Error in ConnectionStatusChanged handler" ,
                            DeviceId ) ;
        }
    }

    private IGattServicesProvider GetOrCreateProvider ( )
    {
        // note the creation of the provider only once,
        // but it might fail if the device is not connected
        return _provider ??= _providerFactory.Create ( this ) ;
    }
}
