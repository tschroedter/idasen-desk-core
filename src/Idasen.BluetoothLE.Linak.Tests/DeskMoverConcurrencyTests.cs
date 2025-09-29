using System.Reactive.Subjects ;
using FluentAssertions ;
using Idasen.BluetoothLE.Linak.Control ;
using Idasen.BluetoothLE.Linak.Interfaces ;
using Microsoft.Reactive.Testing ;
using NSubstitute ;
using Serilog ;

namespace Idasen.BluetoothLE.Linak.Tests ;

[ TestClass ]
public class DeskMoverConcurrencyTests : IDisposable
{
    private IStoppingHeightCalculator             _calculator         = null! ;
    private IDeskCommandExecutor                  _executor           = null! ;
    private Subject < uint >                      _finishedSubject    = null! ;
    private IDeskHeightAndSpeed                   _heightAndSpeed     = null! ;
    private IDeskHeightMonitor                    _heightMonitor      = null! ;
    private Subject < HeightSpeedDetails >        _heightSpeedSubject = null! ;
    private IInitialHeightProvider                _initialProvider    = null! ;
    private ILogger                               _logger             = null! ;
    private IDeskMovementMonitorFactory           _monitorFactory     = null! ;
    private IDeskMovementMonitor                  _movementMonitor    = null! ;
    private IInitialHeightAndSpeedProviderFactory _providerFactory    = null! ;
    private TestScheduler                         _scheduler          = null! ;

    public void Dispose ( )
    {
        _finishedSubject?.Dispose ( ) ;
        _heightSpeedSubject?.Dispose ( ) ;

        GC.SuppressFinalize ( this ) ;
    }

    [ TestInitialize ]
    public void Setup ( )
    {
        _logger    = Substitute.For < ILogger > ( ) ;
        _scheduler = new TestScheduler ( ) ;

        _initialProvider = Substitute.For < IInitialHeightProvider > ( ) ;
        _providerFactory = Substitute.For < IInitialHeightAndSpeedProviderFactory > ( ) ;
        _providerFactory.Create ( Arg.Any < IDeskCommandExecutor > ( ) ,
                                  Arg.Any < IDeskHeightAndSpeed > ( ) )
                        .Returns ( _initialProvider ) ;

        _movementMonitor = Substitute.For < IDeskMovementMonitor > ( ) ;
        _monitorFactory  = Substitute.For < IDeskMovementMonitorFactory > ( ) ;
        _monitorFactory.Create ( Arg.Any < IDeskHeightAndSpeed > ( ) ).Returns ( _movementMonitor ) ;

        _executor = Substitute.For < IDeskCommandExecutor > ( ) ;
        _executor.StopMovement ( ).Returns ( Task.FromResult ( true ) ) ;

        _heightAndSpeed = Substitute.For < IDeskHeightAndSpeed > ( ) ;
        _heightAndSpeed.Height.Returns ( 1000u ) ;
        _heightAndSpeed.Speed.Returns ( 10 ) ;
        _heightSpeedSubject = new Subject < HeightSpeedDetails > ( ) ;
        _heightAndSpeed.HeightAndSpeedChanged.Returns ( _heightSpeedSubject ) ;

        _calculator = Substitute.For < IStoppingHeightCalculator > ( ) ;
        _calculator.Calculate ( ).Returns ( _calculator ) ;
        _calculator.HasReachedTargetHeight.Returns ( false ) ;
        _calculator.MoveIntoDirection.Returns ( Direction.Up ) ;

        _heightMonitor = Substitute.For < IDeskHeightMonitor > ( ) ;
        _heightMonitor.IsHeightChanging ( ).Returns ( true ) ;

        _finishedSubject = new Subject < uint > ( ) ;
        _initialProvider.Finished.Returns ( _finishedSubject ) ;
    }

    private DeskMover CreateSut ( IObservable < uint > ? finishedStream = null )
    {
        return new DeskMover ( _logger ,
                               _scheduler ,
                               _providerFactory ,
                               _monitorFactory ,
                               _executor ,
                               _heightAndSpeed ,
                               _calculator ,
#pragma warning disable CA2000
                               finishedStream as ISubject < uint > ?? new Subject < uint > ( ) ,
#pragma warning restore CA2000
                               _heightMonitor ) ;
    }

    [ TestMethod ]
    public async Task TimerOverlap_DoesNotInvokeUpConcurrently ( )
    {
        using var sut = CreateSut ( ) ;

        sut.TargetHeight = 2000u ;

        // Arrange Up to block until we release it
        var tcs = new TaskCompletionSource < bool > ( ) ;
        _executor.Up ( ).Returns ( _ => tcs.Task ) ;

        sut.Initialize ( ) ;
        sut.Start ( ) ;

        // Signal initial height has been received -> schedules StartAfterReceivingCurrentHeight via ObserveOn
        _finishedSubject.OnNext ( 1000u ) ;
        _scheduler.AdvanceBy ( 1 ) ; // process OnFinished

        // Simulate first timer tick -> Move starts and calls Up (blocked)
        var t1 = sut.OnTimerElapsed ( 0 ) ;

        // Give a moment for the call to be issued
        await Task.Yield ( ) ;
        await _executor.Received ( 1 ).Up ( ) ;

        // Simulate second overlapping timer tick -> should be skipped by semaphore/pending task
        var t2 = sut.OnTimerElapsed ( 1 ) ;
        await t2 ; // should complete quickly without invoking Up again

        await _executor.Received ( 1 ).Up ( ) ;

        // Release the blocked Up and finish
        tcs.TrySetResult ( true ) ;
        await t1 ;
    }

