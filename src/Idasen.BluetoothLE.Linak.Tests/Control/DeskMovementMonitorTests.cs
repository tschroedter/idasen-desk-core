using System.Reactive.Subjects ;
using FluentAssertions ;
using Idasen.BluetoothLE.Linak.Control ;
using Idasen.BluetoothLE.Linak.Interfaces ;
using Microsoft.Reactive.Testing ;
using NSubstitute ;
using Serilog ;

namespace Idasen.BluetoothLE.Linak.Tests.Control ;

[ TestClass ]
public class DeskMovementMonitorTests : IDisposable
{
    private const int DefaultCapacity = 3 ;

    private HeightSpeedDetails  _details1                     = null! ;
    private HeightSpeedDetails  _details2                     = null! ;
    private HeightSpeedDetails  _details3                     = null! ;
    private HeightSpeedDetails  _details4SameHeightAsDetails1 = null! ;
    private HeightSpeedDetails  _details5SameHeightAsDetails1 = null! ;
    private HeightSpeedDetails  _details6WithSpeedZero        = null! ;
    private HeightSpeedDetails  _details7WithSpeedZero        = null! ;
    private HeightSpeedDetails  _details8WithSpeedZero        = null! ;
    private IDeskHeightAndSpeed _heightAndSpeed               = null! ;

    private ILogger                        _logger                = null! ;
    private TestScheduler                  _scheduler             = null! ;
    private Subject < HeightSpeedDetails > _subjectHeightAndSpeed = null! ;

    public void Dispose ( )
    {
        _subjectHeightAndSpeed.OnCompleted ( ) ;
        _subjectHeightAndSpeed.Dispose ( ) ;
        GC.SuppressFinalize ( this ) ;
    }

    [ TestInitialize ]
    public void Initialize ( )
    {
        _logger                = Substitute.For < ILogger > ( ) ;
        _scheduler             = new TestScheduler ( ) ;
        _heightAndSpeed        = Substitute.For < IDeskHeightAndSpeed > ( ) ;
        _subjectHeightAndSpeed = new Subject < HeightSpeedDetails > ( ) ;

        _heightAndSpeed.HeightAndSpeedChanged
                       .Returns ( _subjectHeightAndSpeed ) ;

        _details1 = new HeightSpeedDetails ( DateTimeOffset.Now ,
                                             1u ,
                                             2 ) ;
        _details2 = new HeightSpeedDetails ( DateTimeOffset.Now ,
                                             3u ,
                                             4 ) ;
        _details3 = new HeightSpeedDetails ( DateTimeOffset.Now ,
                                             5u ,
                                             6 ) ;

        _details4SameHeightAsDetails1 = new HeightSpeedDetails ( DateTimeOffset.Now ,
                                                                 1u ,
                                                                 22 ) ;
        _details5SameHeightAsDetails1 = new HeightSpeedDetails ( DateTimeOffset.Now ,
                                                                 1u ,
                                                                 33 ) ;

        _details6WithSpeedZero = new HeightSpeedDetails ( DateTimeOffset.Now ,
                                                          1u ,
                                                          0 ) ;
        _details7WithSpeedZero = new HeightSpeedDetails ( DateTimeOffset.Now ,
                                                          3u ,
                                                          0 ) ;
        _details8WithSpeedZero = new HeightSpeedDetails ( DateTimeOffset.Now ,
                                                          5u ,
                                                          0 ) ;
    }

    private DeskMovementMonitor CreateSut ( )
    {
        var sut = new DeskMovementMonitor ( _logger ,
                                            _scheduler ,
                                            _heightAndSpeed ) ;

        sut.Initialize ( DefaultCapacity ) ;

        return sut ;
    }

    [ TestMethod ]
    public void Initialize_WithZeroCapacity_ThrowsArgumentOutOfRangeException ( )
    {
        using var sut = new DeskMovementMonitor ( _logger ,
                                                   _scheduler ,
                                                   _heightAndSpeed ) ;

        var action = ( ) => sut.Initialize ( 0 ) ;

        action.Should ( )
              .Throw < ArgumentOutOfRangeException > ( )
              .WithMessage ( "*Capacity must be positive*" )
              .And.ParamName.Should ( ).Be ( "capacity" ) ;
    }

    [ TestMethod ]
    public void Initialize_WithNegativeCapacity_ThrowsArgumentOutOfRangeException ( )
    {
        using var sut = new DeskMovementMonitor ( _logger ,
                                                   _scheduler ,
                                                   _heightAndSpeed ) ;

        var action = ( ) => sut.Initialize ( -5 ) ;

        action.Should ( )
              .Throw < ArgumentOutOfRangeException > ( )
              .WithMessage ( "*Capacity must be positive*" )
              .And.ParamName.Should ( ).Be ( "capacity" ) ;
    }

