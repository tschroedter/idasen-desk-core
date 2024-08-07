﻿using System.Reactive.Concurrency ;
using System.Reactive.Linq ;
using System.Reactive.Subjects ;
using Autofac.Extras.DynamicProxy ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Core ;
using Idasen.BluetoothLE.Core.Interfaces.DevicesDiscovery ;
using Idasen.BluetoothLE.Linak.Interfaces ;
using Serilog ;

namespace Idasen.BluetoothLE.Linak
{
    [ Intercept ( typeof ( LogAspect ) ) ]
    public class DeskDetector
        : IDeskDetector
    {
        public DeskDetector ( ILogger                  logger ,
                              IScheduler               scheduler ,
                              IDeviceMonitorWithExpiry monitor ,
                              IDeskFactory             factory ,
                              ISubject < IDesk >       deskDetected )
        {
            Guard.ArgumentNotNull ( logger ,
                                    nameof ( logger ) ) ;
            Guard.ArgumentNotNull ( scheduler ,
                                    nameof ( scheduler ) ) ;
            Guard.ArgumentNotNull ( monitor ,
                                    nameof ( monitor ) ) ;
            Guard.ArgumentNotNull ( factory ,
                                    nameof ( factory ) ) ;
            Guard.ArgumentNotNull ( deskDetected ,
                                    nameof ( deskDetected ) ) ;

            _logger       = logger ;
            _scheduler    = scheduler ;
            _monitor      = monitor ;
            _factory      = factory ;
            _deskDetected = deskDetected ;
        }

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
                                          ulong  deviceAddress ,
                                          uint   deviceTimeout )
        {
            Guard.ArgumentNotNull ( deviceName ,
                                    nameof ( deviceName ) ) ;

            _monitor.TimeOut = TimeSpan.FromSeconds ( deviceTimeout ) ;

            _updated = _monitor.DeviceUpdated
                               .ObserveOn ( _scheduler )
                               .Subscribe ( OnDeviceUpdated ) ;

            _discovered = _monitor.DeviceDiscovered
                                  .ObserveOn ( _scheduler )
                                  .Subscribe ( OnDeviceDiscovered ) ;

            _nameChanged = _monitor.DeviceNameUpdated
                                   .ObserveOn ( _scheduler )
                                   .Subscribe ( OnDeviceNameChanged ) ;

            // todo find by address if possible

            _deskFound = _monitor.DeviceNameUpdated
                                 .ObserveOn ( _scheduler )
                                 .Where ( device => ( device.Name != null &&
                                                      device.Name.StartsWith ( deviceName ,
                                                                               StringComparison
                                                                                  .InvariantCultureIgnoreCase ) ) ||
                                                    device.Address == deviceAddress )
                                 .SubscribeAsync ( OnDeskDiscovered ) ;

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

        public bool IsConnecting { get ; private set ; }

        private async Task OnDeskDiscovered ( IDevice device )
        {
            lock ( this )
            {
                if ( _desk != null || IsConnecting )
                    return ;

                IsConnecting = true ;
            }

            try
            {
                _logger.Information ( $"[{device.MacAddress}] Desk '{device.Name}' discovered" ) ;

                _desk = await _factory.CreateAsync ( device.Address ) ;

                _refreshedChanged = _desk.RefreshedChanged
                                         .Subscribe ( OnRefreshedChanged ) ;

                _desk.Connect ( ) ;
            }
            catch ( Exception e )
            {
                _logger.Error ( $"[{device.MacAddress}] Failed to connect to desk '{device.Name}' ({e.Message})" ) ;

                IsConnecting = false ;
            }
        }

        private void OnDeviceUpdated ( IDevice device )
        {
            _logger.Information ( $"[{device.MacAddress}] Device Updated: {device.Details}" ) ;
        }

        private void OnDeviceDiscovered ( IDevice device )
        {
            _logger.Information ( $"[{device.MacAddress}] Device Discovered: {device.Details}" ) ;
        }

        private void OnDeviceNameChanged ( IDevice device )
        {
            _logger.Information ( $"[{device.MacAddress}] Device Name Changed: {device.Details}" ) ;
        }

        private void OnRefreshedChanged ( bool status )
        {
            if ( _desk != null )
                _deskDetected.OnNext ( _desk ) ;
            else
                _logger.Warning ( "Desk is null" ) ;
        }

        private readonly ISubject < IDesk > _deskDetected ;

        private readonly IDeskFactory _factory ;

        private readonly ILogger                  _logger ;
        private readonly IDeviceMonitorWithExpiry _monitor ;
        private readonly IScheduler               _scheduler ;

        // todo use list of disposables
        private IDesk ?       _desk ;
        private IDisposable ? _deskFound ;
        private IDisposable ? _discovered ;

        private IDisposable ? _nameChanged ;
        private IDisposable ? _refreshedChanged ;
        private IDisposable ? _updated ;
    }
}