using System.Reactive.Concurrency ;
using System.Reactive.Linq ;
using System.Reactive.Subjects ;
using Windows.Devices.Bluetooth.GenericAttributeProfile ;
using Autofac.Extras.DynamicProxy ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery ;
using Idasen.BluetoothLE.Linak.Interfaces ;
using Serilog ;
using Serilog.Events ;
using System.Collections.Concurrent ;

namespace Idasen.BluetoothLE.Linak ;

/// <inheritdoc />
[ Intercept ( typeof ( LogAspect ) ) ]
public class DeskConnector
    : IDeskConnector
{
    private readonly IDeskCommandExecutorFactory       _commandExecutorFactory ;
    private readonly IDeskCharacteristics              _deskCharacteristics ;
    private readonly IDeskLockerFactory                _deskLockerFactory ;
    private readonly IDevice                           _device ;
    private readonly ISubject < IEnumerable < byte > > _deviceNameChanged ;
    private readonly IErrorManager                     _errorManager ;
    private readonly Subject < uint >                  _finishedSubject = new( ) ;
    private readonly IDeskHeightAndSpeedFactory        _heightAndSpeedFactory ;
    private readonly ILogger                           _logger ;
    private readonly IDeskMoverFactory                 _moverFactory ;
    private readonly IDisposable ?                     _refreshedSubscription ;
    private readonly IScheduler                        _scheduler ;
    private readonly IDeskConnectorSubjects            _subjects;
    private readonly ConcurrentQueue < Func < IDeskMover , Task > > _pendingMoverActions = new ( ) ;
    private          IDeskLocker ?                     _deskLocker ;

    private          IDeskMover ?           _deskMover ;
    private          IDisposable ?          _disposableHeight ;
    private          IDisposable ?          _disposableHeightAndSpeed ;
    private          IDisposable ?          _disposableSpeed ;
    private          IDisposable ?          _finishedSubscription ;
    private          IDeskHeightAndSpeed ?  _heightAndSpeed ;
    private          IDisposable ?          _subscriber ;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DeskConnector" /> class.
    /// </summary>
    public DeskConnector ( ILogger                                    logger ,
                           IScheduler                                 scheduler ,
                           IDeskConnectorSubjects                     subjects ,
                           IDevice                                    device ,
                           IDeskCharacteristics                       deskCharacteristics ,
                           IDeskConnectorFactories                    factories ,
                           IErrorManager                              errorManager )
    {
        ArgumentNullException.ThrowIfNull ( logger ) ;
        ArgumentNullException.ThrowIfNull ( scheduler ) ;
        ArgumentNullException.ThrowIfNull ( subjects ) ;
        ArgumentNullException.ThrowIfNull ( device ) ;
        ArgumentNullException.ThrowIfNull ( deskCharacteristics ) ;
        ArgumentNullException.ThrowIfNull ( factories ) ;
        ArgumentNullException.ThrowIfNull ( errorManager ) ;

        _logger                 = logger ;
        _scheduler              = scheduler ;
        _device                 = device ;
        _deskCharacteristics    = deskCharacteristics ;
        _heightAndSpeedFactory  = factories.HeightAndSpeedFactory ;
        _commandExecutorFactory = factories.CommandExecutorFactory ;
        _moverFactory           = factories.MoverFactory ;
        _deskLockerFactory      = factories.LockerFactory ;
        _errorManager           = errorManager ;
        _subjects               = subjects ;

        _refreshedSubscription = _device.GattServicesRefreshed
                                        .Throttle ( TimeSpan.FromSeconds ( 1 ) )
                                        .SubscribeOn ( scheduler )
                                        .SubscribeAsync ( OnGattServicesRefreshed ,
                                                          ex => _logger.Error ( ex ,
                                                                                   "Error handling GattServicesRefreshed" ) ) ;

        _deviceNameChanged = subjects.SubjectFactory ( ) ;
    }

    /// <inheritdoc />
    public IObservable < uint > HeightChanged => _subjects.HeightChanged;

    /// <inheritdoc />
    public IObservable < int > SpeedChanged => _subjects.SpeedChanged;

    /// <inheritdoc />
    public IObservable < HeightSpeedDetails > HeightAndSpeedChanged => _subjects.HeightAndSpeedChanged;

    /// <inheritdoc />
    public IObservable < uint > FinishedChanged => _finishedSubject ;

    /// <inheritdoc />
    public IObservable < bool > RefreshedChanged => _subjects.RefreshedChanged;

    /// <inheritdoc />
    public void Dispose ( )
    {
        Dispose ( true ) ;

        GC.SuppressFinalize ( this ) ;
    }

    /// <summary>
    /// Finalizer to ensure unmanaged resources are released.
    /// </summary>
    ~DeskConnector()
    {
        Dispose ( false ) ;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _finishedSubscription?.Dispose();
            _refreshedSubscription?.Dispose();
            _deskLocker?.Dispose();
            _deskMover?.Dispose();
            _disposableHeightAndSpeed?.Dispose();
            _disposableSpeed?.Dispose();
            _disposableHeight?.Dispose();
            _heightAndSpeed?.Dispose();
            _subscriber?.Dispose();
            _subscriber = null;
            _device.Dispose();

            _finishedSubject.OnCompleted();
            _deviceNameChanged.OnCompleted();
        }
    }

    /// <inheritdoc />
    public ulong BluetoothAddress => _device.BluetoothAddress ;

    /// <inheritdoc />
    public string BluetoothAddressType => _device.BluetoothAddressType ;

    /// <inheritdoc />
    public string DeviceName => _device.Name ;

    /// <inheritdoc />
    public void Connect ( )
    {
        _device.Connect ( ) ;
    }

    /// <inheritdoc />
    public async Task < bool > MoveUpAsync ( )
    {
        if ( TryGetDeskMover ( out var deskMover ) )
            return await deskMover!.Up ( ).ConfigureAwait ( false ) ;

        return await EnqueueMoverAction ( m => m.Up ( ) ).ConfigureAwait ( false ) ;
    }

    /// <inheritdoc />
    public async Task < bool > MoveDownAsync ( )
    {
        if ( TryGetDeskMover ( out var deskMover ) )
            return await deskMover!.Down ( ).ConfigureAwait ( false ) ;

        return await EnqueueMoverAction ( m => m.Down ( ) ).ConfigureAwait ( false ) ;
    }

    /// <inheritdoc />
    public IObservable < IEnumerable < byte > > DeviceNameChanged => _deviceNameChanged ;

    /// <inheritdoc />
    public void MoveTo ( uint targetHeight )
    {
        if ( ! TryGetDeskMover ( out var deskMover ) )
        {
            _logger.Debug ( "Mover not ready yet; queuing MoveTo({TargetHeight})" ,
                            targetHeight ) ;
            _pendingMoverActions.Enqueue ( m =>
            {
                m.TargetHeight = targetHeight ;
                m.Start ( ) ;
                return Task.CompletedTask ;
            } ) ;
            return ;
        }

        deskMover!.TargetHeight = targetHeight ;

        if ( targetHeight == 0u )
            throw new ArgumentException ( "TargetHeight can't be zero" ,
                                          nameof ( targetHeight ) ) ;

        deskMover.Start ( ) ;
    }

    /// <inheritdoc />
    public async Task < bool > MoveStopAsync ( )
    {
        if ( TryGetDeskMover ( out var deskMover ) )
            return await deskMover!.StopMovement ( ).ConfigureAwait ( false ) ;

        return await EnqueueMoverAction ( m => m.StopMovement ( ) ).ConfigureAwait ( false ) ;
    }

    /// <inheritdoc />
    public Task < bool > MoveLockAsync ( )
    {
        if ( ! TryGetDeskLocker ( out var deskLocker ) )
            return Task.FromResult ( false ) ;

        deskLocker!.Lock ( ) ;

        return Task.FromResult ( true ) ;
    }

    /// <inheritdoc />
    public Task < bool > MoveUnlockAsync ( )
    {
        if ( ! TryGetDeskLocker ( out var deskLocker ) )
            return Task.FromResult ( false ) ;

        deskLocker!.Unlock ( ) ;

        return Task.FromResult ( true ) ;
    }

    private bool TryGetDeskMover ( out IDeskMover ? deskMover )
    {
        if ( _deskMover == null )
        {
            _logger.Debug ( "Desk mover not ready; actions will be queued until refresh completes" ) ;

            deskMover = null ;

            return false ;
        }

        deskMover = _deskMover ;

        return true ;
    }

    private bool TryGetDeskLocker ( out IDeskLocker ? deskLocker )
    {
        if ( _deskLocker == null )
        {
            _logger.Error ( "Desk needs to be refreshed first" ) ;

            deskLocker = null ;

            return false ;
        }

        deskLocker = _deskLocker ;

        return true ;
    }

    private async Task OnGattServicesRefreshed ( GattCommunicationStatus status )
    {
        try
        {
            if ( status != GattCommunicationStatus.Success )
                _subjects.RefreshedChanged
                         .OnNext ( false ) ;
            else
                await DoRefresh ( status ).ConfigureAwait ( false ) ;
        }
        catch ( Exception e )
        {
            const string message = "Failed to refresh Gatt services" ;

            if ( _logger.IsEnabled ( LogEventLevel.Debug ) )
                _logger.Debug ( e ,
                                message ) ;
            else
                _logger.Warning ( e, 
                                  message ) ;

            _errorManager.PublishForMessage(message);

            _subjects.RefreshedChanged
                     .OnNext ( false ) ;
        }
    }

    private async Task DoRefresh ( GattCommunicationStatus status )
    {
        _logger.Information ( "[{Id}] ConnectionStatus={ConnectionStatus} GattCommunicationStatus={Status} DeviceGattStatus={DeviceGattStatus}" ,
                              _device.Id ,
                              _device.ConnectionStatus ,
                              status ,
                              _device.GattCommunicationStatus ) ;

        _deskCharacteristics.Initialize ( _device ) ;

        _subscriber?.Dispose ( ) ;

        _subscriber = _deskCharacteristics.GenericAccess
                                          .DeviceNameChanged
                                          .SubscribeOn ( _scheduler )
                                          .Subscribe ( OnDeviceNameChanged ,
                                                       ex => _logger.Error ( ex ,
                                                                             "Error handling DeviceNameChanged" ) ) ;

        await _deskCharacteristics.Refresh ( ).ConfigureAwait ( false ) ;

        _heightAndSpeed?.Dispose ( ) ;
        _heightAndSpeed = _heightAndSpeedFactory.Create ( _deskCharacteristics.ReferenceOutput ) ;
        _heightAndSpeed.Initialize ( ) ;

        _disposableHeight = _heightAndSpeed.HeightChanged
                                           .SubscribeOn ( _scheduler )
                                           .Subscribe ( height => _subjects.HeightChanged
                                                                           .OnNext ( height ) ) ;
        _disposableSpeed = _heightAndSpeed.SpeedChanged
                                          .SubscribeOn ( _scheduler )
                                          .Subscribe ( speed => _subjects.SpeedChanged
                                                                         .OnNext ( speed ) ) ;
        _disposableHeightAndSpeed = _heightAndSpeed.HeightAndSpeedChanged
                                                   .SubscribeOn ( _scheduler )
                                                   .Subscribe ( details => _subjects.HeightAndSpeedChanged
                                                                   .OnNext ( details ) ) ;

        var executor = _commandExecutorFactory.Create ( _deskCharacteristics.Control ) ;
        _deskMover = _moverFactory.Create ( executor ,
                                            _heightAndSpeed ) ??
                     throw new ArgumentException ( "Failed to create desk mover instance" ) ;

        _deskMover.Initialize ( ) ;

        _finishedSubscription?.Dispose ( ) ;
        _finishedSubscription = _deskMover.Finished
                                          .SubscribeOn ( _scheduler )
                                          .Subscribe ( value => _finishedSubject.OnNext ( value ) ) ;

        _deskLocker = _deskLockerFactory.Create ( _deskMover ,
                                                  executor ,
                                                  _heightAndSpeed ) ;

        _deskLocker.Initialize ( ) ;

        await DrainPendingMoverActionsAsync ( ).ConfigureAwait ( false ) ;

        _subjects.RefreshedChanged
                 .OnNext ( true ) ;
    }

    private void OnDeviceNameChanged ( IEnumerable < byte > value )
    {
        _deviceNameChanged.OnNext ( value ) ;
    }

    private Task < bool > EnqueueMoverAction ( Func < IDeskMover , Task < bool > > work )
    {
        var tcs = new TaskCompletionSource < bool > ( TaskCreationOptions.RunContinuationsAsynchronously ) ;

        _pendingMoverActions.Enqueue ( async m =>
                                       {
                                           try
                                           {
                                               var result = await work ( m ).ConfigureAwait ( false ) ;
                                               tcs.TrySetResult ( result ) ;
                                           }
                                           catch ( Exception ex )
                                           {
                                               _logger.Error ( ex ,
                                                               "Queued mover action failed" ) ;
                                               tcs.TrySetResult ( false ) ;
                                           }
                                       } ) ;

        _logger.Debug ( "Queued mover action until refresh completes" ) ;

        return tcs.Task ;
    }

    private async Task DrainPendingMoverActionsAsync ( )
    {
        var mover = _deskMover ;
        if ( mover == null )
            return ;

        while ( _pendingMoverActions.TryDequeue ( out var action ) )
        {
            try
            {
                await action ( mover ).ConfigureAwait ( false ) ;
            }
            catch ( Exception ex )
            {
                _logger.Error ( ex ,
                                "Error executing queued mover action" ) ;
            }
        }
    }
}
