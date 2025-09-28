using System.Reactive.Linq ;
using System.Reactive.Subjects ;
using FluentAssertions ;
using FluentAssertions.Execution ;
using Idasen.BluetoothLE.Linak.Control ;
using Idasen.BluetoothLE.Linak.Interfaces ;
using Microsoft.Reactive.Testing ;
using NSubstitute ;
using Serilog ;

namespace Idasen.BluetoothLE.Linak.Tests ;

[ TestClass ]
public class DeskMoverTests : IDisposable
{
    private const uint InitialHeight       = 100u ;
    private const uint DefaultTargetHeight = 1500 ;
    private const uint DefaultHeight       = 1000 ;
    private const int  DefaultSpeed        = 200 ;

    private IStoppingHeightCalculator _calculator         = null! ;
    private HeightSpeedDetails        _details1           = null! ;
    private IDisposable               _disposable         = null! ;
    private IInitialHeightProvider    _disposableProvider = null! ;
    private IDeskCommandExecutor      _executor           = null! ;
    private IObservable < uint >      _finished           = null! ;
    private IDeskHeightAndSpeed       _heightAndSpeed     = null! ;
    private IDeskHeightMonitor        _heightMonitor      = null! ;

    private ILogger                               _logger                = null! ;
    private IDeskMovementMonitor                  _monitor               = null! ;
    private IDeskMovementMonitorFactory           _monitorFactory        = null! ;
    private IInitialHeightProvider                _provider              = null! ;
    private IInitialHeightAndSpeedProviderFactory _providerFactory       = null! ;
    private TestScheduler                         _scheduler             = null! ;
    private Subject < uint >                      _subjectFinished       = null! ;
    private Subject < HeightSpeedDetails >        _subjectHeightAndSpeed = null! ;

    public void Dispose ( )
    {
        _subjectFinished.OnCompleted ( ) ;
        _subjectHeightAndSpeed.OnCompleted ( ) ;

        _subjectFinished.Dispose ( ) ;
        _subjectHeightAndSpeed.Dispose ( ) ;
        GC.SuppressFinalize ( this ) ;
    }

    [ TestInitialize ]
    public void Initialize ( )
    {
        _logger          = Substitute.For < ILogger > ( ) ;
        _scheduler       = new TestScheduler ( ) ;
        _providerFactory = Substitute.For < IInitialHeightAndSpeedProviderFactory > ( ) ;
        _monitorFactory  = Substitute.For < IDeskMovementMonitorFactory > ( ) ;
        _executor        = Substitute.For < IDeskCommandExecutor > ( ) ;
        // Ensure Stop() has a valid default return to avoid awaiting null
        _executor.Stop ( ).Returns ( true ) ;
        _heightAndSpeed        = Substitute.For < IDeskHeightAndSpeed > ( ) ;
        _calculator            = Substitute.For < IStoppingHeightCalculator > ( ) ;
        _subjectHeightAndSpeed = new Subject < HeightSpeedDetails > ( ) ;
        _subjectFinished       = new Subject < uint > ( ) ;
        _provider              = Substitute.For < IInitialHeightProvider > ( ) ;
        _heightMonitor         = Substitute.For < IDeskHeightMonitor > ( ) ;

        _providerFactory.Create (
                                 Arg.Any < IDeskCommandExecutor > ( ) ,
                                 Arg.Any < IDeskHeightAndSpeed > ( ) )
                        .Returns ( _provider ) ;

        _provider.Finished
                 .Returns ( _subjectFinished ) ;

        _heightAndSpeed.HeightAndSpeedChanged
                       .Returns ( _subjectHeightAndSpeed ) ;
        _heightAndSpeed.Height
                       .Returns ( DefaultHeight ) ;
        _heightAndSpeed.Speed
                       .Returns ( DefaultSpeed ) ;

        _details1 = new HeightSpeedDetails (
                                            DateTimeOffset.Now ,
                                            123u ,
                                            321 ) ;

        _heightMonitor.IsHeightChanging ( )
                      .Returns ( true ) ;

        _monitor = Substitute.For < IDeskMovementMonitor > ( ) ;

        _monitorFactory.Create ( _heightAndSpeed )
                       .Returns ( _monitor ) ;

        _finished = Substitute.For < ISubject < uint > > ( ) ;

        // initialise the subscription disposable used in assertions
        _disposable = Substitute.For < IDisposable > ( ) ;
        _finished.Subscribe ( null! )
                 .ReturnsForAnyArgs ( _disposable ) ;

        _disposableProvider = Substitute.For < IInitialHeightProvider > ( ) ;
        _disposableProvider.Finished
                           .Returns ( _finished ) ;
    }

