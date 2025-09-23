using System.Reactive.Concurrency ;
using System.Reactive.Linq ;
using System.Reactive.Subjects ;
using Autofac.Extras.DynamicProxy ;
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

    private readonly IDeskFactory _factory ;

    private readonly ILogger _logger ;
    private readonly IDeviceMonitorWithExpiry _monitor ;
    private readonly IScheduler _scheduler ;

    private readonly object _sync = new ( ) ;

    // todo use list of disposables
    private IDesk? _desk ;
    private IDisposable? _deskFound ;
    private IDisposable? _discovered ;

    private IDisposable? _nameChanged ;
    private IDisposable? _refreshedChanged ;
    private IDisposable? _updated ;

    public DeskDetector ( ILogger logger ,
                          IScheduler scheduler ,
                          IDeviceMonitorWithExpiry monitor ,
                          IDeskFactory factory ,
                          ISubject < IDesk > deskDetected )
    {
        ArgumentNullException.ThrowIfNull ( logger ) ;
        ArgumentNullException.ThrowIfNull ( scheduler ) ;
        ArgumentNullException.ThrowIfNull ( monitor ) ;
        ArgumentNullException.ThrowIfNull ( factory ) ;
        ArgumentNullException.ThrowIfNull ( deskDetected ) ;

        _logger = logger ;
        _scheduler = scheduler ;
        _monitor = monitor ;
        _factory = factory ;
        _deskDetected = deskDetected ;
    }

    /// <summary>
    ///     Indicates whether the detector is currently attempting to connect to a desk.
    /// </summary>
    public bool IsConnecting { get ; private set ; }

    /// <inheritdoc />
    public void Dispose ( )
    {
        _refreshedChanged?.Dispose ( ) ;
        _deskFound?.Dispose ( ) ;
        _nameChanged?.Dispose ( ) ;
        _discovered?.Dispose ( ) ;
        _updated?.Dispose ( ) ;
        _monitor.Dispose ( ) ;
    }

    /// <inheritdoc />
    public IObservable < IDesk > DeskDetected => _deskDetected ;

    /// <inheritdoc />
    public IDeskDetector Initialize ( string deviceName ,
                                      ulong deviceAddress ,
                                      uint deviceTimeout )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace ( deviceName ) ;

        _monitor.TimeOut = TimeSpan.FromSeconds ( deviceTimeout ) ;

        _updated = _monitor.DeviceUpdated
                           .ObserveOn ( _scheduler )
                           .Subscribe ( OnDeviceUpdated ,
                                        ex => _logger.Error ( ex ,
                                                              "Error handling DeviceUpdated" ) ) ;

        _discovered = _monitor.DeviceDiscovered
                              .ObserveOn ( _scheduler )
                              .Subscribe ( OnDeviceDiscovered ,
                                           ex => _logger.Error ( ex ,
                                                                 "Error handling DeviceDiscovered" ) ) ;

        _nameChanged = _monitor.DeviceNameUpdated
                               .ObserveOn ( _scheduler )
                               .Subscribe ( OnDeviceNameChanged ,
                                            ex => _logger.Error ( ex ,
                                                                  "Error handling DeviceNameUpdated" ) ) ;

        // todo find by address if possible

        _deskFound = _monitor.DeviceNameUpdated
                             .ObserveOn ( _scheduler )
                             .Where ( device =>
                                          device.Name != null &&
                                          device.Name.StartsWith ( deviceName ,
                                                                   StringComparison.InvariantCultureIgnoreCase ) ||
                                          device.Address == deviceAddress )
                             .SubscribeAsync ( OnDeskDiscovered ,
                                               ex => _logger.Error ( ex ,
                                                                     "Error handling OnDeskDiscovered" ) ) ;

        return this ;
    }

    /// <inheritdoc />
    public void Start ( )
    {
        _desk?.Dispose ( ) ;
        _desk = null ;

        _monitor.Start ( ) ;
    }

    /// <inheritdoc />
    public void Stop ( )
    {
        _monitor.Stop ( ) ;
    }

    private async Task OnDeskDiscovered ( IDevice device )
    {
        lock (_sync)
        {
            if ( _desk != null || IsConnecting )
            {
                return ;
            }

            IsConnecting = true ;
        }

        try
        {
            _logger.Information ( "[{Mac}] Desk '{Name}' discovered" ,
                                  device.MacAddress ,
                                  device.Name ) ;

            _desk = await _factory.CreateAsync ( device.Address )
                                  .ConfigureAwait ( false ) ;

            _refreshedChanged = _desk.RefreshedChanged
                                     .Subscribe ( OnRefreshedChanged ,
                                                  ex => _logger.Error ( ex ,
                                                                        "Error handling RefreshedChanged" ) ) ;

            _desk.Connect ( ) ;
        }
        catch ( Exception e )
        {
            _logger.Error ( e ,
                            "[{Mac}] Failed to connect to desk '{Name}'" ,
                            device.MacAddress ,
                            device.Name ) ;

            IsConnecting = false ;
        }
    }

    private void OnDeviceUpdated ( IDevice device )
    {
        _logger.Information ( "[{Mac}] Device Updated: {Details}" ,
                              device.MacAddress ,
                              device.Details ) ;
    }

    private void OnDeviceDiscovered ( IDevice device )
    {
        _logger.Information ( "[{Mac}] Device Discovered: {Details}" ,
                              device.MacAddress ,
                              device.Details ) ;
    }

    private void OnDeviceNameChanged ( IDevice device )
    {
        _logger.Information ( "[{Mac}] Device Name Changed: {Details}" ,
                              device.MacAddress ,
                              device.Details ) ;
    }

    private void OnRefreshedChanged ( bool status )
    {
        if ( _desk != null )
        {
            _deskDetected.OnNext ( _desk ) ;
        }
        else
        {
            _logger.Warning ( "Desk is null" ) ;
        }
    }
}