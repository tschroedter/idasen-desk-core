using System.Reactive.Subjects ;
using System.Runtime.CompilerServices ;
using Autofac.Extras.DynamicProxy ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Core ;
using Idasen.BluetoothLE.Linak.Interfaces ;
using Serilog ;

namespace Idasen.BluetoothLE.Linak ;

[ Intercept ( typeof ( LogAspect ) ) ]
/// <summary>
///     Default implementation of <see cref="IErrorManager"/> that publishes error notifications via an observable stream.
/// </summary>
public class ErrorManager // todo testing, move to more general project
    : IErrorManager , IDisposable
{
    private readonly ILogger _logger ;
    private readonly ISubject < IErrorDetails > _subject ;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ErrorManager"/> class.
    /// </summary>
    public ErrorManager (
        ILogger logger ,
        ISubject < IErrorDetails > subject )
    {
        Guard.ArgumentNotNull ( logger ,
                                nameof ( logger ) ) ;
        Guard.ArgumentNotNull ( subject ,
                                nameof ( subject ) ) ;
        _logger = logger ;
        _subject = subject ;
    }

    public void Dispose ( )
    {
        // Complete the stream to release subscribers in long-running apps
        _subject.OnCompleted ( ) ;
    }

    /// <inheritdoc />
    public void Publish ( IErrorDetails details )
    {
        Guard.ArgumentNotNull ( details ,
                                nameof ( details ) ) ;

        _logger.Debug ( "Received {Details}" ,
                        details ) ;

        _subject.OnNext ( details ) ;
    }

    /// <inheritdoc />
    public void PublishForMessage ( string message ,
                                    [ CallerMemberName ] string caller = "" )
    {
        Guard.ArgumentNotNull ( message ,
                                nameof ( message ) ) ;

        _logger.Debug ( "Received {Message}" ,
                        message ) ;

        _subject.OnNext ( new ErrorDetails ( message ,
                                             caller ) ) ;
    }

    public IObservable < IErrorDetails > ErrorChanged => _subject ;
}