    private DeskMover CreateSut ( )
    {
        return new DeskMover (
                              _logger ,
                              _scheduler ,
                              _providerFactory ,
                              _monitorFactory ,
                              _executor ,
                              _heightAndSpeed ,
                              _calculator ,
                              _subjectFinished ,
                              _heightMonitor ) ;
    }

    private DeskMover CreateSutInitialized ( )
    {
        var sut = CreateSut ( ) ;

        sut.Initialize ( ) ;

        return sut ;
    }

    private DeskMover CreateSutWithTargetHeight ( )
    {
        var sut = CreateSutInitialized ( ) ;

        sut.TargetHeight = DefaultTargetHeight ;

        return sut ;
    }

    private DeskMover CreateSutWithIsAllowedToMoveIsTrue ( )
    {
        var sut = CreateSutWithTargetHeight ( ) ;

        _subjectFinished.OnNext ( InitialHeight ) ;

        // Process ObserveOn scheduled work without running indefinitely
        _scheduler.AdvanceBy ( TimeSpan.FromMilliseconds ( 1 ).Ticks ) ;

        return sut ;
    }

    private DeskMover CreateSutWithIsAllowedToMoveIsTrueAndHeightChanged ( )
    {
        var sut = CreateSutWithIsAllowedToMoveIsTrue ( ) ;

        _heightAndSpeed.Height.Returns ( _details1.Height ) ;
        _heightAndSpeed.Speed.Returns ( _details1.Speed ) ;
        _subjectHeightAndSpeed.OnNext ( _details1 ) ;

        // Advance virtual time to allow the sampling window to emit
        _scheduler.AdvanceBy ( TimeSpan.FromMilliseconds ( 200 ).Ticks ) ;

        return sut ;
    }

    [ TestMethod ]
    public void StartAfterRefreshed_ForInvoked_SetsHeight ( )
    {
        CreateSutWithIsAllowedToMoveIsTrue ( ).Height
                                              .Should ( )
                                              .Be ( DefaultHeight ) ;
    }

    [ TestMethod ]
    public void StartAfterRefreshed_ForInvoked_SetsSpeed ( )
    {
        CreateSutWithIsAllowedToMoveIsTrue ( ).Speed
                                              .Should ( )
                                              .Be ( DefaultSpeed ) ;
    }

    [ TestMethod ]
    public void StartAfterRefreshed_ForInvoked_CallsCalculator ( )
    {
        using var _ = CreateSutWithIsAllowedToMoveIsTrue ( ) ;

        using var scope = new AssertionScope ( ) ;

        _calculator.Height
                   .Should ( )
                   .Be (
                        DefaultHeight ,
                        "Height" ) ;
        _calculator.Speed
                   .Should ( )
                   .Be (
                        DefaultSpeed ,
                        "Speed" ) ;

        _calculator.StartMovingIntoDirection
                   .Should ( )
                   .Be (
                        Direction.None ,
                        "StartMovingIntoDirection" ) ;

        _calculator.TargetHeight
                   .Should ( )
                   .Be (
                        DefaultTargetHeight ,
                        "TargetHeight" ) ;

        _calculator.Received ( )
                   .Calculate ( ) ;
    }

    [ TestMethod ]
    public void StartAfterRefreshed_ForInvoked_SetsIsAllowedToMoveToTrue ( )
    {
        CreateSutWithIsAllowedToMoveIsTrue ( ).IsAllowedToMove
                                              .Should ( )
                                              .BeTrue ( ) ;
    }

    [ TestMethod ]
    public void StartAfterRefreshed_ForIsAllowedToMoveIsTrueAndSuccess_SetsStartMovingIntoDirection ( )
    {
        CreateSutWithIsAllowedToMoveIsTrue ( ).StartMovingIntoDirection
                                              .Should ( )
                                              .Be ( _calculator.MoveIntoDirection ) ;
    }

    [ TestMethod ]
    public void Start_Invoked_CallsHeightMonitorResets ( )
    {
        _executor.Up ( )
                 .Returns ( true ) ;

        using var sut = CreateSutWithIsAllowedToMoveIsTrue ( ) ;

        _heightMonitor.Received ( )
                      .Reset ( ) ;
    }

    [ TestMethod ]
    public async Task Up_ForIsAllowedToMoveIsTrueAndSuccess_ReturnsTrue ( )
    {
        _executor.Up ( )
                 .Returns ( true ) ;

        using var sut = CreateSutWithIsAllowedToMoveIsTrue ( ) ;

        var actual = await sut.Up ( ) ;

        actual.Should ( )
              .BeTrue ( ) ;
    }

