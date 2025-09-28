namespace Idasen.BluetoothLE.Linak.Tests ;

using System.Reactive.Linq ;
using System.Reactive.Subjects ;
using Characteristics.Characteristics ;
using Characteristics.Interfaces.Characteristics ;
using FluentAssertions ;
using Interfaces ;
using Microsoft.Reactive.Testing ;
using NSubstitute ;
using Serilog ;

[ TestClass ]
public class DeskHeightAndSpeedTests : IDisposable
{
    private const uint DefaultHeight = 1u ;
    private const int DefaultSpeed = 2 ;

    private IRawValueToHeightAndSpeedConverter _converter = null! ;
    private ILogger _logger = null! ;
    private RawValueChangedDetails _rawDetailsDummy = null! ;
    private IReferenceOutput _referenceOutput = null! ;
    private TestScheduler _scheduler = null! ;
    private Subject < uint > _subjectHeight = null! ;
    private Subject < HeightSpeedDetails > _subjectHeightAndSpeed = null! ;
    private Subject < RawValueChangedDetails > _subjectRawHeightAndSpeed = null! ;
    private Subject < int > _subjectSpeed = null! ;

    public void Dispose ( )
    {
        _subjectHeight.OnCompleted ( ) ;
        _subjectSpeed.OnCompleted ( ) ;
        _subjectHeightAndSpeed.OnCompleted ( ) ;
        _subjectRawHeightAndSpeed.OnCompleted ( ) ;

        _subjectHeight.Dispose ( ) ;
        _subjectSpeed.Dispose ( ) ;
        _subjectHeightAndSpeed.Dispose ( ) ;
        _subjectRawHeightAndSpeed.Dispose ( ) ;
        GC.SuppressFinalize ( this ) ;
    }

    [ TestMethod ]
    public void Initialize_ForInvokedTwice_DisposesSubscriber ( )
    {
        IDisposable? subscriber = Substitute.For < IDisposable > ( ) ;
        ISubject < RawValueChangedDetails >? subject = Substitute.For < ISubject < RawValueChangedDetails > > ( ) ;

        subject.Subscribe ( Arg.Any < IObserver < RawValueChangedDetails > > ( ) )
               .Returns ( subscriber ) ;

        _referenceOutput.HeightSpeedChanged
                        .Returns ( subject ) ;

        using DeskHeightAndSpeed sut = CreateSut ( ) ;

        sut.Initialize ( ) ;

        sut.Initialize ( ) ;

        subscriber.Received ( )
                  .Dispose ( ) ;
    }

    [ TestMethod ]
    public void Dispose_ForInvokedTwice_DisposesSubscriber ( )
    {
        IDisposable? subscriber = Substitute.For < IDisposable > ( ) ;
        ISubject < RawValueChangedDetails >? subject = Substitute.For < ISubject < RawValueChangedDetails > > ( ) ;

        subject.Subscribe ( Arg.Any < IObserver < RawValueChangedDetails > > ( ) )
               .Returns ( subscriber ) ;

        _referenceOutput.HeightSpeedChanged
                        .Returns ( subject ) ;

        DeskHeightAndSpeed sut = CreateSut ( ) ;

        sut.Initialize ( ) ;

        sut.Dispose ( ) ;

        subscriber.Received ( )
                  .Dispose ( ) ;
    }

    [ TestMethod ]
    public void Dispose_ForInvokedTwice_DisposesReferenceOutput ( )
    {
        DeskHeightAndSpeed sut = CreateSut ( ) ;

        sut.Initialize ( ) ;

        sut.Dispose ( ) ;
        sut.Dispose ( ) ;

        _referenceOutput.Received ( )
                        .Dispose ( ) ;
    }

    [ TestMethod ]
    public async Task Refresh_ForInvoked_CallsReferenceOutputRefresh ( )
    {
        using DeskHeightAndSpeed sut = CreateSut ( ) ;

        await sut.Refresh ( ) ;

        await _referenceOutput.Received ( )
                              .Refresh ( ) ;
    }

