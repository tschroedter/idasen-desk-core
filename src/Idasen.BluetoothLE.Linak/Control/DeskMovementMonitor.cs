using System.Reactive.Concurrency ;
using System.Reactive.Linq ;
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

    internal const int    DefaultCapacity    = 5 ;
    internal const string HeightDidNotChange = "Height didn't change when moving desk" ;
    internal const string SpeedWasZero       = "Speed was zero when moving desk" ;

    private readonly IDeskHeightAndSpeed _heightAndSpeed ;
    private readonly ILogger             _logger ;
    private readonly IScheduler          _scheduler ;

    private IDisposable ? _disposalHeightAndSpeed ;

    internal CircularBuffer < HeightSpeedDetails > History = // todo interface and test
        new(5) ;

    public DeskMovementMonitor ( ILogger             logger ,
                                 IScheduler          scheduler ,
                                 IDeskHeightAndSpeed heightAndSpeed )
    {
        ArgumentNullException.ThrowIfNull ( scheduler ) ;
        ArgumentNullException.ThrowIfNull ( heightAndSpeed ) ;
        ArgumentNullException.ThrowIfNull ( logger ) ;

        _logger         = logger ;
        _scheduler      = scheduler ;
        _heightAndSpeed = heightAndSpeed ;
    }

    /// <inheritdoc />
    public void Dispose ( )
    {
        _disposalHeightAndSpeed?.Dispose ( ) ;
        _disposalHeightAndSpeed = null ;
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
    }

    private void OnHeightAndSpeedChanged ( HeightSpeedDetails details )
    {
        History.PushBack ( details ) ;

        _logger.Debug ( "History: {History}" ,
                        string.Join ( ',' ,
                                      History ) ) ;

        if ( History.Size < History.Capacity )
            return ;

        var height        = History [ 0 ].Height ;
        var allSameHeight = History.All ( x => x.Height == height ) ;

        if ( allSameHeight )
            throw new ApplicationException ( HeightDidNotChange ) ;

        _logger.Debug ( "Good, height changed" ) ;

        if ( History.Count ( ) >= MinimumNumberOfItems &&
             History.All ( x => x.Speed == 0 ) )
            throw new ApplicationException ( SpeedWasZero ) ;

        _logger.Debug ( "Good, speed changed" ) ;
    }
}