    [ TestMethod ]
    public async Task Up_ForIsAllowedToMoveIsTrueAndFailed_ReturnsFalse ( )
    {
        _executor.Up ( )
                 .Returns ( false ) ;

        using var sut = CreateSutWithIsAllowedToMoveIsTrue ( ) ;

        var actual = await sut.Up ( ) ;

        actual.Should ( )
              .BeFalse ( ) ;
    }

    [ TestMethod ]
    public void Height_ForIsAllowedToMoveIsTrueAndSuccessAndNotified_UpdatesHeight ( )
    {
        using var sut = CreateSutWithIsAllowedToMoveIsTrueAndHeightChanged ( ) ;

        sut.Height
           .Should ( )
           .Be ( _details1.Height ) ;
    }

    [ TestMethod ]
    public void Speed_ForIsAllowedToMoveIsTrueAndSuccessAndNotified_UpdatesSpeed ( )
    {
        using var sut = CreateSutWithIsAllowedToMoveIsTrueAndHeightChanged ( ) ;

        sut.Speed
           .Should ( )
           .Be ( _details1.Speed ) ;
    }

    [ TestMethod ]
    public async Task OnTimerElapsed_ForTimerIsNull_DoNotMovesUp ( )
    {
        _calculator.MoveIntoDirection
                   .Returns ( Direction.Up ) ;

        using var sut = CreateSutInitialized ( ) ;

        await sut.Stop ( ) ; // make sure timer is null

        await sut.OnTimerElapsed ( 1 ) ;

        await _executor.DidNotReceive ( )
                       .Up ( ) ;
    }

    [ TestMethod ]
    public async Task OnTimerElapsed_ForMoveIntoDirectionDown_MovesDown ( )
    {
        _calculator.MoveIntoDirection
                   .Returns ( Direction.Down ) ;

        using var sut = CreateSutWithIsAllowedToMoveIsTrue ( ) ;

        await sut.OnTimerElapsed ( 1 ) ;

        await _executor.Received ( )
                       .Down ( ) ;
    }

    [ TestMethod ]
    public async Task OnTimerElapsed_ForMoveIntoDirectionNone_MoveStop ( )
    {
        _calculator.MoveIntoDirection
                   .Returns ( Direction.None ) ;

        using var sut = CreateSutWithIsAllowedToMoveIsTrue ( ) ;

        await sut.OnTimerElapsed ( 1 ) ;

        using var scope = new AssertionScope ( ) ;

        await _executor.Received ( )
                       .Stop ( ) ;
    }

    [ TestMethod ]
    public async Task OnTimerElapsed_ForIsAllowedToMoveIsFalse_DoesNotMove ( )
    {
        _calculator.MoveIntoDirection
                   .Returns ( Direction.None ) ;

        using var sut = CreateSutInitialized ( ) ;

        await sut.OnTimerElapsed ( 1 ) ;

        using var scope = new AssertionScope ( ) ;

        await _executor.DidNotReceive ( )
                       .Up ( ) ;
        await _executor.DidNotReceive ( )
                       .Down ( ) ;
        await _executor.DidNotReceive ( )
                       .Stop ( ) ;
    }

    [ TestMethod ]
    public async Task Finished_ForMoveFinished_Notifies ( )
    {
        using var sut = CreateSutWithIsAllowedToMoveIsTrue ( ) ;

        var wasNotified = false ;

        sut.Finished.ObserveOn ( _scheduler )
           .Subscribe ( _ => wasNotified = true ) ;

        await sut.Stop ( ) ;

        // Process the ObserveOn scheduled Finished notification
        _scheduler.AdvanceBy ( TimeSpan.FromMilliseconds ( 1 ).Ticks ) ;

        wasNotified.Should ( )
                   .BeTrue ( ) ;
    }

    [ TestMethod ]
    public async Task Up_ForIsAllowedToMoveIsTrue_DoesNotMoveUp ( )
    {
        using var sut = CreateSutInitialized ( ) ;

        await sut.Up ( ) ;

        _ = _executor.DidNotReceive ( )
                     .Up ( ) ;
    }

    [ TestMethod ]
    public async Task Up_ForIsAllowedToMoveIsTrue_ReturnsFalse ( )
    {
        using var sut = CreateSutInitialized ( ) ;

        var actual = await sut.Up ( ) ;

        actual.Should ( )
              .BeFalse ( ) ;
    }

    [ TestMethod ]
    public async Task Down_ForIsAllowedToMoveIsTrue_DoesNotMoveDown ( )
    {
        using var sut = CreateSutInitialized ( ) ;

        await sut.Down ( ) ;

        _ = _executor.DidNotReceive ( )
                     .Down ( ) ;
    }