    [ TestMethod ]
    public void OnHeightAndSpeedChanged_ForThreeEventsWithDifferentHeightAndSpeed_DoesNotThrow ( )
    {
        using var sut = CreateSut ( ) ;

        _subjectHeightAndSpeed.OnNext ( _details1 ) ;
        _subjectHeightAndSpeed.OnNext ( _details2 ) ;
        _subjectHeightAndSpeed.OnNext ( _details3 ) ;

        var action = ( ) => _scheduler.Start ( ) ;

        action.Should ( )
              .NotThrow < ApplicationException > ( ) ;
    }

    [ TestMethod ]
    public void OnHeightAndSpeedChanged_ForThreeEventsWithSameHeight_Throws ( )
    {
        using var sut = CreateSut ( ) ;

        _subjectHeightAndSpeed.OnNext ( _details1 ) ;
        _subjectHeightAndSpeed.OnNext ( _details4SameHeightAsDetails1 ) ;
        _subjectHeightAndSpeed.OnNext ( _details5SameHeightAsDetails1 ) ;

        var action = ( ) => _scheduler.Start ( ) ;

        action.Should ( )
              .Throw < InvalidOperationException > ( )
              .WithMessage ( DeskMovementMonitor.HeightDidNotChange ) ;
    }

    [ TestMethod ]
    public void OnHeightAndSpeedChanged_ForThreeEventsWithSpeedZero_Throws ( )
    {
        using var sut = CreateSut ( ) ;

        _subjectHeightAndSpeed.OnNext ( _details6WithSpeedZero ) ;
        _subjectHeightAndSpeed.OnNext ( _details7WithSpeedZero ) ;
        _subjectHeightAndSpeed.OnNext ( _details8WithSpeedZero ) ;

        var action = ( ) => _scheduler.Start ( ) ;

        action.Should ( )
              .Throw < InvalidOperationException > ( )
              .WithMessage ( DeskMovementMonitor.SpeedWasZero ) ;
    }

    [ TestMethod ]
    public void InactivityTimer_WhenUpdatesReceivedWithinTimeout_DoesNotThrow ( )
    {
        using var sut = CreateSut ( ) ;

        // Send an update immediately
        _subjectHeightAndSpeed.OnNext ( _details1 ) ;
        _scheduler.AdvanceBy ( TimeSpan.FromSeconds ( 1 ).Ticks ) ;

        // Advance time by 2 seconds (within 3 second timeout)
        _scheduler.AdvanceBy ( TimeSpan.FromSeconds ( 2 ).Ticks ) ;

        // Send another update
        _subjectHeightAndSpeed.OnNext ( _details2 ) ;
        _scheduler.AdvanceBy ( TimeSpan.FromSeconds ( 1 ).Ticks ) ;

        // Advance another 2 seconds (total 5 seconds, but only 2 since last update)
        _scheduler.AdvanceBy ( TimeSpan.FromSeconds ( 2 ).Ticks ) ;

        // Should not have logged a warning
        _logger.DidNotReceive ( )
               .Warning ( "No height updates received for {Seconds} seconds" ,
                          Arg.Any < double > ( ) ) ;
    }

    [ TestMethod ]
    public void InactivityTimer_WhenRegularUpdatesReceived_NeverThrows ( )
    {
        using var sut = CreateSut ( ) ;

        // Simulate regular updates every 1 second for 15 seconds
        for ( var i = 0 ; i < 15 ; i++ )
        {
            _subjectHeightAndSpeed.OnNext ( new HeightSpeedDetails ( DateTimeOffset.Now ,
                                                                     ( uint ) ( i + 1 ) ,
                                                                     10 ) ) ;
            _scheduler.AdvanceBy ( TimeSpan.FromSeconds ( 1 ).Ticks ) ;
        }

        // Should not have logged any warnings
        _logger.DidNotReceive ( )
               .Warning ( "No height updates received for {Seconds} seconds" ,
                          Arg.Any < double > ( ) ) ;
    }

    [ TestMethod ]
    public void InactivityTimer_DisposedProperly_WhenMonitorDisposed ( )
    {
        var sut = CreateSut ( ) ;

        // Send an update
        _subjectHeightAndSpeed.OnNext ( _details1 ) ;
        _scheduler.AdvanceBy ( TimeSpan.FromSeconds ( 1 ).Ticks ) ;

        // Dispose the monitor
        sut.Dispose ( ) ;

        // Advance time past the timeout
        _scheduler.AdvanceBy ( TimeSpan.FromSeconds ( 6 ).Ticks ) ;

        // Should not throw or log (timer should be disposed)
        _logger.DidNotReceive ( )
               .Warning ( "No height updates received for {Seconds} seconds" ,
                          Arg.Any < double > ( ) ) ;
    }

