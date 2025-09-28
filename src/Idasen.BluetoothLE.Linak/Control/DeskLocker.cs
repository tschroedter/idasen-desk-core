namespace Idasen.BluetoothLE.Linak.Control ;

using System.Reactive.Concurrency ;
using System.Reactive.Linq ;
using Aop.Aspects ;
using Autofac.Extras.DynamicProxy ;
using Interfaces ;
using Serilog ;

/// <inheritdoc />
[ Intercept ( typeof ( LogAspect ) ) ]
public class DeskLocker
    : IDeskLocker
{
    public delegate IDeskLocker Factory ( IDeskMover deskMover ,
                                          IDeskCommandExecutor executor ,
                                          IDeskHeightAndSpeed heightAndSpeed ) ;

    private readonly IDeskMover _deskMover ;
    private readonly IDeskCommandExecutor _executor ;
    private readonly IDeskHeightAndSpeed _heightAndSpeed ;

    private readonly ILogger _logger ;
    private readonly IScheduler _scheduler ;

    private IDisposable? _disposalHeightAndSpeed ;

    private bool _disposed ;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DeskLocker" /> class.
    /// </summary>
    public DeskLocker ( ILogger logger ,
                        IScheduler scheduler ,
                        IDeskMover deskMover ,
                        IDeskCommandExecutor executor ,
                        IDeskHeightAndSpeed heightAndSpeed )
    {
        ArgumentNullException.ThrowIfNull ( logger ) ;
        ArgumentNullException.ThrowIfNull ( scheduler ) ;
        ArgumentNullException.ThrowIfNull ( deskMover ) ;
        ArgumentNullException.ThrowIfNull ( executor ) ;
        ArgumentNullException.ThrowIfNull ( heightAndSpeed ) ;

        _logger = logger ;
        _scheduler = scheduler ;
        _deskMover = deskMover ;
        _executor = executor ;
        _heightAndSpeed = heightAndSpeed ;
    }

    /// <inheritdoc />
    public IDeskLocker Initialize ( )
    {
        _disposalHeightAndSpeed?.Dispose ( ) ;

        _disposalHeightAndSpeed = _heightAndSpeed.HeightAndSpeedChanged
                                                 .ObserveOn ( _scheduler )
                                                 .SubscribeAsync ( OnHeightAndSpeedChanged ) ;

        return this ;
    }

    /// <inheritdoc />
    public IDeskLocker Lock ( )
    {
        _logger.Information ( "Desk locked" ) ;

        IsLocked = true ;

        return this ;
    }

    /// <inheritdoc />
    public bool IsLocked { get ; private set ; }

    /// <inheritdoc />
    public IDeskLocker Unlock ( )
    {
        _logger.Information ( "Desk unlocked" ) ;

        IsLocked = false ;

        return this ;
    }

    /// <inheritdoc />
    public void Dispose ( )
    {
        Dispose ( true ) ;
        GC.SuppressFinalize ( this ) ;
    }

    protected virtual void Dispose ( bool disposing )
    {
        if ( _disposed )
        {
            return ;
        }

        if ( disposing )
        {
            _disposalHeightAndSpeed?.Dispose ( ) ;
            _disposalHeightAndSpeed = null ;
        }

        _disposed = true ;
    }

    private async Task OnHeightAndSpeedChanged ( HeightSpeedDetails details )
    {
        if ( ! IsLocked )
        {
            return ;
        }

        if ( _deskMover.IsAllowedToMove )
        {
            return ;
        }

        _logger.Information ( "Manual move detected. Calling Stop. Details={Details}" ,
                              details ) ;

        try
        {
            await _executor.Stop ( ).ConfigureAwait ( false ) ;
        }
        catch ( Exception ex )
        {
            _logger.Error ( ex ,
                            "Error while stopping after manual move detection" ) ;
        }
    }
}
