using System.Reactive.Concurrency ;
using System.Reactive.Linq ;
using System.Reactive.Subjects ;
using Autofac.Extras.DynamicProxy ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Core.Interfaces ;
using Idasen.BluetoothLE.Core.Interfaces.DevicesDiscovery ;
using Serilog ;

namespace Idasen.BluetoothLE.Core.DevicesDiscovery ;

[ Intercept ( typeof ( LogAspect ) ) ]
public class DeviceMonitorWithExpiry
    : IDeviceMonitorWithExpiry
{
    internal const int SixtySeconds = 60 ;

    private readonly IDateTimeOffset _dateTimeOffset ;

    private readonly ISubject < IDevice >    _deviceExpired ;
    private readonly IDeviceMonitor          _deviceMonitor ;
    private readonly IObservableTimerFactory _factory ;
    private readonly ILogger                 _logger ;
    private readonly IScheduler              _scheduler ;
    private          TimeSpan                _timeOut = TimeSpan.FromSeconds ( SixtySeconds ) ;

    private IDisposable ? _timer ;

    public DeviceMonitorWithExpiry (
        ILogger                 logger ,
        IDateTimeOffset         dateTimeOffset ,
        IDeviceMonitor          deviceMonitor ,
        ISubject < IDevice >    deviceExpired ,
        IObservableTimerFactory factory ,
        IScheduler              scheduler )
    {
        Guard.ArgumentNotNull (
                               logger ,
                               nameof ( logger ) ) ;
        Guard.ArgumentNotNull (
                               dateTimeOffset ,
                               nameof ( dateTimeOffset ) ) ;
        Guard.ArgumentNotNull (
                               deviceMonitor ,
                               nameof ( deviceMonitor ) ) ;
        Guard.ArgumentNotNull (
                               deviceExpired ,
                               nameof ( deviceExpired ) ) ;
        Guard.ArgumentNotNull (
                               factory ,
                               nameof ( factory ) ) ;
        Guard.ArgumentNotNull (
                               scheduler ,
                               nameof ( scheduler ) ) ;

        _logger         = logger ;
        _dateTimeOffset = dateTimeOffset ;
        _deviceMonitor  = deviceMonitor ;
        _deviceExpired  = deviceExpired ;
        _factory        = factory ;
        _scheduler      = scheduler ;

        // Keep backward compatibility with existing tests: start timer immediately
        StartTimerIfNeeded ( ) ;
    }

    /// <inheritdoc />
    public TimeSpan TimeOut
    {
        get => _timeOut ;
        set
        {
            if ( value.TotalSeconds < 0 )
                throw new ArgumentException ( "Value must be >= 0" ) ;

            _timeOut = value ;

            _logger.Information (
                                 "TimeOut = {Timeout}" ,
                                 value ) ;

            // restart timer if running to apply new timeout
            if ( _timer != null )
                RestartTimer ( ) ;
        }
    }

    /// <inheritdoc />
    public IObservable < IDevice > DeviceExpired => _deviceExpired ;

    /// <inheritdoc />
    public void Dispose ( )
    {
        StopTimer ( ) ;
        _deviceMonitor.Dispose ( ) ;
    }

    /// <inheritdoc />
    public IReadOnlyCollection < IDevice > DiscoveredDevices => _deviceMonitor.DiscoveredDevices ;

    /// <inheritdoc />
    public bool IsListening => _deviceMonitor.IsListening ;

    /// <inheritdoc />
    public IObservable < IDevice > DeviceUpdated => _deviceMonitor.DeviceUpdated ;

    /// <inheritdoc />
    public IObservable < IDevice > DeviceDiscovered => _deviceMonitor.DeviceDiscovered ;

    /// <inheritdoc />
    public IObservable < IDevice > DeviceNameUpdated => _deviceMonitor.DeviceNameUpdated ;

    /// <inheritdoc />
    public void Start ( )
    {
        _deviceMonitor.Start ( ) ;
        StartTimerIfNeeded ( ) ;
    }

    /// <inheritdoc />
    public void Stop ( )
    {
        _deviceMonitor.Stop ( ) ;
        StopTimer ( ) ;
    }

    /// <inheritdoc />
    public void RemoveDevice ( IDevice device ) => _deviceMonitor.RemoveDevice ( device ) ;

    private void OnCompleted ( ) => Stop ( ) ;

    private void OnError ( Exception ex )
    {
        _logger.Error ( ex.Message ) ;

        Stop ( ) ;
    }

    private void CleanUp ( long l )
    {
        foreach ( var device in DiscoveredDevices ) {
            var delta = _dateTimeOffset.Now.Ticks - device.BroadcastTime.Ticks ;

            if ( ! ( delta >= TimeOut.Ticks ) )
                continue ;

            RemoveDevice ( device ) ;

            _deviceExpired.OnNext ( device ) ;
        }
    }

    private void StartTimerIfNeeded ( )
    {
        if ( _timer != null )
            return ;

        _timer = _factory.Create (
                                  TimeOut ,
                                  _scheduler )
                         .SubscribeOn ( _scheduler )
                         .Subscribe (
                                     CleanUp ,
                                     OnError ,
                                     OnCompleted ) ;
    }

    private void StopTimer ( )
    {
        _timer?.Dispose ( ) ;
        _timer = null ;
    }

    private void RestartTimer ( )
    {
        StopTimer ( ) ;
        StartTimerIfNeeded ( ) ;
    }
}
