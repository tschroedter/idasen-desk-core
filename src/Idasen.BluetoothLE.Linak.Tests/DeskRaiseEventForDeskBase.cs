namespace Idasen.BluetoothLE.Linak.Tests ;

using System.Reactive.Subjects ;
using FluentAssertions ;
using Interfaces ;
using Microsoft.Reactive.Testing ;
using Selkie.AutoMocking ;

[ AutoDataTestClass ]
public abstract class DeskRaiseEventForDeskBase<TSubject>
{
    public bool WasCalled { get ; private set ; }

    [ AutoDataTestMethod ]
    public void RefreshedChanged_ForEventRaised_GetsNotified (
        Desk _ ,
        IDeskConnector connector ,
        Subject < TSubject > subject ,
        TestScheduler scheduler )
    {
        ArgumentNullException.ThrowIfNull ( connector ) ;
        ArgumentNullException.ThrowIfNull ( subject ) ;
        ArgumentNullException.ThrowIfNull ( scheduler ) ;

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

    protected abstract void SetSubscription ( IDesk desk ,
                                              TestScheduler scheduler ) ;

    protected abstract void SetSubject ( IDeskConnector connector ,
                                         Subject < TSubject > subject ) ;

    public void OnRaised ( TSubject value ) => WasCalled = true ;
}
