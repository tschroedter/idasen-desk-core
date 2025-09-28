namespace Idasen.BluetoothLE.Linak.Tests ;

using System.Reactive.Subjects ;
using FluentAssertions ;
using Interfaces ;
using NSubstitute ;
using Serilog ;

[ TestClass ]
public class ErrorManagerTests
{
    [ TestMethod ]
    public void Publish_ForValidDetails_PushesToSubject ( )
    {
        ILogger? logger = Substitute.For < ILogger > ( ) ;
        ISubject < IErrorDetails >? subject = Substitute.For < ISubject < IErrorDetails > > ( ) ;
        var sut = new ErrorManager ( logger ,
                                     subject ) ;
        IErrorDetails? details = Substitute.For < IErrorDetails > ( ) ;

        sut.Publish ( details ) ;

        subject.Received ( 1 ).OnNext ( details ) ;
    }

    [ TestMethod ]
    public void PublishForMessage_ForValidMessage_PushesConstructedDetails ( )
    {
        ILogger? logger = Substitute.For < ILogger > ( ) ;
        ISubject < IErrorDetails >? subject = Substitute.For < ISubject < IErrorDetails > > ( ) ;
        var sut = new ErrorManager ( logger ,
                                     subject ) ;

        const string caller = "caller" ;
        sut.PublishForMessage ( "message" ,
                                caller ) ;

        subject.Received ( 1 )
               .OnNext ( Arg.Is < IErrorDetails > ( d => d.Message == "message" && d.Caller == caller ) ) ;
    }

    [ TestMethod ]
    public void ErrorChanged_ReturnsSubject ( )
    {
        ILogger? logger = Substitute.For < ILogger > ( ) ;
        ISubject < IErrorDetails >? subject = Substitute.For < ISubject < IErrorDetails > > ( ) ;
        var sut = new ErrorManager ( logger ,
                                     subject ) ;

        sut.ErrorChanged.Should ( ).Be ( subject ) ;
    }
}
