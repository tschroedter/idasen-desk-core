using System.Reactive.Concurrency ;
using System.Reactive.Linq ;
using Autofac.Extras.DynamicProxy ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Characteristics.Common ;
using Idasen.BluetoothLE.Core ;
using Idasen.BluetoothLE.Linak.Interfaces ;
using Serilog ;

namespace Idasen.BluetoothLE.Linak ;

/// <inheritdoc />
[Intercept ( typeof ( LogAspect ) ) ]
public class DeskProvider
    : IDeskProvider
{
    private readonly IDeskDetector _detector ;
    private readonly IErrorManager _errorManager ;
    private readonly ILogger _logger ;
    private readonly IScheduler _scheduler ;
    private readonly ITaskRunner _taskRunner ;

    internal readonly AutoResetEvent DeskDetectedEvent = new ( false ) ;

    private IDisposable? _deskDetected ;
    private bool _disposed ;

    public DeskProvider (
        ILogger logger ,
        ITaskRunner taskRunner ,
        IScheduler scheduler ,
        IDeskDetector detector ,
        IErrorManager errorManager )
    {
        ArgumentNullException.ThrowIfNull ( logger ) ;
        ArgumentNullException.ThrowIfNull ( taskRunner ) ;
        ArgumentNullException.ThrowIfNull ( scheduler ) ;
        ArgumentNullException.ThrowIfNull ( detector ) ;
        ArgumentNullException.ThrowIfNull ( errorManager ) ;

        _logger = logger ;
        _taskRunner = taskRunner ;
        _scheduler = scheduler ;
        _detector = detector ;
        _errorManager = errorManager ;
    }

    /// <inheritdoc />
    public async Task < (bool , IDesk?) > TryGetDesk ( CancellationToken token )
    {
        Desk?.Dispose ( ) ;
        Desk = null ;

        try
        {
            _detector.Start ( ) ;

            await _taskRunner.Run ( ( ) => DoTryGetDesk ( token ) ,
                                    token )
                             .ConfigureAwait ( false ) ;

            if ( token.IsCancellationRequested )
            {
                return ( false , null ) ;
            }

            return Desk == null
                       ? ( false , null )
                       : ( true , Desk ) ;
        }
        catch ( Exception e )
        {
            if ( e.IsBluetoothDisabledException ( ) )
            {
                e.LogBluetoothStatusException ( _logger ,
                                                string.Empty ) ;
            }
            else
            {
                _logger.Error ( e ,
                                "Failed to detect desk" ) ;
            }

            return ( false , null ) ;
        }
        finally
        {
            _detector.Stop ( ) ;
            DeskDetectedEvent.Reset ( ) ;
        }
    }

    /// <inheritdoc />
    public IObservable < IDesk > DeskDetected => _detector.DeskDetected ;

    /// <inheritdoc />
    public IDeskProvider Initialize ( string deviceName ,
                                      ulong deviceAddress ,
                                      uint deviceTimeout )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace ( deviceName ) ;

        _detector.Initialize ( deviceName ,
                               deviceAddress ,
                               deviceTimeout ) ;

        _deskDetected?.Dispose ( ) ;
        _deskDetected = _detector.DeskDetected
                                 .ObserveOn ( _scheduler )
                                 .Subscribe ( OnDeskDetected ,
                                             ex => _logger.Error ( ex , "Error while handling detected desk" ) ) ;

        return this ;
    }

    /// <inheritdoc />
    public IDeskProvider StartDetecting ( )
    {
        _logger.Information ( "Start trying to detect desk" ) ;

        try
        {
            _detector.Start ( ) ;
        }
        catch ( Exception e )
        {
            _logger.Error ( e ,
                            "Failed Start Detecting" ) ;

            _errorManager.PublishForMessage ( "Failed Start Detecting" ) ;
        }

        return this ;
    }

    /// <inheritdoc />
    public IDeskProvider StopDetecting ( )
    {
        _logger.Information ( "Stop trying to detect desk" ) ;

        try
        {
            _detector.Stop ( ) ;
        }
        catch ( Exception e )
        {
            _logger.Error ( e ,
                            "Failed Stop Detecting" ) ;

            _errorManager.PublishForMessage ( "Failed Stop Detecting" ) ;
        }

        return this ;
    }

    /// <inheritdoc />
    public void Dispose ( )
    {
        if ( _disposed )
        {
            return ;
        }

        Desk?.Dispose ( ) ; // todo test
        _deskDetected?.Dispose ( ) ;
        _deskDetected = null ;
        _detector.Dispose ( ) ;

        _disposed = true ;
    }

    /// <summary>
    ///     Gets the last detected desk instance.
    /// </summary>
    public IDesk? Desk { get ; private set ; }

    internal void DoTryGetDesk ( CancellationToken token )
    {
        while ( Desk == null &&
                ! token.IsCancellationRequested )
        {
            _logger.Information ( "Trying to find desk" ) ;

            DeskDetectedEvent.WaitOne ( TimeSpan.FromSeconds ( 1 ) ) ;
        }
    }

    internal void OnDeskDetected ( IDesk desk )
    {
        _logger.Information ( "Detected desk {Name} with Bluetooth address {Address}" ,
                              desk.Name ,
                              desk.BluetoothAddress ) ;

        _detector.Stop ( ) ;

        Desk = desk ;

        DeskDetectedEvent.Set ( ) ;
    }
}