    [ TestMethod ]
    public void InactivityTimer_ResetsAfterEachUpdate ( )
    {
        using var sut = CreateSut ( ) ;

        // Send first update
        _subjectHeightAndSpeed.OnNext ( _details1 ) ;
        _scheduler.AdvanceBy ( TimeSpan.FromSeconds ( 1 ).Ticks ) ;

        // Wait 2 seconds (within 3 second timeout)
        _scheduler.AdvanceBy ( TimeSpan.FromSeconds ( 2 ).Ticks ) ;

        // Send another update (resets the timer)
        _subjectHeightAndSpeed.OnNext ( _details2 ) ;
        _scheduler.AdvanceBy ( TimeSpan.FromSeconds ( 1 ).Ticks ) ;

        // Wait another 2 seconds (total 5 seconds, but only 2 since last update)
        _scheduler.AdvanceBy ( TimeSpan.FromSeconds ( 2 ).Ticks ) ;

        // Should not have thrown or logged
        _logger.DidNotReceive ( )
               .Warning ( "No height updates received for {Seconds} seconds" ,
                          Arg.Any < double > ( ) ) ;
    }

    [ TestMethod ]
    public void InactivityDetected_WhenRegularUpdates_DoesNotEmit ( )
    {
        using var sut = CreateSut ( ) ;

        var receivedEvents = new List < string > ( ) ;
        sut.InactivityDetected.Subscribe ( receivedEvents.Add ) ;

        // Send regular updates
        _subjectHeightAndSpeed.OnNext ( _details1 ) ;
        _scheduler.AdvanceBy ( TimeSpan.FromSeconds ( 1 ).Ticks ) ;

        _subjectHeightAndSpeed.OnNext ( _details2 ) ;
        _scheduler.AdvanceBy ( TimeSpan.FromSeconds ( 1 ).Ticks ) ;

        _subjectHeightAndSpeed.OnNext ( _details3 ) ;
        _scheduler.AdvanceBy ( TimeSpan.FromSeconds ( 1 ).Ticks ) ;

        // Should not have emitted any inactivity events
        receivedEvents.Should ( ).BeEmpty ( ) ;
    }

    [ TestMethod ]
    public void InactivityDetected_Observable_IsNotNull ( )
    {
        using var sut = CreateSut ( ) ;

        // The InactivityDetected observable should be available
        sut.InactivityDetected.Should ( ).NotBeNull ( ) ;
    }

    [ TestMethod ]
    public void InactivityDetected_CanSubscribe_WithoutError ( )
    {
        using var sut = CreateSut ( ) ;

        // Should be able to subscribe to the observable
        var action = ( ) => sut.InactivityDetected.Subscribe ( _ => { } ) ;

        action.Should ( ).NotThrow ( ) ;
    }

    [ TestMethod ]
    public void InactivityDetected_WhenDisposed_CompletesObservable ( )
    {
        var sut = CreateSut ( ) ;

        var completed = false ;
        sut.InactivityDetected.Subscribe (
            _ => { } ,
            ( ) => completed = true ) ;

        // Send an update
        _subjectHeightAndSpeed.OnNext ( _details1 ) ;
        _scheduler.AdvanceBy ( TimeSpan.FromSeconds ( 1 ).Ticks ) ;

        // Dispose the monitor
        sut.Dispose ( ) ;

        // The observable should complete
        completed.Should ( ).BeTrue ( ) ;
    }

    [ TestMethod ]
    public void InactivityDetected_WhenNoUpdatesFor3Seconds_EmitsEvent ( )
    {
        using var sut = CreateSut ( ) ;

        var receivedEvents = new List < string > ( ) ;
        sut.InactivityDetected.Subscribe ( receivedEvents.Add ) ;

        // Send initial update
        _subjectHeightAndSpeed.OnNext ( _details1 ) ;
        _scheduler.AdvanceBy ( TimeSpan.FromSeconds ( 1 ).Ticks ) ;

        // Advance time by 4 seconds (exceeds 3 second timeout)
        _scheduler.AdvanceBy ( TimeSpan.FromSeconds ( 4 ).Ticks ) ;

        // Should have emitted an inactivity event
        receivedEvents.Should ( ).HaveCount ( 1 ) ;
        receivedEvents [ 0 ].Should ( ).Be ( DeskMovementMonitor.NoHeightUpdatesReceived ) ;
    }

