using System.Reactive.Concurrency ;
using System.Reactive.Linq ;
using System.Reactive.Subjects ;
using Autofac.Extras.DynamicProxy ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Linak.Interfaces ;
using Serilog ;

namespace Idasen.BluetoothLE.Linak.Control ;

/// <inheritdoc />
[ Intercept ( typeof ( LogAspect ) ) ]
public class DeskMovementMonitor
    : IDeskMovementMonitor
{
    public delegate IDeskMovementMonitor Factory ( IDeskHeightAndSpeed heightAndSpeed ) ;

    internal const int MinimumNumberOfItems = 3 ;

    internal const int    DefaultCapacity       = 5 ;
    internal const string HeightDidNotChange    = "Height didn't change when moving desk" ;
    internal const string SpeedWasZero          = "Speed was zero when moving desk" ;
    internal const string NoHeightUpdatesReceived = "No height updates received for timeout period" ;

    private readonly IDeskHeightAndSpeed       _heightAndSpeed ;
    private readonly ILogger                   _logger ;
    private readonly IScheduler                _scheduler ;
    private readonly Subject < string >        _subjectInactivityDetected ;

    private IDisposable ? _disposalHeightAndSpeed ;
    private IDisposable ? _inactivityTimer ;
    private DateTimeOffset _lastUpdateTime = DateTimeOffset.MinValue ;
    private bool           _inactivityDetected ;
    private bool           _disposed ;

    internal CircularBuffer < HeightSpeedDetails > History = new(5) ;

    public DeskMovementMonitor ( ILogger             logger ,
                                 IScheduler          scheduler ,
                                 IDeskHeightAndSpeed heightAndSpeed )
    {
        ArgumentNullException.ThrowIfNull ( scheduler ) ;
        ArgumentNullException.ThrowIfNull ( heightAndSpeed ) ;
        ArgumentNullException.ThrowIfNull ( logger ) ;

        _logger                    = logger ;
        _scheduler                 = scheduler ;
        _heightAndSpeed            = heightAndSpeed ;
        _subjectInactivityDetected = new Subject < string > ( ) ;
    }

    /// <inheritdoc />
    public IObservable < string > InactivityDetected => _subjectInactivityDetected ;

    /// <inheritdoc />
    public void Dispose ( )
    {
        Dispose ( true ) ;

        GC.SuppressFinalize ( this ) ;
    }

    /// <summary>
    ///     Initializes monitoring with the specified history capacity.
    /// </summary>
    /// <param name="capacity">The number of samples to retain in history.</param>
    public void Initialize ( int capacity = DefaultCapacity )
    {
        if ( capacity <= 0 )
            throw new ArgumentOutOfRangeException ( nameof ( capacity ) ,
                                                    capacity ,
                                                    "Capacity must be positive." ) ;

        History = new CircularBuffer < HeightSpeedDetails > ( capacity ) ;

        _disposalHeightAndSpeed?.Dispose ( ) ;
        _disposalHeightAndSpeed = _heightAndSpeed.HeightAndSpeedChanged
                                                 .ObserveOn ( _scheduler )
                                                 .Subscribe ( OnHeightAndSpeedChanged ,
                                                              ex => _logger.Error ( ex ,
                                                                                    "Error observing height/speed changes" ) ) ;

        // Reset inactivity detection flag
        _inactivityDetected = false ;

        // Start inactivity watchdog: check every 5 seconds if we've received updates
        _lastUpdateTime = _scheduler.Now ;
        _inactivityTimer?.Dispose ( ) ;
        _inactivityTimer = Observable.Interval ( TimeSpan.FromSeconds ( 1 ) ,
                                                 _scheduler )
                                     .Subscribe ( _ => CheckForInactivity ( ) ,
                                                  ex =>
                                                  {
                                                      _logger.Error ( ex ,
                                                                      "Inactivity detected - desk stopped responding" ) ;
                                                      // Don't re-throw here as it would create unobserved task exception
                                                      // The exception is already logged and the monitor will be disposed
                                                  } ) ;

        _logger.Information ( "DeskMovementMonitor initialized with {Capacity} capacity, inactivity watchdog started" ,
                              capacity ) ;
    }

    protected virtual void Dispose ( bool disposing )
    {
        if ( _disposed )
            return ;

        if ( disposing )
        {
            _disposalHeightAndSpeed?.Dispose ( ) ;
            _disposalHeightAndSpeed = null ;

            _inactivityTimer?.Dispose ( ) ;
            _inactivityTimer = null ;

            _subjectInactivityDetected?.OnCompleted ( ) ;
            _subjectInactivityDetected?.Dispose ( ) ;
        }

        _disposed = true ;
    }

    /// <summary>
    ///     Finalizer to ensure unmanaged resources are released.
    /// </summary>
    ~DeskMovementMonitor ( )
    {
        Dispose ( false ) ;
    }

    private void OnHeightAndSpeedChanged ( HeightSpeedDetails details )
    {
        _lastUpdateTime = _scheduler.Now ; // Update the last activity timestamp

        _logger.Debug ( "Received height/speed update: Height={Height}, Speed={Speed}" ,
                        details.Height ,
                        details.Speed ) ;

        History.PushBack ( details ) ;

        _logger.Debug ( "History: {History}" ,
                        string.Join ( ',' ,
                                      History ) ) ;

        if ( History.Size < History.Capacity )
            return ;

        var height        = History [ 0 ].Height ;
        var allSameHeight = History.All ( x => x.Height == height ) ;

        if ( allSameHeight )
            throw new InvalidOperationException ( HeightDidNotChange ) ;

        _logger.Debug ( "Good, height changed" ) ;

        if ( History.Count >= MinimumNumberOfItems &&
             History.All ( x => x.Speed == 0 ) )
            throw new InvalidOperationException ( SpeedWasZero ) ;

        _logger.Debug ( "Good, speed changed" ) ;
    }

    private void CheckForInactivity ( )
    {
        // If we've already detected inactivity, don't check again
        if ( _inactivityDetected )
            return ;

        var elapsed = _scheduler.Now - _lastUpdateTime ;

        _logger.Verbose ( "Inactivity check: {Elapsed} seconds since last update" ,
                          elapsed.TotalSeconds ) ;

        if ( elapsed.TotalSeconds > 3 )
        {
            _inactivityDetected = true ;

            _logger.Warning ( "No height updates received for {Seconds} seconds" ,
                              elapsed.TotalSeconds ) ;

            // Publish event to trigger movement stop
            _subjectInactivityDetected.OnNext ( NoHeightUpdatesReceived ) ;

            // Dispose the timer to stop further checks
            _inactivityTimer?.Dispose ( ) ;
            _inactivityTimer = null ;
        }
    }
}
