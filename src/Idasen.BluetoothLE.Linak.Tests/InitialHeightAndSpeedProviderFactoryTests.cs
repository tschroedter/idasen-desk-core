using FluentAssertions ;
using Idasen.BluetoothLE.Linak.Control ;
using Idasen.BluetoothLE.Linak.Interfaces ;
using NSubstitute ;

namespace Idasen.BluetoothLE.Linak.Tests ;

[ TestClass ]
public class InitialHeightAndSpeedProviderFactoryTests
{
    [ TestMethod ]
    public void Create_ForInvoked_Instance ( )
    {
        var logger = Substitute.For< Serilog.ILogger > ( ) ;
        var scheduler = Substitute.For< System.Reactive.Concurrency.IScheduler > ( ) ;
        using var subjectFinished = new System.Reactive.Subjects.Subject< uint > ( ) ;
        var factory = new InitialHeightProvider.Factory ( ( executor , height ) => new InitialHeightProvider ( logger , scheduler , height , executor , subjectFinished ) ) ;
        var sut = new InitialHeightAndSpeedProviderFactory ( factory ) ;
        var executor = Substitute.For< IDeskCommandExecutor > ( ) ;
        var heightAndSpeed = Substitute.For< IDeskHeightAndSpeed > ( ) ;

        sut.Create ( executor ,
                     heightAndSpeed )
           .Should ( )
           .NotBeNull ( ) ;
    }
}
