using System.Reactive.Concurrency ;
using FluentAssertions ;
using NSubstitute ;

namespace Idasen.BluetoothLE.Core.Tests ;

[ TestClass ]
public class ObservableTimerFactoryTests
{
    [ TestMethod ]
    public void Create_ForInvoked_ReturnsInstance ( )
    {
        var sut = new ObservableTimerFactory ( ) ;
        var scheduler = Substitute.For< IScheduler > ( ) ;

        sut.Create ( TimeSpan.FromSeconds ( 10 ) ,
                     scheduler )
           .Should ( )
           .NotBeNull ( ) ;
    }
}
