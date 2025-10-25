using System.Reactive.Subjects ;
using FluentAssertions ;
using Idasen.BluetoothLE.Core.DevicesDiscovery ;
using Idasen.BluetoothLE.Core.Interfaces.DevicesDiscovery ;
using NSubstitute ;
using Selkie.AutoMocking ;

namespace Idasen.BluetoothLE.Core.Tests.DevicesDiscovery ;

[ AutoDataTestClass ]
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

    [ AutoDataTestMethod ]
    public void Start_ForIsListeningTrue_DoesNotCallWatcherStart ( Watcher             sut ,
                                                                   [ Freeze ] IWrapper wrapper )
    {
        wrapper.Status
               .Returns ( Status.Started ) ;

        sut.StartListening ( ) ;

        wrapper.DidNotReceive ( )
               .StartListening ( ) ;
    }

    [ AutoDataTestMethod ]
    public void Start_ForIsListeningFalse_CallsWatcherStart ( Watcher             sut ,
                                                              [ Freeze ] IWrapper wrapper )
    {
        wrapper.Status
               .Returns ( Status.Created ) ;

        sut.StartListening ( ) ;

        wrapper.Received ( )
               .StartListening ( ) ;
    }

    [ AutoDataTestMethod ]
    public void Start_ForIsListeningFalse_PublishesStarted ( Watcher                          sut ,
                                                             [ Freeze ] IWrapper              wrapper ,
                                                             [ Freeze ] ISubject < DateTime > subject )
    {
        wrapper.Status
               .Returns ( Status.Created ) ;

        sut.StartListening ( ) ;

        subject.Received ( 1 )
               .OnNext ( Arg.Any < DateTime > ( ) ) ; // Assertion added to verify the method call
    }

    [ AutoDataTestMethod ]
    public void Stop_ForIsListeningTrue_CallsWatcherStop ( Watcher             sut ,
                                                           [ Freeze ] IWrapper wrapper )
    {
        wrapper.Status
               .Returns ( Status.Started ) ;

        sut.StopListening ( ) ;

        wrapper.Received ( )
               .StopListening ( ) ;
    }

    [ AutoDataTestMethod ]
    public void Stop_ForIsListeningFalse_DoesNotCallWatcherStop ( Watcher             sut ,
                                                                  [ Freeze ] IWrapper wrapper )
    {
        wrapper.Status
               .Returns ( Status.Created ) ;

        sut.StopListening ( ) ;

        wrapper.DidNotReceive ( )
               .StopListening ( ) ;
    }

    [ AutoDataTestMethod ]
    public void Dispose_ForInvoked_DisposesWrapper ( Watcher             sut ,
                                                     [ Freeze ] IWrapper wrapper )
    {
        sut.Dispose ( ) ;

        wrapper.Received ( )
               .Dispose ( ) ;
    }

    [ AutoDataTestMethod ]
    public void Started_ForSubscribe_CallsSubscribe ( Watcher                          sut ,
                                                      [ Freeze ] ISubject < DateTime > subject )
    {
        using var disposable = sut.Started.Subscribe ( ) ;

        subject.ReceivedWithAnyArgs ( )
               .Subscribe ( ) ;
    }

    [ AutoDataTestMethod ]
    public void Stopped_ForSubscribe_CallsSubscribe ( Watcher               sut ,
                                                      ISubject < DateTime > subject ,
                                                      [ Freeze ] IWrapper   wrapper )
    {
        wrapper.Stopped
               .Returns ( subject ) ;

        using var disposable = sut.Stopped
                                  .Subscribe ( ) ;

        subject.ReceivedWithAnyArgs ( )
               .Subscribe ( ) ;
    }

    [ AutoDataTestMethod ]
    public void Received_ForSubscribe_CallsSubscribe ( Watcher              sut ,
                                                       ISubject < IDevice > subject ,
                                                       [ Freeze ] IWrapper  wrapper )
    {
        wrapper.Received
               .Returns ( subject ) ;

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