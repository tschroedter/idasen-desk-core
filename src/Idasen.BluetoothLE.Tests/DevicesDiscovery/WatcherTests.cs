using System.Reactive.Subjects ;
using FluentAssertions ;
using Idasen.BluetoothLE.Core.DevicesDiscovery ;
using Idasen.BluetoothLE.Core.Interfaces.DevicesDiscovery ;
using NSubstitute ;

namespace Idasen.BluetoothLE.Tests.DevicesDiscovery ;

[ TestClass ]
public class WatcherTests
{
    private ISubject < DateTime > _subject = null! ;
    private IWrapper              _wrapper = null! ;

    [ TestInitialize ]
    public void Initialize ( )
    {
        _wrapper = Substitute.For < IWrapper > ( ) ;
        _subject = Substitute.For < ISubject < DateTime > > ( ) ;
    }

    [ TestMethod ]
    [ DataRow ( Status.Created ,
                false ) ]
    [ DataRow ( Status.Started ,
                true ) ]
    [ DataRow ( Status.Stopping ,
                false ) ]
    [ DataRow ( Status.Stopped ,
                false ) ]
    [ DataRow ( Status.Aborted ,
                false ) ]
    public void IsListening_ForStatus_ReturnsExpected ( Status status ,
                                                        bool   expected )
    {
        _wrapper.Status
                .Returns ( status ) ;

        using var sut = CreateSut ( ) ;

        sut.IsListening
           .Should ( )
           .Be ( expected ) ;
    }

    [ TestMethod ]
    public void Start_ForIsListeningTrue_DoesNotCallWatcherStart ( )
    {
        var wrapper = Substitute.For < IWrapper > ( ) ;
        wrapper.Status
               .Returns ( Status.Started ) ;

        using var sut = new Watcher ( wrapper , _subject ) ;

        sut.StartListening ( ) ;

        wrapper.DidNotReceive ( )
               .StartListening ( ) ;
    }

    [ TestMethod ]
    public void Start_ForIsListeningFalse_CallsWatcherStart ( )
    {
        var wrapper = Substitute.For < IWrapper > ( ) ;
        wrapper.Status
               .Returns ( Status.Created ) ;

        using var sut = new Watcher ( wrapper , _subject ) ;

        sut.StartListening ( ) ;

        wrapper.Received ( )
               .StartListening ( ) ;
    }

    [ TestMethod ]
    public void Start_ForIsListeningFalse_PublishesStarted ( )
    {
        var wrapper = Substitute.For < IWrapper > ( ) ;
        var subject = Substitute.For < ISubject < DateTime > > ( ) ;

        wrapper.Status
               .Returns ( Status.Created ) ;

        using var sut = new Watcher ( wrapper , subject ) ;

        sut.StartListening ( ) ;

        subject.Received ( )
               .OnNext ( Arg.Any < DateTime > ( ) ) ; // Assertion added to verify that OnNext is called
    }

    [ TestMethod ]
    public void Stop_ForIsListeningTrue_CallsWatcherStop ( )
    {
        var wrapper = Substitute.For < IWrapper > ( ) ;
        wrapper.Status
               .Returns ( Status.Started ) ;

        using var sut = new Watcher ( wrapper , _subject ) ;

        sut.StopListening ( ) ;

        wrapper.Received ( )
               .StopListening ( ) ;
    }

    [ TestMethod ]
    public void Stop_ForIsListeningFalse_DoesNotCallWatcherStop ( )
    {
        var wrapper = Substitute.For < IWrapper > ( ) ;
        wrapper.Status
               .Returns ( Status.Created ) ;

        using var sut = new Watcher ( wrapper , _subject ) ;

        sut.StopListening ( ) ;

        wrapper.DidNotReceive ( )
               .StopListening ( ) ;
    }

    [ TestMethod ]
    public void Dispose_ForInvoked_DisposesWrapper ( )
    {
        var wrapper = Substitute.For < IWrapper > ( ) ;

        var sut = new Watcher ( wrapper , _subject ) ;

        sut.Dispose ( ) ;

        wrapper.Received ( )
               .Dispose ( ) ;
    }

    [ TestMethod ]
    public void Started_ForSubscribe_CallsSubscribe ( )
    {
        var subject = Substitute.For < ISubject < DateTime > > ( ) ;

        using var sut = new Watcher ( _wrapper , subject ) ;

        using var disposable = sut.Started.Subscribe ( ) ;

        subject.ReceivedWithAnyArgs ( )
               .Subscribe ( ) ;
    }

    [ TestMethod ]
    public void Stopped_ForSubscribe_CallsSubscribe ( )
    {
        var subject = Substitute.For < ISubject < DateTime > > ( ) ;
        var wrapper = Substitute.For < IWrapper > ( ) ;

        wrapper.Stopped
               .Returns ( subject ) ;

        using var sut = new Watcher ( wrapper , _subject ) ;

        using var disposable = sut.Stopped
                                  .Subscribe ( ) ;

        subject.ReceivedWithAnyArgs ( )
               .Subscribe ( ) ;
    }

    [ TestMethod ]
    public void Received_ForSubscribe_CallsSubscribe ( )
    {
        var subject = Substitute.For < ISubject < IDevice > > ( ) ;
        var wrapper = Substitute.For < IWrapper > ( ) ;

        wrapper.Received
               .Returns ( subject ) ;

        using var sut = new Watcher ( wrapper , _subject ) ;

        using var disposable = sut.Received
                                  .Subscribe ( ) ;

        subject.ReceivedWithAnyArgs ( )
               .Subscribe ( ) ;
    }

    private Watcher CreateSut ( )
    {
        return new Watcher ( _wrapper ,
                             _subject ) ;
    }
}
