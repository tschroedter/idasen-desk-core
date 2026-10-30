using System.Reactive.Subjects ;
using FluentAssertions ;
using Idasen.BluetoothLE.Linak.Interfaces ;
using Microsoft.Reactive.Testing ;
using NSubstitute ;

namespace Idasen.BluetoothLE.Linak.Tests ;

[ TestClass ]
public abstract class DeskRaiseEventForDeskBase < TSubject >
{
    public bool WasCalled { get ; private set ; }

    [ TestMethod ]
    public void RefreshedChanged_ForEventRaised_GetsNotified ( )
    {
        var connector = Substitute.For< IDeskConnector > ( ) ;
        using var subject = new Subject< TSubject > ( ) ;
        var scheduler = new TestScheduler ( ) ;

        SetSubject ( connector ,
                     subject ) ;

        using var sut = new Desk ( connector ) ;

        SetSubscription ( sut ,
                          scheduler ) ;

        subject.OnNext ( default! ) ;

        scheduler.Start ( ) ;

        WasCalled.Should ( )
                 .BeTrue ( ) ;
    }

    protected abstract void SetSubscription ( IDesk         desk ,
                                              TestScheduler scheduler ) ;

    protected abstract void SetSubject ( IDeskConnector       connector ,
                                         Subject < TSubject > subject ) ;

    public void OnRaised ( TSubject value )
    {
        WasCalled = true ;
    }
}