    [ TestMethod ]
    public async Task Refresh_ForInvoked_CallsInitialize ( )
    {
        using DeskHeightAndSpeed sut = CreateSut ( ) ;

        await sut.Refresh ( ) ;

        // indirect test
        sut.Height
           .Should ( )
           .Be ( DefaultHeight ) ;
    }

    [ TestMethod ]
    public async Task Refresh_ForInvokedAndHeightAvailable_SetsHeight ( )
    {
        using DeskHeightAndSpeed sut = CreateSut ( ) ;

        await sut.Refresh ( ) ;

        sut.Height
           .Should ( )
           .Be ( DefaultHeight ) ;
    }

    [ TestMethod ]
    public async Task Refresh_ForInvokedAndSpeedAvailable_SetsSpeed ( )
    {
        using DeskHeightAndSpeed sut = CreateSut ( ) ;

        await sut.Refresh ( ) ;

        sut.Speed
           .Should ( )
           .Be ( DefaultSpeed ) ;
    }

    [ TestMethod ]
    public async Task Refresh_ForInvokedAndHeightNotAvailable_DoesNotSetsHeight ( )
    {
        SetTryConvert ( _converter ,
                        false ,
                        DefaultHeight ,
                        DefaultSpeed ) ;

        using DeskHeightAndSpeed sut = CreateSut ( ) ;

        await sut.Refresh ( ) ;

        sut.Height
           .Should ( )
           .Be ( 0 ) ;
    }

    [ TestMethod ]
    public async Task Refresh_ForInvokedAndSpeedNotAvailable_DoesNotSetsSpeed ( )
    {
        SetTryConvert ( _converter ,
                        false ,
                        DefaultHeight ,
                        DefaultSpeed ) ;

        using DeskHeightAndSpeed sut = CreateSut ( ) ;

        await sut.Refresh ( ) ;

        sut.Speed
           .Should ( )
           .Be ( 0 ) ;
    }

    [ TestInitialize ]
    public void Initialize ( )
    {
        _logger = Substitute.For < ILogger > ( ) ;
        _scheduler = new TestScheduler ( ) ;
        _referenceOutput = Substitute.For < IReferenceOutput > ( ) ;
        _converter = Substitute.For < IRawValueToHeightAndSpeedConverter > ( ) ;
        _subjectHeight = new Subject < uint > ( ) ;
        _subjectSpeed = new Subject < int > ( ) ;
        _subjectHeightAndSpeed = new Subject < HeightSpeedDetails > ( ) ;
        _subjectRawHeightAndSpeed = new Subject < RawValueChangedDetails > ( ) ;

        SetTryConvert ( _converter ,
                        true ,
                        DefaultHeight ,
                        DefaultSpeed ) ;

        _referenceOutput.HeightSpeedChanged
                        .Returns ( _subjectRawHeightAndSpeed ) ;

        _rawDetailsDummy = new RawValueChangedDetails ( string.Empty ,
                                                        [] ,
                                                        DateTimeOffset.Now ,
                                                        Guid.Empty ) ;
    }

    private static void SetTryConvert (
        IRawValueToHeightAndSpeedConverter converter ,
        bool result ,
        uint height ,
        int speed )
    {
        converter.TryConvert ( Arg.Any < IEnumerable < byte > > ( ) ,
                               out _ ,
                               out _ )
                 .Returns ( x =>
                            {
                                x[1] = height ;
                                x[2] = speed ;

                                return result ;
                            } ) ;
    }

    private DeskHeightAndSpeed CreateSut ( )
    {
        return new DeskHeightAndSpeed ( _logger ,
                                        _scheduler ,
                                        _referenceOutput ,
                                        _converter ,
                                        _subjectHeight ,
                                        _subjectSpeed ,
                                        _subjectHeightAndSpeed ) ;
    }

    [ TestMethod ]
    public void OnHeightSpeedChanged_ForInvoked_SetsHeight ( )
    {
        IDeskHeightAndSpeed sut = CreateSut ( ).Initialize ( ) ;

        _subjectRawHeightAndSpeed.OnNext ( _rawDetailsDummy ) ;

        _scheduler.Start ( ) ;

        sut.Height
           .Should ( )
           .Be ( DefaultHeight ) ;
    }

