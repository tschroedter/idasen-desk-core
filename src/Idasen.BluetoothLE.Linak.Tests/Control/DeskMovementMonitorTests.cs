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
        sut.Start ( ) ; // Start the inactivity watchdog

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
    public void InactivityTimeoutSeconds_DefaultValue_IsOne ( )
    {
        using var sut = new DeskMovementMonitor ( _logger ,
                                                   _scheduler ,
                                                   _heightAndSpeed ) ;

        sut.InactivityTimeoutSeconds.Should ( ).Be ( 1 ) ;
    }

    [ TestMethod ]
    public void InactivityTimeoutSeconds_SetToPositiveValue_Succeeds ( )
    {
        using var sut = new DeskMovementMonitor ( _logger ,
                                                   _scheduler ,
                                                   _heightAndSpeed ) ;

        sut.InactivityTimeoutSeconds = 5 ;

        sut.InactivityTimeoutSeconds.Should ( ).Be ( 5 ) ;
    }

    [ TestMethod ]
    public void InactivityTimeoutSeconds_SetToZero_ThrowsArgumentOutOfRangeException ( )
    {
        using var sut = new DeskMovementMonitor ( _logger ,
                                                   _scheduler ,
                                                   _heightAndSpeed ) ;

        var action = ( ) => sut.InactivityTimeoutSeconds = 0 ;

        action.Should ( )
              .Throw < ArgumentOutOfRangeException > ( )
              .WithMessage ( "*InactivityTimeoutSeconds must be greater than 0.*" )
              .And.ParamName.Should ( ).Be ( "value" ) ;
    }

    [ TestMethod ]
    public void InactivityTimeoutSeconds_SetToNegative_ThrowsArgumentOutOfRangeException ( )
    {
        using var sut = new DeskMovementMonitor ( _logger ,
                                                   _scheduler ,
                                                   _heightAndSpeed ) ;

        var action = ( ) => sut.InactivityTimeoutSeconds = -1 ;

        action.Should ( )
              .Throw < ArgumentOutOfRangeException > ( )
              .WithMessage ( "*InactivityTimeoutSeconds must be greater than 0.*" )
              .And.ParamName.Should ( ).Be ( "value" ) ;
    }

    [ TestMethod ]
    public void InactivityTimeoutSeconds_WhenChanged_AffectsTimerInterval ( )
    {
        using var sut = new DeskMovementMonitor ( _logger ,
                                                   _scheduler ,
                                                   _heightAndSpeed ) ;

        // Set custom timeout before starting
        sut.InactivityTimeoutSeconds = 3 ;
        sut.Initialize ( DefaultCapacity ) ;
        sut.Start ( ) ;

        var receivedEvents = new List < string > ( ) ;
        sut.InactivityDetected.Subscribe ( receivedEvents.Add ) ;

        // Send initial update
        _subjectHeightAndSpeed.OnNext ( _details1 ) ;
        _scheduler.AdvanceBy ( TimeSpan.FromSeconds ( 3 ).Ticks ) ;

        // Advance by 2 seconds (still within 3 second timeout)
        _scheduler.AdvanceBy ( TimeSpan.FromSeconds ( 2 ).Ticks ) ;

        // Should not have emitted yet
        receivedEvents.Should ( ).BeEmpty ( ) ;

        // Advance by 2 more seconds (now exceeds 3 second timeout)
        _scheduler.AdvanceBy ( TimeSpan.FromSeconds ( 2 ).Ticks ) ;

        // Should have emitted an inactivity event
        receivedEvents.Should ( ).HaveCount ( 1 ) ;
    }

    [ TestMethod ]
    public void InactivityTimeoutSeconds_WhenChangedBeforeStart_UsesNewInterval ( )
    {
        using var sut = new DeskMovementMonitor ( _logger ,
                                                   _scheduler ,
                                                   _heightAndSpeed ) ;

        // Set to 5 seconds before starting
        sut.InactivityTimeoutSeconds = 5 ;
        sut.Initialize ( DefaultCapacity ) ;
        sut.Start ( ) ;

        var receivedEvents = new List < string > ( ) ;
        sut.InactivityDetected.Subscribe ( receivedEvents.Add ) ;

        // Send initial update
        _subjectHeightAndSpeed.OnNext ( _details1 ) ;
        _scheduler.AdvanceBy ( TimeSpan.FromSeconds ( 5 ).Ticks ) ;

        // Advance by 4 seconds (still within 5 second timeout)
        _scheduler.AdvanceBy ( TimeSpan.FromSeconds ( 4 ).Ticks ) ;

        // Should not have detected inactivity yet
        receivedEvents.Should ( ).BeEmpty ( ) ;

        // Advance by 2 more seconds (now exceeds 5 second timeout)
        _scheduler.AdvanceBy ( TimeSpan.FromSeconds ( 2 ).Ticks ) ;

        // Should have detected inactivity
        receivedEvents.Should ( ).HaveCount ( 1 ) ;
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
        _scheduler.AdvanceBy ( TimeSpan.FromMilliseconds ( 100 ).Ticks ) ;

        // Advance time by 0.5 seconds (within 1 second timeout)
        _scheduler.AdvanceBy ( TimeSpan.FromMilliseconds ( 500 ).Ticks ) ;

        // Send another update (resets timer)
        _subjectHeightAndSpeed.OnNext ( _details2 ) ;
        _scheduler.AdvanceBy ( TimeSpan.FromMilliseconds ( 100 ).Ticks ) ;

        // Advance another 0.5 seconds (only 0.5 since last update)
        _scheduler.AdvanceBy ( TimeSpan.FromMilliseconds ( 500 ).Ticks ) ;

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
        _scheduler.AdvanceBy ( TimeSpan.FromMilliseconds ( 100 ).Ticks ) ;

        // Wait 0.5 seconds (within 1 second timeout)
        _scheduler.AdvanceBy ( TimeSpan.FromMilliseconds ( 500 ).Ticks ) ;

        // Send another update (resets the timer)
        _subjectHeightAndSpeed.OnNext ( _details2 ) ;
        _scheduler.AdvanceBy ( TimeSpan.FromMilliseconds ( 100 ).Ticks ) ;

        // Wait another 0.5 seconds (only 0.5 since last update)
        _scheduler.AdvanceBy ( TimeSpan.FromMilliseconds ( 500 ).Ticks ) ;

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
    public void InactivityDetected_WhenNoUpdatesFor1Second_EmitsEvent ( )
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
    public void InactivityTimer_WhenNoUpdatesFor1Second_LogsWarning ( )
    {
        using var sut = CreateSut ( ) ;

        // Send initial update
        _subjectHeightAndSpeed.OnNext ( _details1 ) ;
        _scheduler.AdvanceBy ( TimeSpan.FromSeconds ( 1 ).Ticks ) ;

        // Advance time by 2 seconds (exceeds 1 second timeout)
        _scheduler.AdvanceBy ( TimeSpan.FromSeconds ( 2 ).Ticks ) ;

        // Should have logged a warning
        _logger.Received ( 1 )
               .Warning ( "No height updates received for {Seconds} seconds" ,
                          Arg.Is < double > ( s => s > 1.0 ) ) ;
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

        // Advance time by 5 seconds (well beyond 1 second timeout)
        // This should only emit one event, not multiple
        _scheduler.AdvanceBy ( TimeSpan.FromSeconds ( 5 ).Ticks ) ;

        // Should have emitted exactly one inactivity event
        receivedEvents.Should ( ).HaveCount ( 1 ) ;
        receivedEvents [ 0 ].Should ( ).Be ( DeskMovementMonitor.NoHeightUpdatesReceived ) ;

        // Should only have logged warning once
        _logger.Received ( 1 )
               .Warning ( "No height updates received for {Seconds} seconds" ,
                          Arg.Any < double > ( ) ) ;
    }

    [ TestMethod ]
    public void Start_ResetsInactivityDetection_ForNewCycle ( )
    {
        using var sut = CreateSut ( ) ;

        var receivedEvents = new List < string > ( ) ;
        sut.InactivityDetected.Subscribe ( receivedEvents.Add ) ;

        // First cycle: Send update and wait for timeout
        _subjectHeightAndSpeed.OnNext ( _details1 ) ;
        _scheduler.AdvanceBy ( TimeSpan.FromSeconds ( 1 ).Ticks ) ;
        _scheduler.AdvanceBy ( TimeSpan.FromSeconds ( 2 ).Ticks ) ;

        // Should have emitted one event
        receivedEvents.Should ( ).HaveCount ( 1 ) ;

        // Start a new movement cycle (resets inactivity detection)
        sut.Start ( ) ;

        // Second cycle: Send update and wait for timeout
        _subjectHeightAndSpeed.OnNext ( _details2 ) ;
        _scheduler.AdvanceBy ( TimeSpan.FromSeconds ( 1 ).Ticks ) ;
        _scheduler.AdvanceBy ( TimeSpan.FromSeconds ( 2 ).Ticks ) ;

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

        // Advance time by 2 seconds to trigger inactivity
        _scheduler.AdvanceBy ( TimeSpan.FromSeconds ( 2 ).Ticks ) ;

        // Verify first detection
        receivedEvents.Should ( ).HaveCount ( 1 ) ;
        _logger.Received ( 1 )
               .Warning ( "No height updates received for {Seconds} seconds" ,
                          Arg.Any < double > ( ) ) ;

        // Clear received calls to verify no additional calls
        _logger.ClearReceivedCalls ( ) ;

        // Continue advancing time (timer keeps ticking, but should early-return)
        _scheduler.AdvanceBy ( TimeSpan.FromSeconds ( 5 ).Ticks ) ;

        // Should still only have one event (no duplicates)
        receivedEvents.Should ( ).HaveCount ( 1 ) ;

        // Should not have logged additional warnings (early return prevents it)
        _logger.DidNotReceive ( )
               .Warning ( "No height updates received for {Seconds} seconds" ,
                          Arg.Any < double > ( ) ) ;
    }

    [ TestMethod ]
    public void Initialize_WhenCalledWithoutPriorInitialize_DoesNotThrow ( )
    {
        // This covers the null branch of _inactivityTimer?.Dispose() on line 87
        using var sut = new DeskMovementMonitor ( _logger ,
                                                   _scheduler ,
                                                   _heightAndSpeed ) ;

        var action = ( ) => sut.Initialize ( ) ;

        action.Should ( ).NotThrow ( ) ;
    }

    [ TestMethod ]
    public void Initialize_WhenCalledMultipleTimes_DisposesOldTimer ( )
    {
        // This covers the non-null branch of _inactivityTimer?.Dispose() on line 87
        using var sut = new DeskMovementMonitor ( _logger ,
                                                   _scheduler ,
                                                   _heightAndSpeed ) ;

        // First initialization creates timer
        sut.Initialize ( ) ;

        // Second initialization should dispose old timer and create new one
        var action = ( ) => sut.Initialize ( ) ;

        action.Should ( ).NotThrow ( ) ;
    }

    [ TestMethod ]
    public void Dispose_WhenCalledWithoutInitialize_DoesNotThrow ( )
    {
        // This covers the null branches on lines 113-114
        using var sut = new DeskMovementMonitor ( _logger ,
                                                   _scheduler ,
                                                   _heightAndSpeed ) ;

        var action = ( ) => sut.Dispose ( ) ;

        action.Should ( ).NotThrow ( ) ;
    }

    [ TestMethod ]
    public void OnHeightAndSpeedChanged_WithLessThanMinimumItems_DoesNotCheckSpeed ( )
    {
        // This covers the false branch of "History.Count >= MinimumNumberOfItems" on line 151
        // Create monitor with capacity 5 (larger than MinimumNumberOfItems which is 3)
        using var sut = new DeskMovementMonitor ( _logger ,
                                                   _scheduler ,
                                                   _heightAndSpeed ) ;
        sut.Initialize ( 5 ) ;

        // Send only 2 items with different heights but speed zero
        var details1 = new HeightSpeedDetails ( DateTimeOffset.Now , 1u , 0 ) ;
        var details2 = new HeightSpeedDetails ( DateTimeOffset.Now , 2u , 0 ) ;

        _subjectHeightAndSpeed.OnNext ( details1 ) ;
        _subjectHeightAndSpeed.OnNext ( details2 ) ;

        // Should not throw even though speed is zero, because we haven't reached MinimumNumberOfItems
        var action = ( ) => _scheduler.Start ( ) ;

        action.Should ( ).NotThrow ( ) ;
    }

    [ TestMethod ]
    public void OnHeightAndSpeedChanged_WithMinimumItems_AndNonZeroSpeed_DoesNotThrow ( )
    {
        // This covers the branch where History.Count >= MinimumNumberOfItems is true
        // but History.All(x => x.Speed == 0) is false
        using var sut = CreateSut ( ) ;

        // Send 3 items with different heights and at least one non-zero speed
        var details1 = new HeightSpeedDetails ( DateTimeOffset.Now , 1u , 0 ) ;
        var details2 = new HeightSpeedDetails ( DateTimeOffset.Now , 2u , 0 ) ;
        var details3 = new HeightSpeedDetails ( DateTimeOffset.Now , 3u , 5 ) ; // Non-zero speed

        _subjectHeightAndSpeed.OnNext ( details1 ) ;
        _subjectHeightAndSpeed.OnNext ( details2 ) ;
        _subjectHeightAndSpeed.OnNext ( details3 ) ;

        var action = ( ) => _scheduler.Start ( ) ;

        action.Should ( ).NotThrow ( ) ;
    }

    [ TestMethod ]
    public void StopWatchdog_StopsInactivityTimer ( )
    {
        // Arrange
        using var sut = CreateSut ( ) ;
        var receivedEvents = new List < string > ( ) ;
        sut.InactivityDetected.Subscribe ( receivedEvents.Add ) ;

        // Act - stop the watchdog before timeout
        _subjectHeightAndSpeed.OnNext ( _details1 ) ;
        _scheduler.AdvanceBy ( TimeSpan.FromSeconds ( 1 ).Ticks ) ;
        sut.StopWatchdog ( ) ;
        _scheduler.AdvanceBy ( TimeSpan.FromSeconds ( 5 ).Ticks ) ;

        // Assert - no inactivity event should be emitted after stopping
        receivedEvents.Should ( ).BeEmpty ( "watchdog was stopped before timeout" ) ;
    }

    [ TestMethod ]
    public void StopWatchdog_CanBeCalledMultipleTimes ( )
    {
        // Arrange
        using var sut = CreateSut ( ) ;

        // Act
        var act = ( ) =>
        {
            sut.StopWatchdog ( ) ;
            sut.StopWatchdog ( ) ;
            sut.StopWatchdog ( ) ;
        } ;

        // Assert
        act.Should ( ).NotThrow ( "multiple calls to StopWatchdog should be safe" ) ;
    }

    [ TestMethod ]
    public void StopWatchdog_ThenStart_RestartsWatchdog ( )
    {
        // Arrange
        using var sut = CreateSut ( ) ;
        var receivedEvents = new List < string > ( ) ;
        sut.InactivityDetected.Subscribe ( receivedEvents.Add ) ;

        // Act - start, stop, then start again
        _subjectHeightAndSpeed.OnNext ( _details1 ) ;
        _scheduler.AdvanceBy ( TimeSpan.FromSeconds ( 1 ).Ticks ) ;
        sut.StopWatchdog ( ) ;

        // Start a new cycle
        sut.Start ( ) ;
        _scheduler.AdvanceBy ( TimeSpan.FromSeconds ( 2 ).Ticks ) ;

        // Assert - should emit inactivity event after restart
        receivedEvents.Should ( ).HaveCount ( 1 ) ;
        receivedEvents [ 0 ].Should ( ).Be ( DeskMovementMonitor.NoHeightUpdatesReceived ) ;
    }

    [ TestMethod ]
    public void StopWatchdog_WithoutStart_DoesNotThrow ( )
    {
        // Arrange
        var sut = new DeskMovementMonitor ( _logger ,
                                            _scheduler ,
                                            _heightAndSpeed ) ;
        sut.Initialize ( DefaultCapacity ) ;
        // Note: not calling Start()

        // Act
        var act = ( ) => sut.StopWatchdog ( ) ;

        // Assert
        act.Should ( ).NotThrow ( "calling StopWatchdog without prior Start should be safe" ) ;

        sut.Dispose ( ) ;
    }
}
