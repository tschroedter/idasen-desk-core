﻿using System.Reactive.Linq ;
using System.Reactive.Subjects ;
using FluentAssertions ;
using Idasen.BluetoothLE.Characteristics.Characteristics ;
using Idasen.BluetoothLE.Characteristics.Interfaces.Characteristics ;
using Idasen.BluetoothLE.Linak.Interfaces ;
using Microsoft.Reactive.Testing ;
using Microsoft.VisualStudio.TestTools.UnitTesting ;
using NSubstitute ;
using Serilog ;

namespace Idasen.BluetoothLE.Linak.Tests
{
    [ TestClass ]
    public class DeskHeightAndSpeedTests
    {
        [ TestMethod ]
        public void Initialize_ForInvokedTwice_DisposesSubscriber ( )
        {
            var subscriber = Substitute.For < IDisposable > ( ) ;
            var subject    = Substitute.For < ISubject < RawValueChangedDetails > > ( ) ;

            subject.Subscribe ( Arg.Any < IObserver < RawValueChangedDetails > > ( ) )
                   .Returns ( subscriber ) ;

            _referenceOutput.HeightSpeedChanged
                            .Returns ( subject ) ;

            using var sut = CreateSut ( ) ;

            sut.Initialize ( ) ;

            sut.Initialize ( ) ;

            subscriber.Received ( )
                      .Dispose ( ) ;
        }

        [ TestMethod ]
        public void Dispose_ForInvokedTwice_DisposesSubscriber ( )
        {
            var subscriber = Substitute.For < IDisposable > ( ) ;
            var subject    = Substitute.For < ISubject < RawValueChangedDetails > > ( ) ;

            subject.Subscribe ( Arg.Any < IObserver < RawValueChangedDetails > > ( ) )
                   .Returns ( subscriber ) ;

            _referenceOutput.HeightSpeedChanged
                            .Returns ( subject ) ;

            var sut = CreateSut ( ) ;

            sut.Initialize ( ) ;

            sut.Dispose ( ) ;

            subscriber.Received ( )
                      .Dispose ( ) ;
        }

        [ TestMethod ]
        public void Dispose_ForInvokedTwice_DisposesReferenceOutput ( )
        {
            var sut = CreateSut ( ) ;

            sut.Initialize ( ) ;

            sut.Dispose ( ) ;
            sut.Dispose ( ) ;

            _referenceOutput.Received ( )
                            .Dispose ( ) ;
        }

        [ TestMethod ]
        public async Task Refresh_ForInvoked_CallsReferenceOutputRefresh ( )
        {
            using var sut = CreateSut ( ) ;

            await sut.Refresh ( ) ;

            await _referenceOutput.Received ( )
                                  .Refresh ( ) ;
        }

        [ TestMethod ]
        public async Task Refresh_ForInvoked_CallsInitialize ( )
        {
            using var sut = CreateSut ( ) ;

            await sut.Refresh ( ) ;

            // indirect test
            sut.Height
               .Should ( )
               .Be ( DefaultHeight ) ;
        }

        [ TestMethod ]
        public async Task Refresh_ForInvokedAndHeightAvailable_SetsHeight ( )
        {
            using var sut = CreateSut ( ) ;

            await sut.Refresh ( ) ;

            sut.Height
               .Should ( )
               .Be ( DefaultHeight ) ;
        }

        [ TestMethod ]
        public async Task Refresh_ForInvokedAndSpeedAvailable_SetsSpeed ( )
        {
            using var sut = CreateSut ( ) ;

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

            using var sut = CreateSut ( ) ;

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

            using var sut = CreateSut ( ) ;

            await sut.Refresh ( ) ;

            sut.Speed
               .Should ( )
               .Be ( 0 ) ;
        }

        [ TestInitialize ]
        public void Initialize ( )
        {
            _logger                   = Substitute.For < ILogger > ( ) ;
            _scheduler                = new TestScheduler ( ) ;
            _referenceOutput          = Substitute.For < IReferenceOutput > ( ) ;
            _converter                = Substitute.For < IRawValueToHeightAndSpeedConverter > ( ) ;
            _subjectHeight            = new Subject < uint > ( ) ;
            _subjectSpeed             = new Subject < int > ( ) ;
            _subjectHeightAndSpeed    = new Subject < HeightSpeedDetails > ( ) ;
            _subjectRawHeightAndSpeed = new Subject < RawValueChangedDetails > ( ) ;

            SetTryConvert ( _converter ,
                            true ,
                            DefaultHeight ,
                            DefaultSpeed ) ;

            _referenceOutput.HeightSpeedChanged
                            .Returns ( _subjectRawHeightAndSpeed ) ;

            _rawDetailsDummy = new RawValueChangedDetails ( string.Empty ,
                                                            Array.Empty < byte > ( ) ,
                                                            DateTimeOffset.Now ,
                                                            Guid.Empty ) ;
        }

        private void SetTryConvert (
            IRawValueToHeightAndSpeedConverter converter ,
            bool                               result ,
            uint                               height ,
            int                                speed )
        {
            converter.TryConvert ( Arg.Any < IEnumerable < byte > > ( ) ,
                                   out var _ ,
                                   out var _ )
                     .Returns ( x =>
                                {
                                    x [ 1 ] = height ;
                                    x [ 2 ] = speed ;

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
            var sut = CreateSut ( ).Initialize ( ) ;

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

            var sut = CreateSut ( ).Initialize ( ) ;

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

            var sut = CreateSut ( ).Initialize ( ) ;

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

            var sut = CreateSut ( ).Initialize ( ) ;

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

            var sut = CreateSut ( ).Initialize ( ) ;

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

            var sut = CreateSut ( ).Initialize ( ) ;

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

            var sut = CreateSut ( ).Initialize ( ) ;

            sut.HeightAndSpeedChanged
               .ObserveOn ( _scheduler )
               .Subscribe ( _ => wasNotified = true ) ;

            _subjectRawHeightAndSpeed.OnNext ( _rawDetailsDummy ) ;

            _scheduler.Start ( ) ;

            wasNotified
               .Should ( )
               .BeFalse ( ) ;
        }

        private const uint DefaultHeight = 1u ;
        private const int  DefaultSpeed  = 2 ;

        private IRawValueToHeightAndSpeedConverter _converter                = null! ;
        private ILogger                            _logger                   = null! ;
        private RawValueChangedDetails             _rawDetailsDummy          = null! ;
        private IReferenceOutput                   _referenceOutput          = null! ;
        private TestScheduler                      _scheduler                = null! ;
        private ISubject < uint >                  _subjectHeight            = null! ;
        private Subject < HeightSpeedDetails >     _subjectHeightAndSpeed    = null! ;
        private Subject < RawValueChangedDetails > _subjectRawHeightAndSpeed = null! ;
        private ISubject < int >                   _subjectSpeed             = null! ;
    }
}