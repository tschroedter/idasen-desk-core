using System.Reactive.Concurrency ;
using System.Reactive.Linq ;
using System.Reactive.Subjects ;
using Windows.Devices.Bluetooth.GenericAttributeProfile ;
using Autofac.Extras.DynamicProxy ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Core ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery ;
using Idasen.BluetoothLE.Linak.Interfaces ;
using Serilog ;

namespace Idasen.BluetoothLE.Linak ;

// todo tests
[ Intercept ( typeof ( LogAspect ) ) ]
public class DeskConnector
    : IDeskConnector
{
    private readonly IDeskCommandExecutorFactory _commandExecutorFactory ;
    private readonly IDeskCharacteristics _deskCharacteristics ;
    private readonly IDeskLockerFactory _deskLockerFactory ;
    private readonly IDevice _device ;
    private readonly ISubject < IEnumerable < byte > > _deviceNameChanged ;
    private readonly IErrorManager _errorManager ;
    private readonly Subject < uint > _finishedSubject = new ( ) ;
    private readonly IDeskHeightAndSpeedFactory _heightAndSpeedFactory ;
    private readonly ILogger _logger ;
    private readonly IDeskMoverFactory _moverFactory ;
    private readonly IDisposable? _refreshedSubscription ;
    private readonly IScheduler _scheduler ;
    private readonly ISubject < uint > _subjectHeight ;
    private readonly ISubject < HeightSpeedDetails > _subjectHeightAndSpeed ;
    private readonly ISubject < bool > _subjectRefreshed ;
    private readonly ISubject < int > _subjectSpeed ;
    private IDeskLocker? _deskLocker ;

    // todo use list of IDisposables
    private IDeskMover? _deskMover ;
    private IDisposable? _disposableHeight ;
    private IDisposable? _disposableHeightAndSpeed ;
    private IDisposable? _disposableSpeed ;
    private IDeskCommandExecutor? _executor ;

    private IDisposable? _finishedSubscription ;
    private IDeskHeightAndSpeed? _heightAndSpeed ;
    private IDisposable? _subscriber ;

    public DeskConnector (
        ILogger logger ,
        IScheduler scheduler ,
        Func < ISubject < IEnumerable < byte > > > subjectFactory ,
        ISubject < uint > subjectHeight ,
        ISubject < int > subjectSpeed ,
        ISubject < bool > subjectRefreshed ,
        ISubject < HeightSpeedDetails > subjectHeightAndSpeed ,
        IDevice device ,
        IDeskCharacteristics deskCharacteristics ,
        IDeskHeightAndSpeedFactory heightAndSpeedFactory ,
        IDeskCommandExecutorFactory commandExecutorFactory ,
        IDeskMoverFactory moverFactory ,
        IDeskLockerFactory deskLockerFactory ,
        IErrorManager errorManager )
    {
        Guard.ArgumentNotNull ( logger ,
                                nameof ( logger ) ) ;
        Guard.ArgumentNotNull ( scheduler ,
                                nameof ( scheduler ) ) ;
        Guard.ArgumentNotNull ( subjectFactory ,
                                nameof ( subjectFactory ) ) ;
        Guard.ArgumentNotNull ( subjectHeight ,
                                nameof ( subjectHeight ) ) ;
        Guard.ArgumentNotNull ( subjectRefreshed ,
                                nameof ( subjectRefreshed ) ) ;
        Guard.ArgumentNotNull ( subjectSpeed ,
                                nameof ( subjectSpeed ) ) ;
        Guard.ArgumentNotNull ( subjectHeightAndSpeed ,
                                nameof ( subjectHeightAndSpeed ) ) ;
        Guard.ArgumentNotNull ( device ,
                                nameof ( device ) ) ;
        Guard.ArgumentNotNull ( deskCharacteristics ,
                                nameof ( deskCharacteristics ) ) ;
        Guard.ArgumentNotNull ( heightAndSpeedFactory ,
                                nameof ( heightAndSpeedFactory ) ) ;
        Guard.ArgumentNotNull ( commandExecutorFactory ,
                                nameof ( commandExecutorFactory ) ) ;
        Guard.ArgumentNotNull ( moverFactory ,
                                nameof ( moverFactory ) ) ;
        Guard.ArgumentNotNull ( deskLockerFactory ,
                                nameof ( deskLockerFactory ) ) ;
        Guard.ArgumentNotNull ( errorManager ,
                                nameof ( errorManager ) ) ; // todo testing

        _logger = logger ;
        _scheduler = scheduler ;
        _subjectHeight = subjectHeight ;
        _subjectSpeed = subjectSpeed ;
        _subjectRefreshed = subjectRefreshed ;
        _subjectHeightAndSpeed = subjectHeightAndSpeed ;
        _device = device ;
        _deskCharacteristics = deskCharacteristics ;
        _heightAndSpeedFactory = heightAndSpeedFactory ;
        _commandExecutorFactory = commandExecutorFactory ;
        _moverFactory = moverFactory ;
        _deskLockerFactory = deskLockerFactory ;
        _errorManager = errorManager ;

        _refreshedSubscription = _device.GattServicesRefreshed
                                        .Throttle ( TimeSpan.FromSeconds ( 1 ) )
                                        .SubscribeOn ( scheduler )
                                        .SubscribeAsync ( OnGattServicesRefreshed ) ;

        _deviceNameChanged = subjectFactory ( ) ;
    }

    /// <inheritdoc />
    public IObservable < uint > HeightChanged => _subjectHeight ;

    /// <inheritdoc />
    public IObservable < int > SpeedChanged => _subjectSpeed ;

    /// <inheritdoc />
    public IObservable < HeightSpeedDetails > HeightAndSpeedChanged => _subjectHeightAndSpeed ;

    /// <inheritdoc />
    public IObservable < uint > FinishedChanged => _finishedSubject ;

    /// <inheritdoc />
    public IObservable < bool > RefreshedChanged => _subjectRefreshed ;

    /// <inheritdoc />
    public void Dispose ( )
    {
        _finishedSubscription?.Dispose ( ) ;
        _refreshedSubscription?.Dispose ( ) ;
        _deskLocker?.Dispose ( ) ;
        _deskMover?.Dispose ( ) ;
        _disposableHeightAndSpeed?.Dispose ( ) ;
        _disposableSpeed?.Dispose ( ) ;
        _disposableHeight?.Dispose ( ) ;
        _heightAndSpeed?.Dispose ( ) ;
        _subscriber?.Dispose ( ) ;
        _device.Dispose ( ) ;

        _finishedSubject.OnCompleted ( ) ;
        _deviceNameChanged.OnCompleted ( ) ;
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
    public async Task < bool > MoveUp ( ) // todo this should be async
    {
        if ( ! TryGetDeskMover ( out var deskMover ) )
        {
            return false ;
        }

        return await deskMover!.Up ( ).ConfigureAwait ( false ) ;
    }

    /// <inheritdoc />
    public async Task < bool > MoveDown ( ) // todo check test for async
    {
        if ( ! TryGetDeskMover ( out var deskMover ) )
        {
            return false ;
        }

        return await deskMover!.Down ( ).ConfigureAwait ( false ) ;
    }

    /// <inheritdoc />
    public IObservable < IEnumerable < byte > > DeviceNameChanged => _deviceNameChanged ;

    /// <inheritdoc />
    public void MoveTo ( uint targetHeight )
    {
        if ( ! TryGetDeskMover ( out var deskMover ) )
        {
            return ;
        }

        deskMover!.TargetHeight = targetHeight ;

        if ( targetHeight == 0u )
        {
            throw new ArgumentException ( "TargetHeight can't be zero" ,
                                          nameof ( targetHeight ) ) ;
        }

        deskMover.Start ( ) ;
    }

    /// <inheritdoc />
    public async Task < bool > MoveStop ( ) // todo check test for async
    {
        if ( ! TryGetDeskMover ( out var deskMover ) )
        {
            return false ;
        }

        return await deskMover!.Stop ( ).ConfigureAwait ( false ) ;
    }

    /// <inheritdoc />
    public Task < bool > MoveLock ( ) // todo check test for async
    {
        if ( ! TryGetDeskLocker ( out var deskLocker ) )
        {
            return Task.FromResult ( false ) ;
        }

        deskLocker!.Lock ( ) ;

        return Task.FromResult ( true ) ;
    }

    /// <inheritdoc />
    public Task < bool > MoveUnlock ( ) // todo check test for async
    {
        if ( ! TryGetDeskLocker ( out var deskLocker ) )
        {
            return Task.FromResult ( false ) ;
        }

        deskLocker!.Unlock ( ) ;

        return Task.FromResult ( true ) ;
    }

    private bool TryGetDeskMover ( out IDeskMover? deskMover )
    {
        if ( _deskMover == null )
        {
            _logger.Error ( "Desk needs to be refreshed first!" ) ;

            deskMover = null ;

            return false ;
        }

        deskMover = _deskMover ;

        return true ;
    }

    private bool TryGetDeskLocker ( out IDeskLocker? deskLocker )
    {
        if ( _deskLocker == null )
        {
            _logger.Error ( "Desk needs to be refreshed first!" ) ;

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
            {
                _subjectRefreshed.OnNext ( false ) ;
            }
            else
            {
                await DoRefresh ( status ).ConfigureAwait ( false ) ;
            }
        }
        catch ( Exception e )
        {
            const string message = "Failed to refresh Gatt services" ;

            _logger.Error ( e ,
                            message ) ;

            _errorManager.PublishForMessage ( message ) ;

            _subjectRefreshed.OnNext ( false ) ;
        }
    }

    private async Task DoRefresh ( GattCommunicationStatus status )
    {
        _logger.Information ( $"[{_device.Id}] " +
                              $"ConnectionStatus: {_device.ConnectionStatus} " +
                              $"GattCommunicationStatus: {status} " +
                              $"GattCommunicationStatus: {_device.GattCommunicationStatus}" ) ;

        _deskCharacteristics.Initialize ( _device ) ;

        _subscriber?.Dispose ( ) ;

        _subscriber = _deskCharacteristics.GenericAccess
                                          .DeviceNameChanged
                                          .SubscribeOn ( _scheduler )
                                          .Subscribe ( OnDeviceNameChanged ) ;

        await _deskCharacteristics.Refresh ( ).ConfigureAwait ( false ) ;

        _heightAndSpeed?.Dispose ( ) ;
        _heightAndSpeed = _heightAndSpeedFactory.Create ( _deskCharacteristics.ReferenceOutput ) ;
        _heightAndSpeed.Initialize ( ) ;

        _disposableHeight = _heightAndSpeed.HeightChanged
                                           .SubscribeOn ( _scheduler )
                                           .Subscribe ( height => _subjectHeight.OnNext ( height ) ) ;
        _disposableSpeed = _heightAndSpeed.SpeedChanged
                                          .SubscribeOn ( _scheduler )
                                          .Subscribe ( speed => _subjectSpeed.OnNext ( speed ) ) ;
        _disposableHeightAndSpeed = _heightAndSpeed.HeightAndSpeedChanged
                                                   .SubscribeOn ( _scheduler )
                                                   .Subscribe ( details => _subjectHeightAndSpeed
                                                                   .OnNext ( details ) ) ;

        _executor = _commandExecutorFactory.Create ( _deskCharacteristics.Control ) ;
        _deskMover = _moverFactory.Create ( _executor ,
                                            _heightAndSpeed ) ??
                     throw new ArgumentException ( "Failed to create desk mover instance" ) ;

        _deskMover.Initialize ( ) ;

        _finishedSubscription?.Dispose ( ) ;
        _finishedSubscription = _deskMover.Finished
                                          .SubscribeOn ( _scheduler )
                                          .Subscribe ( value => _finishedSubject.OnNext ( value ) ) ;

        _deskLocker = _deskLockerFactory.Create ( _deskMover ,
                                                  _executor ,
                                                  _heightAndSpeed ) ;

        _deskLocker.Initialize ( ) ;

        _subjectRefreshed.OnNext ( true ) ;
    }

    private void OnDeviceNameChanged ( IEnumerable < byte > value )
    {
        _deviceNameChanged.OnNext ( value ) ;
    }
}