using System.Reactive.Concurrency ;
using System.Reactive.Linq ;
using Autofac.Extras.DynamicProxy ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Characteristics.Common ;
using Idasen.BluetoothLE.Linak.Interfaces ;
using Serilog ;

namespace Idasen.BluetoothLE.Linak ;

/// <inheritdoc />
[ Intercept ( typeof ( LogAspect ) ) ]
public class DeskProvider
    : IDeskProvider
{
    private readonly IDeskDetector _detector ;
    private readonly IErrorManager _errorManager ;
    private readonly ILogger       _logger ;
    private readonly IScheduler    _scheduler ;
    private readonly ITaskRunner   _taskRunner ;

    internal readonly AutoResetEvent DeskDetectedEvent = new(false) ;

    // Backing field for Desk with volatile memory semantics
    private IDesk ? _desk ;

    private IDisposable ? _deskDetected ;
    private bool          _disposed ;

    public DeskProvider ( ILogger       logger ,
                          ITaskRunner   taskRunner ,
                          IScheduler    scheduler ,
                          IDeskDetector detector ,
                          IErrorManager errorManager )
    {
        ArgumentNullException.ThrowIfNull ( logger ) ;
        ArgumentNullException.ThrowIfNull ( taskRunner ) ;
        ArgumentNullException.ThrowIfNull ( scheduler ) ;
        ArgumentNullException.ThrowIfNull ( detector ) ;
        ArgumentNullException.ThrowIfNull ( errorManager ) ;

        _logger       = logger ;
        _taskRunner   = taskRunner ;
        _scheduler    = scheduler ;
        _detector     = detector ;
        _errorManager = errorManager ;
    }

    /// <inheritdoc />
    public async Task < (bool , IDesk ?) > TryGetDesk ( CancellationToken token )
    {
        Desk?.Dispose ( ) ;
        Desk = null ;

        try
        {
            _detector.StartListening ( ) ;

            await _taskRunner.Run ( ( ) => DoTryGetDesk ( token ) ,
                                    token )
                             .ConfigureAwait ( false ) ;

            return token.IsCancellationRequested
                       ? ( false , null )
                       : Desk == null
                           ? ( false , null )
                           : ( true , Desk ) ;
        }
        catch ( Exception e )
        {
            if ( e.IsBluetoothDisabledException ( ) )
                e.LogBluetoothStatusException ( _logger ,
                                                string.Empty ) ;
            else
                _logger.Error ( e ,
                                "Failed to detect desk" ) ;

            return ( false , null ) ;
        }
        finally
        {
            _detector.StopListening ( ) ;
            DeskDetectedEvent.Reset ( ) ;
        }
    }

    /// <inheritdoc />
    public IObservable < IDesk > DeskDetected => _detector.DeskDetected ;

    /// <inheritdoc />
    public IDeskProvider Initialize ( string deviceName ,
                                      ulong  deviceAddress ,
                                      uint   deviceTimeout )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace ( deviceName ) ;

        _detector.Initialize ( deviceName ,
                               deviceAddress ,
                               deviceTimeout ) ;

        _deskDetected?.Dispose ( ) ;
        _deskDetected = _detector.DeskDetected
                                 .ObserveOn ( _scheduler )
                                 .Subscribe ( OnDeskDetected ,
                                              ex => _logger.Error ( ex ,
                                                                    "Error while handling detected desk" ) ) ;

        return this ;
    }

    /// <inheritdoc />
    public IDeskProvider StartDetecting ( )
    {
        _logger.Information ( "StartListening trying to detect desk" ) ;

        try
        {
            _detector.StartListening ( ) ;
        }
        catch ( Exception e )
        {
            _logger.Error ( e ,
                            "Failed StartListening Detecting" ) ;

            _errorManager.PublishForMessage ( "Failed StartListening Detecting" ) ;
        }

        return this ;
    }

    /// <inheritdoc />
    public IDeskProvider StopDetecting ( )
    {
        _logger.Information ( "StopListening trying to detect desk" ) ;

        try
        {
            _detector.StopListening ( ) ;
        }
        catch ( Exception e )
        {
            _logger.Error ( e ,
                            "Failed StopListening Detecting" ) ;

            _errorManager.PublishForMessage ( "Failed StopListening Detecting" ) ;
        }

        return this ;
    }

    /// <inheritdoc />
    public void Dispose ( )
    {
        if ( _disposed )
            return ;

        Desk?.Dispose ( ) ; // todo test
        _deskDetected?.Dispose ( ) ;
        _deskDetected = null ;
        _detector.Dispose ( ) ;

        _disposed = true ;

        GC.SuppressFinalize ( this ) ;
    }

    /// <summary>
    ///     Gets the last detected desk instance.
    /// </summary>
    public IDesk ? Desk
    {
        get => Volatile.Read ( ref _desk ) ;
        private set =>
            Volatile.Write ( ref _desk ,
                             value ) ;
    }

    internal void DoTryGetDesk ( CancellationToken token )
    {
        // Fast-path exit if already detected or canceled
        if ( Desk != null ||
             token.IsCancellationRequested )
            return ;

        var handles = new [ ]
                      {
                          DeskDetectedEvent ,
                          token.WaitHandle
                      } ;

        while ( Desk == null &&
                ! token.IsCancellationRequested )
        {
            _logger.Information ( "Trying to find desk" ) ;

            // Wait up to 1s for either the desk-detected event or cancellation
            var index = WaitHandle.WaitAny ( handles ,
                                             1000 ) ;

            // If cancellation was signaled, leave immediately
            if ( index == 1 )
                break ;

            // If timed out (index == WaitHandle.WaitTimeout) just loop again and re-check conditions
            // If desk-detected event (index == 0), loop will re-check Desk and exit if set
        }
    }

    internal void OnDeskDetected ( IDesk desk )
    {
        // Ensure Desk is visible immediately to other threads waiting on it
        Desk = desk ;
        DeskDetectedEvent.Set ( ) ;

        try
        {
            _detector.StopListening ( ) ;
        }
        catch ( Exception e )
        {
            _logger.Error ( e ,
                            "Failed stopping detector after detection" ) ;
            _errorManager.PublishForMessage ( "Failed StopListening Detecting" ) ;
        }

        try
        {
            _logger.Information ( "Detected desk {Name} with Bluetooth address {Address}" ,
                                  desk.Name ,
                                  desk.BluetoothAddress ) ;
        }
        catch
        {
            // Swallow logging issues to not impact state used in tests
        }
    }
}