    [ TestMethod ]
    public async Task Stop_InvokedOnceAndFinishedEmittedOnce_WhenTargetReached ( )
    {
        var       finishedEvents   = new List < uint > ( ) ;
        using var externalFinished = new Subject < uint > ( ) ;

        using var sut = CreateSut ( externalFinished ) ;
        externalFinished.Subscribe ( h => finishedEvents.Add ( h ) ) ;

        sut.TargetHeight = 2000u ;

        // Make calculator signal target reached immediately
        _calculator.HasReachedTargetHeight.Returns ( true ) ;
        _calculator.MoveIntoDirection.Returns ( Direction.None ) ;

        sut.Initialize ( ) ;
        sut.Start ( ) ;

        _finishedSubject.OnNext ( 1000u ) ;
        _scheduler.AdvanceBy ( 1 ) ; // process OnFinished

        // Simulate timer tick which will call StopListening due to target reached
        await sut.OnTimerElapsed ( 0 ) ;

        await _executor.Received ( 1 ).StopMovement ( ) ;
        finishedEvents.Count.Should ( ).Be ( 1 ) ;
    }

    [ TestMethod ]
    public async Task Stop_CalledTwice_SecondCallIsNoopAndNoDuplicateFinished ( )
    {
        var       finishedEvents   = new List < uint > ( ) ;
        using var externalFinished = new Subject < uint > ( ) ; // CA2000 fix: ensure disposal

        using var sut = CreateSut ( externalFinished ) ;
        externalFinished.Subscribe ( h => finishedEvents.Add ( h ) ) ;

        sut.TargetHeight = 2000u ;
        _calculator.HasReachedTargetHeight.Returns ( true ) ;
        _calculator.MoveIntoDirection.Returns ( Direction.None ) ;

        sut.Initialize ( ) ;
        sut.Start ( ) ;
        _finishedSubject.OnNext ( 1000u ) ;
        _scheduler.AdvanceBy ( 1 ) ;

        // First explicit stop
        await sut.StopMovement ( ) ;
        // Second stop should be a noop
        await sut.StopMovement ( ) ;

        await _executor.Received ( 1 ).StopMovement ( ) ;
        finishedEvents.Count.Should ( ).Be ( 1 ) ;
    }

    [ TestMethod ]
    public void Sampling_TakesLastItemPerInterval ( )
    {
        using var sut = CreateSut ( ) ;
        sut.TargetHeight = 1500u ;
        _calculator.MoveIntoDirection.Returns ( Direction.None ) ;

        sut.Initialize ( ) ;
        sut.Start ( ) ;
        _finishedSubject.OnNext ( 1000u ) ;
        _scheduler.AdvanceBy ( 1 ) ;

        var d1 = new HeightSpeedDetails ( DateTimeOffset.Now ,
                                          1100u ,
                                          50 ) ;

        var d2 = new HeightSpeedDetails ( DateTimeOffset.Now.AddMilliseconds ( 10 ) ,
                                          1200u ,
                                          60 ) ;

        // Emit two samples within one interval
        _heightSpeedSubject.OnNext ( d1 ) ;
        _heightSpeedSubject.OnNext ( d2 ) ;

        // Advance past one sampling window
        _scheduler.AdvanceBy ( TimeSpan.FromMilliseconds ( 200 ).Ticks ) ;

        sut.Height.Should ( ).Be ( d2.Height ) ;
        sut.Speed.Should ( ).Be ( d2.Speed ) ;
    }

    [ TestMethod ]
    public async Task MoveCommand_Failure_AllowsRetry ( )
    {
        var sut = CreateSut ( ) ;
        sut.TargetHeight = 2000u ;
        _calculator.MoveIntoDirection.Returns ( Direction.Up ) ;

        // First Up fails, second succeeds
        _executor.Up ( ).Returns ( Task.FromResult ( false ) ,
                                   Task.FromResult ( true ) ) ;

        sut.Initialize ( ) ;
        sut.Start ( ) ;
        _finishedSubject.OnNext ( 1000u ) ;
        _scheduler.AdvanceBy ( 1 ) ;

        await sut.OnTimerElapsed ( 0 ) ;
        await _executor.Received ( 1 ).Up ( ) ;

        // Allow continuation to reset direction after failure
        await Task.Yield ( ) ;

        await sut.OnTimerElapsed ( 1 ) ;
        await _executor.Received ( 2 ).Up ( ) ;

        sut.Dispose ( ) ;
    }

    [ TestMethod ]
    public async Task Stop_Coalesces_MultipleRequests_WhilePending ( )
    {
        var sut = CreateSut ( ) ;
        sut.TargetHeight = 1500u ;
        _calculator.MoveIntoDirection.Returns ( Direction.None ) ;

        var tcs = new TaskCompletionSource < bool > ( ) ;
        _executor.StopMovement ( ).Returns ( _ => tcs.Task ) ;

        sut.Initialize ( ) ;
        sut.Start ( ) ;
        _finishedSubject.OnNext ( 1000u ) ;
        _scheduler.AdvanceBy ( 1 ) ;

        // First evaluation triggers StopListening (pending)
        await sut.OnTimerElapsed ( 0 ) ;
        await _executor.Received ( 1 ).StopMovement ( ) ;

        // While StopListening is still pending, further evaluations should not send another StopListening
        await sut.OnTimerElapsed ( 1 ) ;
        await _executor.Received ( 1 ).StopMovement ( ) ;

        tcs.TrySetResult ( true ) ;
        sut.Dispose ( ) ;
    }
}