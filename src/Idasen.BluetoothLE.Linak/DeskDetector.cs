using System.Reactive.Concurrency ;
using System.Reactive.Disposables ;
using System.Reactive.Linq ;
using System.Reactive.Subjects ;
using Autofac.Extras.DynamicProxy ;
using Idasen.Aop ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Core.Interfaces.DevicesDiscovery ;
using Idasen.BluetoothLE.Linak.Interfaces ;
using Serilog ;

namespace Idasen.BluetoothLE.Linak ;

/// <inheritdoc />
[ Intercept ( typeof ( LogAspect ) ) ]
public class DeskDetector
    : IDeskDetector
{
    private readonly ISubject < IDesk > _deskDetected ;

    private readonly CompositeDisposable _disposables = new( ) ;

    private readonly IDeskFactory _factory ;

    private readonly ILogger                  _logger ;
    private readonly IDeviceMonitorWithExpiry _monitor ;
    private readonly IScheduler               _scheduler ;

    private readonly object _sync = new( ) ;

    private IDesk ? _desk ;

    public DeskDetector ( ILogger                  logger ,
                          IScheduler               scheduler ,
                          IDeviceMonitorWithExpiry monitor ,
                          IDeskFactory             factory ,
                          ISubject < IDesk >       deskDetected )
    {
        ArgumentNullException.ThrowIfNull ( logger ,
                                            nameof ( logger ) ) ;
        ArgumentNullException.ThrowIfNull ( scheduler ,
                                            nameof ( scheduler ) ) ;
        ArgumentNullException.ThrowIfNull ( monitor ,
                                            nameof ( monitor ) ) ;
        ArgumentNullException.ThrowIfNull ( factory ,
                                            nameof ( factory ) ) ;
        ArgumentNullException.ThrowIfNull ( deskDetected ,
                                            nameof ( deskDetected ) ) ;

        _logger       = logger ;
        _scheduler    = scheduler ;
        _monitor      = monitor ;
        _factory      = factory ;
        _deskDetected = deskDetected ;
    }

    /// <summary>
    ///     Indicates whether the detector is currently attempting to connect to a desk.
    /// </summary>
    public bool IsConnecting { get ; private set ; }

    /// <inheritdoc />
    public void Dispose ( )
    {
        try
        {
            _disposables.Dispose ( ) ;
            _monitor.Dispose ( ) ;
        }
        catch ( Exception ex )
        {
            _logger.Error ( ex ,
                            "Error occurred while disposing resources in DeskDetector." ) ;
        }
        finally
        {
            GC.SuppressFinalize ( this ) ;
        }
    }

    /// <inheritdoc />
    public IObservable < IDesk > DeskDetected => _deskDetected ;

    /// <inheritdoc />
    public IDeskDetector Initialize ( string deviceName ,
                                      ulong  deviceAddress ,
                                      uint   deviceTimeout )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace ( deviceName ) ;

        if ( deviceAddress == 0 )
            throw new ArgumentException ( "Device address must be a valid non-zero value." ,
                                          nameof ( deviceAddress ) ) ;

        if ( deviceTimeout is 0 or > 3600 ) // Example: Limit timeout to 1 hour
            throw new ArgumentOutOfRangeException ( nameof ( deviceTimeout ) ,
                                                    "Device timeout must be between 1 and 3600 seconds." ) ;

        _monitor.TimeOut = TimeSpan.FromSeconds ( deviceTimeout ) ;

        _disposables.Add ( _monitor.DeviceUpdated
                                   .ObserveOn ( _scheduler )
                                   .Subscribe ( OnDeviceUpdated ,
                                                ex => _logger.Error ( ex ,
                                                                      "Error handling DeviceUpdated" ) ) ) ;

        _disposables.Add ( _monitor.DeviceDiscovered
                                   .ObserveOn ( _scheduler )
                                   .Subscribe ( OnDeviceDiscovered ,
                                                ex => _logger.Error ( ex ,
                                                                      "Error handling DeviceDiscovered" ) ) ) ;

        _disposables.Add ( _monitor.DeviceNameUpdated
                                   .ObserveOn ( _scheduler )
                                   .Subscribe ( OnDeviceNameChanged ,
                                                ex => _logger.Error ( ex ,
                                                                      "Error handling DeviceNameUpdated" ) ) ) ;

        // todo find by address if possible

        _disposables.Add ( _monitor.DeviceNameUpdated
                                   .ObserveOn ( _scheduler )
                                   .Where ( device =>
                                                ( device.Name != null &&
                                                  device.Name.StartsWith ( deviceName ,
                                                                           StringComparison
                                                                              .InvariantCultureIgnoreCase ) ) ||
                                                device.Address == deviceAddress )
                                   .SubscribeAsync ( OnDeskDiscovered ,
                                                     ex => _logger.Error ( ex ,
                                                                           "Error handling OnDeskDiscovered" ) ) ) ;

        return this ;
    }

    /// <inheritdoc />
    public void StartListening ( )
    {
        _desk?.Dispose ( ) ;
        _desk = null ;

        _monitor.StartListening ( ) ;
    }

    /// <inheritdoc />
    public void StopListening ( )
    {
        _monitor.StopListening ( ) ;
    }

    private async Task OnDeskDiscovered ( IDevice device )
    {
        lock ( _sync )
        {
            if ( _desk != null || IsConnecting )
                return ;

            IsConnecting = true ;
        }

        try
        {
            _logger.Information ( "Desk discovered: MAC={MaskedMac}, Name={Name}" ,
                                  device.MacAddress.MaskMacAddress ( ) ,
                                  device.Name ) ;

            var desk = await _factory.CreateAsync ( device.Address )
                                     .ConfigureAwait ( false ) ;

            lock ( _sync )
            {
                _desk = desk ;
            }

            _desk.RefreshedChanged
                 .Subscribe ( OnRefreshedChanged ,
                              ex => _logger.Error ( ex ,
                                                    "Error handling RefreshedChanged" ) ) ;

            _desk.Connect ( ) ;
        }
        catch ( Exception e )
        {
            _logger.Error ( e ,
                            "Failed to connect to desk: MAC={MaskedMac}, Name={Name}" ,
                            device.MacAddress.MaskMacAddress ( ) ,
                            device.Name ) ;

            lock ( _sync )
            {
                IsConnecting = false ;
            }
        }
    }

    private void OnDeviceUpdated ( IDevice device )
    {
        _logger.Information ( "[{Mac}] Device Updated: {Details}" ,
                              device.MacAddress.MaskMacAddress ( ) ,
                              device.Details ) ;
    }

    private void OnDeviceDiscovered ( IDevice device )
    {
        _logger.Information ( "[{Mac}] Device Discovered: {Details}" ,
                              device.MacAddress.MaskMacAddress ( ) ,
                              device.Details ) ;
    }

    private void OnDeviceNameChanged ( IDevice device )
    {
        _logger.Information ( "[{Mac}] Device Name Changed: {Details}" ,
                              device.MacAddress.MaskMacAddress ( ) ,
                              device.Details ) ;
    }

    private void OnRefreshedChanged ( bool status )
    {
        lock ( _sync )
        {
            if ( _desk != null )
            {
                _deskDetected.OnNext ( _desk ) ;

                return ;
            }
        }

        _logger.Warning ( "Desk is null" ) ;
    }
}