    [ TestMethod ]
    public void InactivityTimer_WhenNoUpdatesFor3Seconds_LogsWarning ( )
    {
        using var sut = CreateSut ( ) ;

        // Send initial update
        _subjectHeightAndSpeed.OnNext ( _details1 ) ;
        _scheduler.AdvanceBy ( TimeSpan.FromSeconds ( 1 ).Ticks ) ;

        // Advance time by 4 seconds (exceeds 3 second timeout)
        _scheduler.AdvanceBy ( TimeSpan.FromSeconds ( 4 ).Ticks ) ;

        // Should have logged a warning
        _logger.Received ( 1 )
               .Warning ( "No height updates received for {Seconds} seconds" ,
                          Arg.Is < double > ( s => s > 3.0 ) ) ;
    }

    [ TestMethod ]
    public void InactivityDetected_OnlyEmitsOnce_WhenTimeoutExceeded ( )
    {
        using var sut = CreateSut ( ) ;

        var receivedEvents = new List < string > ( ) ;
        sut.InactivityDetected.Subscribe ( receivedEvents.Add ) ;

        // Send initial update
        _subjectHeightAndSpeed.OnNext ( _details1 ) ;
        _scheduler.AdvanceBy ( TimeSpan.FromSeconds ( 1 ).Ticks ) ;

        // Advance time by 10 seconds (well beyond 3 second timeout)
        // This should only emit one event, not multiple
        _scheduler.AdvanceBy ( TimeSpan.FromSeconds ( 10 ).Ticks ) ;

        // Should have emitted exactly one inactivity event
        receivedEvents.Should ( ).HaveCount ( 1 ) ;
        receivedEvents [ 0 ].Should ( ).Be ( DeskMovementMonitor.NoHeightUpdatesReceived ) ;

        // Should only have logged warning once
        _logger.Received ( 1 )
               .Warning ( "No height updates received for {Seconds} seconds" ,
                          Arg.Any < double > ( ) ) ;
    }

    [ TestMethod ]
    public void Initialize_ResetsInactivityDetection_ForNewCycle ( )
    {
        using var sut = CreateSut ( ) ;

        var receivedEvents = new List < string > ( ) ;
        sut.InactivityDetected.Subscribe ( receivedEvents.Add ) ;

        // First cycle: Send update and wait for timeout
        _subjectHeightAndSpeed.OnNext ( _details1 ) ;
        _scheduler.AdvanceBy ( TimeSpan.FromSeconds ( 1 ).Ticks ) ;
        _scheduler.AdvanceBy ( TimeSpan.FromSeconds ( 4 ).Ticks ) ;

        // Should have emitted one event
        receivedEvents.Should ( ).HaveCount ( 1 ) ;

        // Re-initialize for a new movement cycle
        sut.Initialize ( ) ;

        // Second cycle: Send update and wait for timeout
        _subjectHeightAndSpeed.OnNext ( _details2 ) ;
        _scheduler.AdvanceBy ( TimeSpan.FromSeconds ( 1 ).Ticks ) ;
        _scheduler.AdvanceBy ( TimeSpan.FromSeconds ( 4 ).Ticks ) ;

        // Should have emitted a second event (one per cycle)
        receivedEvents.Should ( ).HaveCount ( 2 ) ;
        receivedEvents [ 1 ].Should ( ).Be ( DeskMovementMonitor.NoHeightUpdatesReceived ) ;
    }

    [ TestMethod ]
    public void InactivityDetected_AfterDetection_SubsequentChecksDoNotLogAgain ( )
    {
        using var sut = CreateSut ( ) ;

        var receivedEvents = new List < string > ( ) ;
        sut.InactivityDetected.Subscribe ( receivedEvents.Add ) ;

        // Send initial update
        _subjectHeightAndSpeed.OnNext ( _details1 ) ;
        _scheduler.AdvanceBy ( TimeSpan.FromSeconds ( 1 ).Ticks ) ;

        // Advance time by 4 seconds to trigger inactivity
        _scheduler.AdvanceBy ( TimeSpan.FromSeconds ( 4 ).Ticks ) ;

        // Verify first detection
        receivedEvents.Should ( ).HaveCount ( 1 ) ;
        _logger.Received ( 1 )
               .Warning ( "No height updates received for {Seconds} seconds" ,
                          Arg.Any < double > ( ) ) ;

        // Clear received calls to verify no additional calls
        _logger.ClearReceivedCalls ( ) ;

        // Continue advancing time (timer keeps ticking, but should early-return)
        _scheduler.AdvanceBy ( TimeSpan.FromSeconds ( 10 ).Ticks ) ;

        // Should still only have one event (no duplicates)
        receivedEvents.Should ( ).HaveCount ( 1 ) ;

        // Should not have logged additional warnings (early return prevents it)
        _logger.DidNotReceive ( )
               .Warning ( "No height updates received for {Seconds} seconds" ,
                          Arg.Any < double > ( ) ) ;
    }
}