    [ TestMethod ]
    public async Task Down_ForIsAllowedToMoveIsTrue_ReturnsFalse ( )
    {
        using var sut = CreateSutInitialized ( ) ;

        var actual = await sut.Down ( ) ;

        actual.Should ( )
              .BeFalse ( ) ;
    }

    [ TestMethod ]
    public void Start_ForInvoked_CallsProvider ( )
    {
        using var sut = CreateSutInitialized ( ) ;

        sut.Start ( ) ;

        _provider.Received ( )
                 .Start ( ) ;
    }

    [ TestMethod ]
    public void Dispose_ForInvoked_DisposesMonitor ( )
    {
        var sut = CreateSutInitialized ( ) ;

        sut.Dispose ( ) ;

        _monitor.Received ( )
                .Dispose ( ) ;
    }

    [ TestMethod ]
    public void Dispose_ForInvoked_DisposesDisposableProvider ( )
    {
        _providerFactory.Create (
                                 _executor ,
                                 _heightAndSpeed )
                        .Returns ( _disposableProvider ) ;

        var sut = CreateSutInitialized ( ) ;

        sut.Dispose ( ) ;

        _disposable.Received ( )
                   .Dispose ( ) ;
    }

    [ TestMethod ]
    public void Dispose_ForInvoked_DisposalHeightAndSpeed ( )
    {
        var disposable = Substitute.For < IDisposable > ( ) ;

        var subject = Substitute.For < ISubject < HeightSpeedDetails > > ( ) ;

        subject.Subscribe ( Arg.Any < IObserver < HeightSpeedDetails > > ( ) )
               .Returns ( disposable ) ;

        _heightAndSpeed.HeightAndSpeedChanged
                       .Returns ( subject ) ;

        var sut = CreateSutWithIsAllowedToMoveIsTrue ( ) ;

        sut.Dispose ( ) ;

        disposable.Received ( )
                  .Dispose ( ) ;
    }

    [ TestMethod ]
    public async Task OnTimerElapsed_PredictiveStop_PreventsOvershoot_WhenMovingUp ( )
    {
        // Arrange a scenario where adding MovementUntilStop to current height would cross the target
        _heightAndSpeed.Height.Returns ( 1000u ) ;
        _heightAndSpeed.Speed.Returns ( 100 ) ;

        _calculator.Calculate ( ).Returns ( _calculator ) ;
        _calculator.MoveIntoDirection.Returns ( Direction.Up ) ;
        _calculator.MovementUntilStop.Returns ( 120 ) ; // enough to cross from 1000 to >= 1120
        _calculator.HasReachedTargetHeight.Returns ( false ) ;

        var sut = CreateSut ( ) ;
        sut.TargetHeight = 1080u ; // below predicted stopping height

        sut.Initialize ( ) ;
        sut.Start ( ) ;

        _subjectFinished.OnNext ( 1000u ) ;
        _scheduler.AdvanceBy ( 1 ) ;

        // Act
        await sut.OnTimerElapsed ( 1 ) ;

        // Assert: Stop was issued due to predictive crossing, and no Up/Down command after that
        await _executor.Received ( 1 ).Stop ( ) ;
        await _executor.DidNotReceive ( ).Up ( ) ;
        await _executor.DidNotReceive ( ).Down ( ) ;
    }

    [ TestMethod ]
    public async Task OnTimerElapsed_PredictiveStop_PreventsOvershoot_WhenMovingDown ( )
    {
        // Arrange a scenario where subtracting MovementUntilStop from current height would cross the target downward
        _heightAndSpeed.Height.Returns ( 1200u ) ;
        _heightAndSpeed.Speed.Returns ( - 100 ) ;

        _calculator.Calculate ( ).Returns ( _calculator ) ;
        _calculator.MoveIntoDirection.Returns ( Direction.Down ) ;
        _calculator.MovementUntilStop.Returns ( - 150 ) ; // magnitude 150
        _calculator.HasReachedTargetHeight.Returns ( false ) ;

        var sut = CreateSut ( ) ;
        sut.TargetHeight = 1100u ; // predicted stop 1050 <= target

        sut.Initialize ( ) ;
        sut.Start ( ) ;

        _subjectFinished.OnNext ( 1200u ) ;
        _scheduler.AdvanceBy ( 1 ) ;

        // Act
        await sut.OnTimerElapsed ( 1 ) ;

        // Assert: Stop was issued due to predictive crossing, and no Up/Down command after that
        await _executor.Received ( 1 ).Stop ( ) ;
        await _executor.DidNotReceive ( ).Up ( ) ;
        await _executor.DidNotReceive ( ).Down ( ) ;
    }
}