    [ TestMethod ]
    public void OnHeightSpeedChanged_ForInvoked_NotifiesHeightChanged ( )
    {
        var wasNotified = false ;

        IDeskHeightAndSpeed sut = CreateSut ( ).Initialize ( ) ;

        sut.HeightChanged
           .ObserveOn ( _scheduler )
           .Subscribe ( _ => wasNotified = true ) ;

        _subjectRawHeightAndSpeed.OnNext ( _rawDetailsDummy ) ;

        _scheduler.Start ( ) ;

        wasNotified
           .Should ( )
           .BeTrue ( ) ;
    }

    [ TestMethod ]
    public void OnHeightSpeedChanged_ForInvoked_NotifiesSpeedChange ( )
    {
        var wasNotified = false ;

        IDeskHeightAndSpeed sut = CreateSut ( ).Initialize ( ) ;

        sut.SpeedChanged
           .ObserveOn ( _scheduler )
           .Subscribe ( _ => wasNotified = true ) ;

        _subjectRawHeightAndSpeed.OnNext ( _rawDetailsDummy ) ;

        _scheduler.Start ( ) ;

        wasNotified
           .Should ( )
           .BeTrue ( ) ;
    }

    [ TestMethod ]
    public void OnHeightSpeedChanged_ForInvoked_NotifiesHeightSpeedChanged ( )
    {
        var wasNotified = false ;

        IDeskHeightAndSpeed sut = CreateSut ( ).Initialize ( ) ;

        sut.HeightAndSpeedChanged
           .ObserveOn ( _scheduler )
           .Subscribe ( _ => wasNotified = true ) ;

        _subjectRawHeightAndSpeed.OnNext ( _rawDetailsDummy ) ;

        _scheduler.Start ( ) ;

        wasNotified
           .Should ( )
           .BeTrue ( ) ;
    }

    [ TestMethod ]
    public void OnHeightSpeedChanged_ForInvokedAndCanNotConvert_DoesNotNotifyHeightChanged ( )
    {
        SetTryConvert ( _converter ,
                        false ,
                        DefaultHeight ,
                        DefaultSpeed ) ;

        var wasNotified = false ;

        IDeskHeightAndSpeed sut = CreateSut ( ).Initialize ( ) ;

        sut.HeightChanged
           .ObserveOn ( _scheduler )
           .Subscribe ( _ => wasNotified = true ) ;

        _subjectRawHeightAndSpeed.OnNext ( _rawDetailsDummy ) ;

        _scheduler.Start ( ) ;

        wasNotified
           .Should ( )
           .BeFalse ( ) ;
    }

    [ TestMethod ]
    public void OnHeightSpeedChanged_ForInvokedAndCanNotConvert_DoesNotNotifySpeedChanged ( )
    {
        SetTryConvert ( _converter ,
                        false ,
                        DefaultHeight ,
                        DefaultSpeed ) ;

        var wasNotified = false ;

        IDeskHeightAndSpeed sut = CreateSut ( ).Initialize ( ) ;

        sut.SpeedChanged
           .ObserveOn ( _scheduler )
           .Subscribe ( _ => wasNotified = true ) ;

        _subjectRawHeightAndSpeed.OnNext ( _rawDetailsDummy ) ;

        _scheduler.Start ( ) ;

        wasNotified
           .Should ( )
           .BeFalse ( ) ;
    }

    [ TestMethod ]
    public void OnHeightSpeedChanged_ForInvokedAndCanNotConvert_DoesNotNotifyHeightSpeedChanged ( )
    {
        SetTryConvert ( _converter ,
                        false ,
                        DefaultHeight ,
                        DefaultSpeed ) ;

        var wasNotified = false ;

        IDeskHeightAndSpeed sut = CreateSut ( ).Initialize ( ) ;

        sut.HeightAndSpeedChanged
           .ObserveOn ( _scheduler )
           .Subscribe ( _ => wasNotified = true ) ;

        _subjectRawHeightAndSpeed.OnNext ( _rawDetailsDummy ) ;

        _scheduler.Start ( ) ;

        wasNotified
           .Should ( )
           .BeFalse ( ) ;
    }
}
