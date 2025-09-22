using System.Reactive.Concurrency ;
using System.Reactive.Linq ;
using System.Reactive.Subjects ;
using System.Threading ;
using Autofac.Extras.DynamicProxy ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Characteristics.Common ;
using Idasen.BluetoothLE.Core ;
using Idasen.BluetoothLE.Linak.Interfaces ;
using Serilog ;

namespace Idasen.BluetoothLE.Linak.Control ;

/// <inheritdoc />
[ Intercept ( typeof ( LogAspect ) ) ]
public class InitialHeightProvider
    : IInitialHeightProvider
{
    public delegate IInitialHeightProvider Factory ( IDeskCommandExecutor executor ,
                                                     IDeskHeightAndSpeed heightAndSpeed ) ;

    private readonly IDeskCommandExecutor _executor ;
    private readonly IDeskHeightAndSpeed _heightAndSpeed ;

    private readonly ILogger _logger ;
    private readonly IScheduler _scheduler ;
    private readonly ISubject < uint > _subjectFinished ;

    // ReSharper disable once InconsistentNaming - only used for testing
    internal IDisposable? _disposalHeightAndSpeed ;
    private CancellationTokenSource? _cts ;

    /// <summary>
    ///     Initializes a new instance of the <see cref="InitialHeightProvider" /> class.
    /// </summary>
    public InitialHeightProvider ( ILogger logger ,
                                   IScheduler scheduler ,
                                   IDeskHeightAndSpeed heightAndSpeed ,
                                   IDeskCommandExecutor executor ,
                                   ISubject < uint > subjectFinished )
    {
        ArgumentNullException.ThrowIfNull ( logger ) ;
        ArgumentNullException.ThrowIfNull ( scheduler ) ;
        ArgumentNullException.ThrowIfNull ( heightAndSpeed ) ;
        ArgumentNullException.ThrowIfNull ( executor ) ;
        ArgumentNullException.ThrowIfNull ( subjectFinished ) ;

        _logger = logger ;
        _scheduler = scheduler ;
        _heightAndSpeed = heightAndSpeed ;
        _executor = executor ;
        _subjectFinished = subjectFinished ;
    }

    /// <inheritdoc />
    public void Initialize ( )
    {
        _disposalHeightAndSpeed?.Dispose ( ) ;

        _disposalHeightAndSpeed = _heightAndSpeed.HeightAndSpeedChanged
                                                 .ObserveOn ( _scheduler )
                                                 .Subscribe ( OnHeightAndSpeedChanged ,
                                                             ex => _logger.Error ( ex ,
                                                                                   "Error while observing height/speed changes" ) ) ;

        Height = _heightAndSpeed.Height ;
    }

    /// <summary>
    ///     Starts the process with cancellation support.
    /// </summary>
    public Task Start ( CancellationToken cancellationToken )
    {
        // Cancel previous run if any
        _cts?.Cancel ( ) ;
        _cts?.Dispose ( ) ;
        _cts = CancellationTokenSource.CreateLinkedTokenSource ( cancellationToken ) ;
        return StartInternalAsync ( _cts.Token ) ;
    }

    /// <inheritdoc />
    public Task Start ( ) => Start ( CancellationToken.None ) ;

    private async Task StartInternalAsync ( CancellationToken cancellationToken )
    {
        if ( _disposalHeightAndSpeed == null )
        {
            throw new NotInitializeException ( "Initialize needs to be called first" ) ;
        }

        cancellationToken.ThrowIfCancellationRequested ( ) ;

        if ( _heightAndSpeed.Height > 0 )
        {
            _logger.Information ( "Current height is {Height}" , _heightAndSpeed.Height ) ;

            HasReceivedHeightAndSpeed = true ;

            _subjectFinished.OnNext ( _heightAndSpeed.Height ) ;

            return ;
        }

        _logger.Information ( "Trying to determine current height by moving the desk" ) ;

        HasReceivedHeightAndSpeed = false ;

        try
        {
            cancellationToken.ThrowIfCancellationRequested ( ) ;

            var movedUp = await _executor.Up ( ).ConfigureAwait ( false ) ;

            cancellationToken.ThrowIfCancellationRequested ( ) ;

            var stopped = await _executor.Stop ( ).ConfigureAwait ( false ) ;

            if ( movedUp && stopped )
            {
                return ;
            }

            _logger.Error ( "Failed to move desk up and down" ) ;
        }
        catch ( OperationCanceledException )
        {
            await SafeStopAsync ( ).ConfigureAwait ( false ) ;
            throw ;
        }
    }

    private async Task SafeStopAsync ( )
    {
        try
        {
            await _executor.Stop ( ).ConfigureAwait ( false ) ;
        }
        catch ( Exception ex )
        {
            _logger.Warning ( ex , "Attempt to stop after cancellation failed" ) ;
        }
    }

    /// <inheritdoc />
    public IObservable < uint > Finished => _subjectFinished ;

    /// <inheritdoc />
    public void Dispose ( )
    {
        try
        {
            _cts?.Cancel ( ) ;
        }
        catch
        {
            // ignore
        }
        finally
        {
            _cts?.Dispose ( ) ;
            _cts = null ;
        }

        _disposalHeightAndSpeed?.Dispose ( ) ;
    }

    /// <inheritdoc />
    public uint Height { get ; private set ; }

    /// <inheritdoc />
    public bool HasReceivedHeightAndSpeed { get ; private set ; }

    private void OnHeightAndSpeedChanged ( HeightSpeedDetails details )
    {
        Height = details.Height ;

        if ( HasReceivedHeightAndSpeed )
        {
            return ;
        }

        if ( details.Height == 0 )
        {
            _logger.Information ( "Received invalid height {Height} and speed {Speed} ..." ,
                                  details.Height ,
                                  details.Speed ) ;

            return ;
        }

        _subjectFinished.OnNext ( Height ) ;
        HasReceivedHeightAndSpeed = true ;

        _logger.Information ( "Received valid height {Height}" , details.Height ) ;
    }
}