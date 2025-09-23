using System.Reactive.Subjects ;
using System.Runtime.CompilerServices ;
using Autofac.Extras.DynamicProxy ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Linak.Interfaces ;
using Serilog ;

namespace Idasen.BluetoothLE.Linak ;

/// <inheritdoc />
[ Intercept ( typeof ( LogAspect ) ) ]
public class ErrorManager // todo testing, move to more general project
    : IErrorManager
{
    private readonly ILogger _logger ;
    private readonly ISubject < IErrorDetails > _subject ;
    private bool _disposed ;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ErrorManager" /> class.
    /// </summary>
    public ErrorManager (
        ILogger logger ,
        ISubject < IErrorDetails > subject )
    {
        ArgumentNullException.ThrowIfNull ( logger ) ;
        ArgumentNullException.ThrowIfNull ( subject ) ;

        _logger = logger ;
        _subject = subject ;
    }

    /// <inheritdoc />
    public void Dispose ( )
    {
        Dispose ( true ) ;
        GC.SuppressFinalize ( this ) ;
    }

    /// <inheritdoc />
    public void Publish ( IErrorDetails details )
    {
        ArgumentNullException.ThrowIfNull ( details ) ;

        _logger.Debug ( "Received {Details}" ,
                        details ) ;

        _subject.OnNext ( details ) ;
    }

    /// <inheritdoc />
    public void PublishForMessage ( string message ,
                                    [ CallerMemberName ] string caller = "" )
    {
        ArgumentNullException.ThrowIfNull ( message ) ;

        _logger.Debug ( "Received {Message}" ,
                        message ) ;

        _subject.OnNext ( new ErrorDetails ( message ,
                                             caller ) ) ;
    }

    // Return the underlying subject to satisfy existing unit tests
    public IObservable < IErrorDetails > ErrorChanged => _subject ;

    protected virtual void Dispose ( bool disposing )
    {
        if ( _disposed )
        {
            return ;
        }

        if ( disposing )
        {
            // Complete the stream to release subscribers in long-running apps
            try
            {
                _subject.OnCompleted ( ) ;
            }
            catch ( Exception ex )
            {
                _logger.Warning ( ex ,
                                  "Error completing ErrorChanged stream" ) ;
            }
        }

        _disposed = true ;
    }
}