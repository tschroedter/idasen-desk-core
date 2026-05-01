using System.Reactive.Concurrency ;
using System.Reactive.Subjects ;
using FluentAssertions ;
using Idasen.BluetoothLE.Linak.Control ;
using Idasen.BluetoothLE.Linak.Interfaces ;
using NSubstitute ;
using Serilog ;

namespace Idasen.BluetoothLE.Linak.Tests.Control ;

[ TestClass ]
public sealed class DeskMoverTests : IDisposable
{
    private readonly Subject < uint >                      _finishedSubject              = new( ) ;
    private readonly Subject < HeightSpeedDetails >        _heightAndSpeedChangedSubject = new( ) ;
    private          IStoppingHeightCalculator             _calculator                   = null! ;
    private          IDeskMoveEngine                       _engine                       = null! ;
    private          IDeskCommandExecutor                  _executor                     = null! ;
    private          IDeskMoveGuard                        _guard                        = null! ;
    private          IDeskHeightAndSpeed                   _heightAndSpeed               = null! ;
    private          DeskLocationHandlers                  _locationHandler              = null! ;
    private          ILogger                               _logger                       = null! ;
    private          IDeskMovementMonitorFactory           _monitorFactory               = null! ;
    private          DeskMovementHandlers                  _movementHandler              = null! ;
    private          IInitialHeightAndSpeedProviderFactory _providerFactory              = null! ;
    private          IScheduler                            _scheduler                    = null! ;
    private          ISubject < uint >                     _subjectFinished              = null! ;

    public void Dispose ( )
    {
        ( _subjectFinished as IDisposable )?.Dispose ( ) ;
        _finishedSubject.Dispose ( ) ;
        _heightAndSpeedChangedSubject.Dispose ( ) ;
    }

    [ TestInitialize ]
    public void Setup ( )
    {
        _logger          = Substitute.For < ILogger > ( ) ;
        _scheduler       = Scheduler.Immediate ;
        _providerFactory = Substitute.For < IInitialHeightAndSpeedProviderFactory > ( ) ;
        _monitorFactory  = Substitute.For < IDeskMovementMonitorFactory > ( ) ;
        _executor        = Substitute.For < IDeskCommandExecutor > ( ) ;
        _heightAndSpeed  = Substitute.For < IDeskHeightAndSpeed > ( ) ;
        _calculator      = Substitute.For < IStoppingHeightCalculator > ( ) ;
        _subjectFinished = _finishedSubject ;
        _engine          = Substitute.For < IDeskMoveEngine > ( ) ;
        _guard           = Substitute.For < IDeskMoveGuard > ( ) ;

        _locationHandler = new DeskLocationHandlers ( _heightAndSpeed ,
                                                      _providerFactory ) ;
        _movementHandler = new DeskMovementHandlers ( _monitorFactory ,
                                                      _executor ,
                                                      _calculator ,
                                                      _engine ,
                                                      _guard ) ;


        _heightAndSpeed.HeightAndSpeedChanged
                       .Returns ( _heightAndSpeedChangedSubject ) ;
    }

    private DeskMover CreateSut ( )
    {
        return new DeskMover ( _logger ,
                               _scheduler ,
                               _subjectFinished ,
                               _locationHandler ,
                               _movementHandler ) ;
    }

    [ TestCleanup ]
    public void Cleanup ( )
    {
        Dispose ( ) ;
    }

    [ TestMethod ]
    public void Initialize_SetsUpProvidersAndSubscriptions ( )
    {
        // Arrange
        var monitor         = Substitute.For < IDeskMovementMonitor > ( ) ;
        var initialProvider = Substitute.For < IInitialHeightProvider > ( ) ;
        _monitorFactory.Create ( _heightAndSpeed ).Returns ( monitor ) ;
        _providerFactory.Create ( _executor ,
                                  _heightAndSpeed ).Returns ( initialProvider ) ;
        initialProvider!.Finished.Returns ( _finishedSubject ) ;
        _heightAndSpeed.HeightAndSpeedChanged.Returns ( _heightAndSpeedChangedSubject ) ;
        _guard.TargetHeightReached.Returns ( _finishedSubject ) ;

        using var sut = CreateSut ( ) ;

        // Act
        sut.Initialize ( ) ;

        // Assert
        monitor.Received ( 1 ).Initialize ( ) ;
        initialProvider.Received ( 1 ).Initialize ( ) ;
    }

    [ TestMethod ]
    public void Start_CallsInitialProviderStart ( )
    {
        // Arrange
        var initialProvider = Substitute.For < IInitialHeightProvider > ( ) ;
        _providerFactory.Create ( _executor ,
                                  _heightAndSpeed ).Returns ( initialProvider ) ;
        initialProvider.Finished.Returns ( _finishedSubject ) ;
        _heightAndSpeed.HeightAndSpeedChanged.Returns ( _heightAndSpeedChangedSubject ) ;
        _guard.TargetHeightReached.Returns ( _finishedSubject ) ;

        using var sut = CreateSut ( ) ;
        sut.Initialize ( ) ;

        // Act
        sut.Start ( ) ;

        // Assert
        initialProvider.Received ( 1 ).Start ( ) ;
    }

    [ TestMethod ]
    public async Task Up_WhenAllowedToMove_CallsExecutorUp ( )
    {
        // Arrange
        using var sut = CreateSut ( ) ;

        sut.GetType ( ).GetProperty ( "IsAllowedToMove" )!.SetValue ( sut ,
                                                                      true ) ;
        _executor.Up ( ).Returns ( Task.FromResult ( true ) ) ;

        // Act
        var result = await sut.Up ( ) ;

        // Assert
        result.Should ( ).BeTrue ( ) ;
        await _executor.Received ( 1 ).Up ( ) ;
    }

    [ TestMethod ]
    public async Task Up_WhenNotAllowedToMove_ReturnsFalse ( )
    {
        // Arrange
        using var sut = CreateSut ( ) ;
        sut.GetType ( ).GetProperty ( "IsAllowedToMove" )!.SetValue ( sut ,
                                                                      false ) ;

        // Act
        var result = await sut.Up ( ) ;

        // Assert
        result.Should ( ).BeFalse ( ) ;
        await _executor.DidNotReceive ( ).Up ( ) ;
    }

    [ TestMethod ]
    public async Task Down_WhenAllowedToMove_CallsExecutorDown ( )
    {
        // Arrange
        using var sut = CreateSut ( ) ;
        sut.GetType ( ).GetProperty ( "IsAllowedToMove" )!.SetValue ( sut ,
                                                                      true ) ;
        _executor.Down ( ).Returns ( Task.FromResult ( true ) ) ;

        // Act
        var result = await sut.Down ( ) ;

        // Assert
        result.Should ( ).BeTrue ( ) ;
        await _executor.Received ( 1 ).Down ( ) ;
    }

    [ TestMethod ]
    public async Task Down_WhenNotAllowedToMove_ReturnsFalse ( )
    {
        // Arrange
        using var sut = CreateSut ( ) ;
        sut.GetType ( ).GetProperty ( "IsAllowedToMove" )!.SetValue ( sut ,
                                                                      false ) ;

        // Act
        var result = await sut.Down ( ) ;

        // Assert
        result.Should ( ).BeFalse ( ) ;
        await _executor.DidNotReceive ( ).Down ( ) ;
    }

    [ TestMethod ]
    public async Task StopMovement_StopsEngineAndEmitsFinished ( )
    {
        // Arrange
        using var sut = CreateSut ( ) ;
        sut.GetType ( ).GetProperty ( "IsAllowedToMove" )!.SetValue ( sut ,
                                                                      true ) ;
        _calculator.MoveIntoDirection.Returns ( Direction.Up ) ;

        // Act
        var result = await sut.StopMovement ( ) ;

        // Assert
        result.Should ( ).BeTrue ( ) ;
        await _engine.Received ( 1 ).StopMoveAsync ( ) ;
    }

    [ TestMethod ]
    public async Task StopMovement_CallsGuardStopGuarding ( )
    {
        // Arrange
        var monitor         = Substitute.For < IDeskMovementMonitor > ( ) ;
        var initialProvider = Substitute.For < IInitialHeightProvider > ( ) ;
        _monitorFactory.Create ( _heightAndSpeed ).Returns ( monitor ) ;
        _providerFactory.Create ( _executor ,
                                  _heightAndSpeed ).Returns ( initialProvider ) ;
        initialProvider.Finished.Returns ( _finishedSubject ) ;
        _heightAndSpeed.HeightAndSpeedChanged.Returns ( _heightAndSpeedChangedSubject ) ;
        using var sut = CreateSut ( ) ;
        sut.GetType ( ).GetProperty ( "IsAllowedToMove" )!.SetValue ( sut ,
                                                                      true ) ;
        _calculator.MoveIntoDirection.Returns ( Direction.Up ) ;

        sut.Start ( ) ;

        // Act
        await sut.StopMovement ( ) ;

        // Assert
        _guard.Received ( 1 ).StopGuarding ( ) ;
    }

    [ TestMethod ]
    public async Task StopMovement_WhenAlreadyStopped_DoesNotCallGuardOrEngineAgain ( )
    {
        // Arrange
        var monitor         = Substitute.For < IDeskMovementMonitor > ( ) ;
        var initialProvider = Substitute.For < IInitialHeightProvider > ( ) ;
        _monitorFactory.Create ( _heightAndSpeed ).Returns ( monitor ) ;
        _providerFactory.Create ( _executor ,
                                  _heightAndSpeed ).Returns ( initialProvider ) ;
        initialProvider.Finished.Returns ( _finishedSubject ) ;
        _heightAndSpeed.HeightAndSpeedChanged.Returns ( _heightAndSpeedChangedSubject ) ;

        using var sut = CreateSut ( ) ;
        sut.GetType ( ).GetProperty ( "IsAllowedToMove" )!.SetValue ( sut ,
                                                                      true ) ;
        _calculator.MoveIntoDirection.Returns ( Direction.Up ) ;

        // Act
        await sut.StopMovement ( ) ;
        await sut.StopMovement ( ) ;

        // Assert
        _guard.Received ( 1 ).StopGuarding ( ) ;
        await _engine.Received ( 1 ).StopMoveAsync ( ) ;
    }

    [ TestMethod ]
    public void Dispose_CleansUpResources ( )
    {
        // Arrange
        var monitor         = Substitute.For < IDeskMovementMonitor > ( ) ;
        var initialProvider = Substitute.For < IInitialHeightProvider > ( ) ;
        _monitorFactory.Create ( _heightAndSpeed ).Returns ( monitor ) ;
        _providerFactory.Create ( _executor ,
                                  _heightAndSpeed ).Returns ( initialProvider ) ;
        initialProvider.Finished.Returns ( _finishedSubject ) ;
        _heightAndSpeed.HeightAndSpeedChanged.Returns ( _heightAndSpeedChangedSubject ) ;
        _guard.TargetHeightReached.Returns ( _finishedSubject ) ;

        // Act
        var act = ( ) =>
                  {
                      var sut = CreateSut ( ) ;
                      sut.Initialize ( ) ;
                      sut.Dispose ( ) ;
                  } ;

        // Assert
        act.Should ( ).NotThrow ( ) ;
    }

    [ TestMethod ]
    public void Height_ForIsAllowedToMoveIsTrueAndSuccessAndNotified_UpdatesHeight ( )
    {
        // Arrange
        using var sut = CreateSut ( ) ;
        sut.Initialize ( ) ; // Ensure subscriptions are set up
        sut.GetType ( ).GetProperty ( "IsAllowedToMove" )!.SetValue ( sut ,
                                                                      true ) ;

        var expectedHeight = 123u ;
        var speed          = 0 ;
        _heightAndSpeed.Height.Returns ( expectedHeight ) ;

        // Act
        _heightAndSpeedChangedSubject.OnNext ( new HeightSpeedDetails ( DateTimeOffset.Now ,
                                                                        expectedHeight ,
                                                                        speed ) ) ;

        // Assert
        sut.Height.Should ( ).Be ( expectedHeight ,
                                   "the height should be updated when notified" ) ;
    }

    [ TestMethod ]
    public void Initialize_SubscribesToMonitorInactivityDetected ( )
    {
        // Arrange
        var monitor              = Substitute.For < IDeskMovementMonitor > ( ) ;
        using var inactivitySubject    = new Subject < string > ( ) ;
        var initialProvider      = Substitute.For < IInitialHeightProvider > ( ) ;

        _monitorFactory.Create ( _heightAndSpeed ).Returns ( monitor ) ;
        _providerFactory.Create ( _executor , _heightAndSpeed ).Returns ( initialProvider ) ;
        initialProvider!.Finished.Returns ( _finishedSubject ) ;
        _heightAndSpeed.HeightAndSpeedChanged.Returns ( _heightAndSpeedChangedSubject ) ;
        _guard.TargetHeightReached.Returns ( _finishedSubject ) ;
        monitor.InactivityDetected.Returns ( inactivitySubject ) ;

        using var sut = CreateSut ( ) ;

        // Act
        sut.Initialize ( ) ;

        // Assert - verify subscription was created
        _ = monitor.Received ( 1 ).InactivityDetected ;
    }

    [ TestMethod ]
    public void InactivityDetected_StopsMovementAndEngine ( )
    {
        // Arrange
        var monitor              = Substitute.For < IDeskMovementMonitor > ( ) ;
        using var inactivitySubject    = new Subject < string > ( ) ;
        var initialProvider      = Substitute.For < IInitialHeightProvider > ( ) ;

        _monitorFactory.Create ( _heightAndSpeed ).Returns ( monitor ) ;
        _providerFactory.Create ( _executor , _heightAndSpeed ).Returns ( initialProvider ) ;
        initialProvider!.Finished.Returns ( _finishedSubject ) ;
        _heightAndSpeed.HeightAndSpeedChanged.Returns ( _heightAndSpeedChangedSubject ) ;
        _guard.TargetHeightReached.Returns ( _finishedSubject ) ;
        monitor.InactivityDetected.Returns ( inactivitySubject ) ;

        using var sut = CreateSut ( ) ;
        sut.Initialize ( ) ;
        sut.GetType ( ).GetProperty ( "IsAllowedToMove" )!.SetValue ( sut , true ) ;

        // Act - emit inactivity event
        inactivitySubject.OnNext ( "No height updates received" ) ;

        // Assert
        _engine.Received ( ).StopMoveAsync ( ) ;
        sut.IsAllowedToMove.Should ( ).BeFalse ( "movement should be stopped after inactivity" ) ;
    }

    [ TestMethod ]
    public void InactivityDetected_LogsError ( )
    {
        // Arrange
        var monitor              = Substitute.For < IDeskMovementMonitor > ( ) ;
        using var inactivitySubject    = new Subject < string > ( ) ;
        var initialProvider      = Substitute.For < IInitialHeightProvider > ( ) ;
        var reason               = "No height updates received for timeout period" ;

        _monitorFactory.Create ( _heightAndSpeed ).Returns ( monitor ) ;
        _providerFactory.Create ( _executor , _heightAndSpeed ).Returns ( initialProvider ) ;
        initialProvider!.Finished.Returns ( _finishedSubject ) ;
        _heightAndSpeed.HeightAndSpeedChanged.Returns ( _heightAndSpeedChangedSubject ) ;
        _guard.TargetHeightReached.Returns ( _finishedSubject ) ;
        monitor.InactivityDetected.Returns ( inactivitySubject ) ;

        using var sut = CreateSut ( ) ;
        sut.Initialize ( ) ;

        // Act - emit inactivity event
        inactivitySubject.OnNext ( reason ) ;

        // Assert
        _logger.Received ( 1 ).Error ( "Movement stopped due to inactivity: {Reason}" , reason ) ;
    }

    [ TestMethod ]
    public void StartAfterReceivingCurrentHeight_SetsCalculatorHeightAndSpeed ( )
    {
        // Arrange
        var monitor         = Substitute.For < IDeskMovementMonitor > ( ) ;
        using var inactivitySubject = new Subject < string > ( ) ;
        var initialProvider = Substitute.For < IInitialHeightProvider > ( ) ;
        _monitorFactory.Create ( _heightAndSpeed ).Returns ( monitor ) ;
        _providerFactory.Create ( _executor , _heightAndSpeed ).Returns ( initialProvider ) ;
        initialProvider!.Finished.Returns ( _finishedSubject ) ;
        _heightAndSpeed.HeightAndSpeedChanged.Returns ( _heightAndSpeedChangedSubject ) ;
        _guard.TargetHeightReached.Returns ( _finishedSubject ) ;
        monitor.InactivityDetected.Returns ( inactivitySubject ) ;
        _calculator.MoveIntoDirection.Returns ( Direction.Up ) ;

        using var sut = CreateSut ( ) ;
        sut.Initialize ( ) ;
        sut.TargetHeight = 1000 ;

        var testHeight = 500u ;
        var testSpeed  = 42 ;

        // Act - simulate initial height callback with specific height and speed
        var method = sut.GetType ( )
                        .GetMethod ( "StartAfterReceivingCurrentHeight" ,
                                    System.Reflection.BindingFlags.NonPublic |
                                    System.Reflection.BindingFlags.Instance ) ;
        method?.Invoke ( sut , [ testHeight , testSpeed ] ) ;

        // Assert - verify calculator received the height and speed parameters
        _calculator.Received ( 1 ).Height = testHeight ;
        _calculator.Received ( 1 ).Speed = testSpeed ;
    }

    [ TestMethod ]
    public void StartAfterReceivingCurrentHeight_CallsMonitorStart ( )
    {
        // Arrange
        var monitor         = Substitute.For < IDeskMovementMonitor > ( ) ;
        using var inactivitySubject = new Subject < string > ( ) ;
        var initialProvider = Substitute.For < IInitialHeightProvider > ( ) ;
        _monitorFactory.Create ( _heightAndSpeed ).Returns ( monitor ) ;
        _providerFactory.Create ( _executor , _heightAndSpeed ).Returns ( initialProvider ) ;
        initialProvider!.Finished.Returns ( _finishedSubject ) ;
        _heightAndSpeed.HeightAndSpeedChanged.Returns ( _heightAndSpeedChangedSubject ) ;
        _guard.TargetHeightReached.Returns ( _finishedSubject ) ;
        monitor.InactivityDetected.Returns ( inactivitySubject ) ;
        _calculator.MoveIntoDirection.Returns ( Direction.Up ) ;

        using var sut = CreateSut ( ) ;
        sut.Initialize ( ) ;
        sut.TargetHeight = 1000 ; // Set a non-zero target

        // Act - simulate initial height callback
        var method = sut.GetType ( )
                        .GetMethod ( "StartAfterReceivingCurrentHeight" ,
                                    System.Reflection.BindingFlags.NonPublic |
                                    System.Reflection.BindingFlags.Instance ) ;
        method?.Invoke ( sut , [ 500u , 0 ] ) ;

        // Assert
        monitor.Received ( 1 ).Start ( ) ;
    }

    [ TestMethod ]
    public void StartAfterReceivingCurrentHeight_StartsMonitorBeforeStartingEngine ( )
    {
        // Arrange
        var monitor         = Substitute.For < IDeskMovementMonitor > ( ) ;
        using var inactivitySubject = new Subject < string > ( ) ;
        var initialProvider = Substitute.For < IInitialHeightProvider > ( ) ;
        var callOrder       = new List < string > ( ) ;

        _monitorFactory.Create ( _heightAndSpeed ).Returns ( monitor ) ;
        _providerFactory.Create ( _executor , _heightAndSpeed ).Returns ( initialProvider ) ;
        initialProvider!.Finished.Returns ( _finishedSubject ) ;
        _heightAndSpeed.HeightAndSpeedChanged.Returns ( _heightAndSpeedChangedSubject ) ;
        _guard.TargetHeightReached.Returns ( _finishedSubject ) ;
        monitor.InactivityDetected.Returns ( inactivitySubject ) ;
        _calculator.MoveIntoDirection.Returns ( Direction.Up ) ;

        monitor.When ( m => m.Start ( ) )
               .Do ( _ => callOrder.Add ( "MonitorStart" ) ) ;
        _engine.When ( e => e.StartMoveAsync ( Arg.Any < Direction > ( ) ,
                                              Arg.Any < CancellationToken > ( ) ) )
               .Do ( _ => callOrder.Add ( "EngineStart" ) ) ;

        using var sut = CreateSut ( ) ;
        sut.Initialize ( ) ;
        sut.TargetHeight = 1000 ;

        // Act
        var method = sut.GetType ( )
                        .GetMethod ( "StartAfterReceivingCurrentHeight" ,
                                    System.Reflection.BindingFlags.NonPublic |
                                    System.Reflection.BindingFlags.Instance ) ;
        method?.Invoke ( sut , [ 500u , 0 ] ) ;

        // Assert
        callOrder.Should ( ).HaveCount ( 2 ) ;
        callOrder [ 0 ].Should ( ).Be ( "MonitorStart" ,
                                      "monitor should start before engine" ) ;
        callOrder [ 1 ].Should ( ).Be ( "EngineStart" ,
                                      "engine should start after monitor" ) ;
    }

    [ TestMethod ]
    public void StartAfterReceivingCurrentHeight_WhenTargetHeightIsZero_DoesNotStartMonitor ( )
    {
        // Arrange
        var monitor         = Substitute.For < IDeskMovementMonitor > ( ) ;
        using var inactivitySubject = new Subject < string > ( ) ;
        var initialProvider = Substitute.For < IInitialHeightProvider > ( ) ;
        _monitorFactory.Create ( _heightAndSpeed ).Returns ( monitor ) ;
        _providerFactory.Create ( _executor , _heightAndSpeed ).Returns ( initialProvider ) ;
        initialProvider!.Finished.Returns ( _finishedSubject ) ;
        _heightAndSpeed.HeightAndSpeedChanged.Returns ( _heightAndSpeedChangedSubject ) ;
        _guard.TargetHeightReached.Returns ( _finishedSubject ) ;
        monitor.InactivityDetected.Returns ( inactivitySubject ) ;
        _calculator.MoveIntoDirection.Returns ( Direction.Up ) ;

        using var sut = CreateSut ( ) ;
        sut.Initialize ( ) ;
        sut.TargetHeight = 0 ; // Zero target should prevent movement

        // Act - simulate initial height callback with zero target
        var method = sut.GetType ( )
                        .GetMethod ( "StartAfterReceivingCurrentHeight" ,
                                    System.Reflection.BindingFlags.NonPublic |
                                    System.Reflection.BindingFlags.Instance ) ;
        method?.Invoke ( sut , [ 500u , 0 ] ) ;

        // Assert - monitor.Start should not be called when TargetHeight is 0
        monitor.DidNotReceive ( ).Start ( ) ;
    }

    [ TestMethod ]
    public void StartAfterReceivingCurrentHeight_CallsMonitorStartOnEachMovementCycle ( )
    {
        // Arrange
        var monitor         = Substitute.For < IDeskMovementMonitor > ( ) ;
        using var inactivitySubject = new Subject < string > ( ) ;
        var initialProvider = Substitute.For < IInitialHeightProvider > ( ) ;
        _monitorFactory.Create ( _heightAndSpeed ).Returns ( monitor ) ;
        _providerFactory.Create ( _executor , _heightAndSpeed ).Returns ( initialProvider ) ;
        initialProvider!.Finished.Returns ( _finishedSubject ) ;
        _heightAndSpeed.HeightAndSpeedChanged.Returns ( _heightAndSpeedChangedSubject ) ;
        _guard.TargetHeightReached.Returns ( _finishedSubject ) ;
        monitor.InactivityDetected.Returns ( inactivitySubject ) ;
        _calculator.MoveIntoDirection.Returns ( Direction.Up ) ;

        using var sut = CreateSut ( ) ;
        sut.Initialize ( ) ;
        sut.TargetHeight = 1000 ;

        var method = sut.GetType ( )
                        .GetMethod ( "StartAfterReceivingCurrentHeight" ,
                                    System.Reflection.BindingFlags.NonPublic |
                                    System.Reflection.BindingFlags.Instance ) ;

        // Act - simulate multiple movement cycles with different heights and speeds
        method?.Invoke ( sut , [ 500u , 10 ] ) ;
        method?.Invoke ( sut , [ 700u , 15 ] ) ;
        method?.Invoke ( sut , [ 900u , 20 ] ) ;

        // Assert
        monitor.Received ( 3 ).Start ( ) ;
    }

    [ TestMethod ]
    public void StartAfterReceivingCurrentHeight_SetsCalculatorTargetHeight ( )
    {
        // Arrange
        var monitor         = Substitute.For < IDeskMovementMonitor > ( ) ;
        using var inactivitySubject = new Subject < string > ( ) ;
        var initialProvider = Substitute.For < IInitialHeightProvider > ( ) ;
        _monitorFactory.Create ( _heightAndSpeed ).Returns ( monitor ) ;
        _providerFactory.Create ( _executor , _heightAndSpeed ).Returns ( initialProvider ) ;
        initialProvider!.Finished.Returns ( _finishedSubject ) ;
        _heightAndSpeed.HeightAndSpeedChanged.Returns ( _heightAndSpeedChangedSubject ) ;
        _guard.TargetHeightReached.Returns ( _finishedSubject ) ;
        monitor.InactivityDetected.Returns ( inactivitySubject ) ;
        _calculator.MoveIntoDirection.Returns ( Direction.Up ) ;

        using var sut = CreateSut ( ) ;
        sut.Initialize ( ) ;
        var expectedTargetHeight = 1000u ;
        sut.TargetHeight = expectedTargetHeight ;

        // Act
        var method = sut.GetType ( )
                        .GetMethod ( "StartAfterReceivingCurrentHeight" ,
                                    System.Reflection.BindingFlags.NonPublic |
                                    System.Reflection.BindingFlags.Instance ) ;
        method?.Invoke ( sut , [ 500u , 0 ] ) ;

        // Assert
        _calculator.Received ( 1 ).TargetHeight = expectedTargetHeight ;
    }

    [ TestMethod ]
    public void StartAfterReceivingCurrentHeight_CallsGuardStartGuarding ( )
    {
        // Arrange
        var monitor         = Substitute.For < IDeskMovementMonitor > ( ) ;
        using var inactivitySubject = new Subject < string > ( ) ;
        var initialProvider = Substitute.For < IInitialHeightProvider > ( ) ;
        _monitorFactory.Create ( _heightAndSpeed ).Returns ( monitor ) ;
        _providerFactory.Create ( _executor , _heightAndSpeed ).Returns ( initialProvider ) ;
        initialProvider!.Finished.Returns ( _finishedSubject ) ;
        _heightAndSpeed.HeightAndSpeedChanged.Returns ( _heightAndSpeedChangedSubject ) ;
        _guard.TargetHeightReached.Returns ( _finishedSubject ) ;
        monitor.InactivityDetected.Returns ( inactivitySubject ) ;
        _calculator.MoveIntoDirection.Returns ( Direction.Up ) ;

        using var sut = CreateSut ( ) ;
        sut.Initialize ( ) ;
        var targetHeight = 1000u ;
        sut.TargetHeight = targetHeight ;

        // Act
        var method = sut.GetType ( )
                        .GetMethod ( "StartAfterReceivingCurrentHeight" ,
                                    System.Reflection.BindingFlags.NonPublic |
                                    System.Reflection.BindingFlags.Instance ) ;
        method?.Invoke ( sut , [ 500u , 0 ] ) ;

        // Assert
        _guard.Received ( 1 ).StartGuarding ( Direction.Up , targetHeight , Arg.Any < CancellationToken > ( ) ) ;
    }

    [ TestMethod ]
    public void StartAfterReceivingCurrentHeight_CallsEngineStartMoveAsync ( )
    {
        // Arrange
        var monitor         = Substitute.For < IDeskMovementMonitor > ( ) ;
        using var inactivitySubject = new Subject < string > ( ) ;
        var initialProvider = Substitute.For < IInitialHeightProvider > ( ) ;
        _monitorFactory.Create ( _heightAndSpeed ).Returns ( monitor ) ;
        _providerFactory.Create ( _executor , _heightAndSpeed ).Returns ( initialProvider ) ;
        initialProvider!.Finished.Returns ( _finishedSubject ) ;
        _heightAndSpeed.HeightAndSpeedChanged.Returns ( _heightAndSpeedChangedSubject ) ;
        _guard.TargetHeightReached.Returns ( _finishedSubject ) ;
        monitor.InactivityDetected.Returns ( inactivitySubject ) ;
        _calculator.MoveIntoDirection.Returns ( Direction.Up ) ;

        using var sut = CreateSut ( ) ;
        sut.Initialize ( ) ;
        sut.TargetHeight = 1000 ;

        // Act
        var method = sut.GetType ( )
                        .GetMethod ( "StartAfterReceivingCurrentHeight" ,
                                    System.Reflection.BindingFlags.NonPublic |
                                    System.Reflection.BindingFlags.Instance ) ;
        method?.Invoke ( sut , [ 500u , 0 ] ) ;

        // Assert
        _engine.Received ( 1 ).StartMoveAsync ( Direction.Up , Arg.Any < CancellationToken > ( ) ) ;
    }

    [ TestMethod ]
    public void StartAfterReceivingCurrentHeight_WhenDirectionIsNone_DoesNotCallEngineStartMoveAsync ( )
    {
        // Arrange
        var monitor         = Substitute.For < IDeskMovementMonitor > ( ) ;
        using var inactivitySubject = new Subject < string > ( ) ;
        var initialProvider = Substitute.For < IInitialHeightProvider > ( ) ;
        _monitorFactory.Create ( _heightAndSpeed ).Returns ( monitor ) ;
        _providerFactory.Create ( _executor , _heightAndSpeed ).Returns ( initialProvider ) ;
        initialProvider!.Finished.Returns ( _finishedSubject ) ;
        _heightAndSpeed.HeightAndSpeedChanged.Returns ( _heightAndSpeedChangedSubject ) ;
        _guard.TargetHeightReached.Returns ( _finishedSubject ) ;
        monitor.InactivityDetected.Returns ( inactivitySubject ) ;
        _calculator.MoveIntoDirection.Returns ( Direction.None ) ;

        using var sut = CreateSut ( ) ;
        sut.Initialize ( ) ;
        sut.TargetHeight = 1000 ;

        // Act
        var method = sut.GetType ( )
                        .GetMethod ( "StartAfterReceivingCurrentHeight" ,
                                    System.Reflection.BindingFlags.NonPublic |
                                    System.Reflection.BindingFlags.Instance ) ;
        method?.Invoke ( sut , [ 1000u , 0 ] ) ; // Already at target

        // Assert
        _engine.DidNotReceive ( ).StartMoveAsync ( Arg.Any < Direction > ( ) , Arg.Any < CancellationToken > ( ) ) ;
    }

    [ TestMethod ]
    public void StartAfterReceivingCurrentHeight_WhenDirectionIsNone_DoesNotCallMonitorStart ( )
    {
        // Arrange
        var monitor         = Substitute.For < IDeskMovementMonitor > ( ) ;
        using var inactivitySubject = new Subject < string > ( ) ;
        var initialProvider = Substitute.For < IInitialHeightProvider > ( ) ;
        _monitorFactory.Create ( _heightAndSpeed ).Returns ( monitor ) ;
        _providerFactory.Create ( _executor , _heightAndSpeed ).Returns ( initialProvider ) ;
        initialProvider!.Finished.Returns ( _finishedSubject ) ;
        _heightAndSpeed.HeightAndSpeedChanged.Returns ( _heightAndSpeedChangedSubject ) ;
        _guard.TargetHeightReached.Returns ( _finishedSubject ) ;
        monitor.InactivityDetected.Returns ( inactivitySubject ) ;
        _calculator.MoveIntoDirection.Returns ( Direction.None ) ;

        using var sut = CreateSut ( ) ;
        sut.Initialize ( ) ;
        sut.TargetHeight = 1000 ;

        // Act
        var method = sut.GetType ( )
                        .GetMethod ( "StartAfterReceivingCurrentHeight" ,
                                    System.Reflection.BindingFlags.NonPublic |
                                    System.Reflection.BindingFlags.Instance ) ;
        method?.Invoke ( sut , [ 1000u , 0 ] ) ; // Already at target

        // Assert
        monitor.DidNotReceive ( ).Start ( ) ;
    }

    [ TestMethod ]
    public async Task StopMovement_CallsMonitorStopWatchdog ( )
    {
        // Arrange
        var monitor         = Substitute.For < IDeskMovementMonitor > ( ) ;
        using var inactivitySubject = new Subject < string > ( ) ;
        var initialProvider = Substitute.For < IInitialHeightProvider > ( ) ;
        _monitorFactory.Create ( _heightAndSpeed ).Returns ( monitor ) ;
        _providerFactory.Create ( _executor , _heightAndSpeed ).Returns ( initialProvider ) ;
        initialProvider!.Finished.Returns ( _finishedSubject ) ;
        _heightAndSpeed.HeightAndSpeedChanged.Returns ( _heightAndSpeedChangedSubject ) ;
        _guard.TargetHeightReached.Returns ( _finishedSubject ) ;
        monitor.InactivityDetected.Returns ( inactivitySubject ) ;

        using var sut = CreateSut ( ) ;
        sut.Initialize ( ) ;
        sut.GetType ( ).GetProperty ( "IsAllowedToMove" )!.SetValue ( sut , true ) ;

        // Act
        await sut.StopMovement ( ) ;

        // Assert
        monitor.Received ( 1 ).StopWatchdog ( ) ;
    }

    [ TestMethod ]
    public async Task StopMovement_StopsWatchdogBeforeEmittingFinished ( )
    {
        // Arrange
        var monitor         = Substitute.For < IDeskMovementMonitor > ( ) ;
        using var inactivitySubject = new Subject < string > ( ) ;
        var initialProvider = Substitute.For < IInitialHeightProvider > ( ) ;
        var callOrder       = new List < string > ( ) ;

        _monitorFactory.Create ( _heightAndSpeed ).Returns ( monitor ) ;
        _providerFactory.Create ( _executor , _heightAndSpeed ).Returns ( initialProvider ) ;
        initialProvider!.Finished.Returns ( _finishedSubject ) ;
        _heightAndSpeed.HeightAndSpeedChanged.Returns ( _heightAndSpeedChangedSubject ) ;
        _guard.TargetHeightReached.Returns ( _finishedSubject ) ;
        monitor.InactivityDetected.Returns ( inactivitySubject ) ;

        monitor.When ( m => m.StopWatchdog ( ) )
               .Do ( _ => callOrder.Add ( "MonitorStop" ) ) ;
        _subjectFinished.Subscribe ( _ => callOrder.Add ( "FinishedEmitted" ) ) ;

        using var sut = CreateSut ( ) ;
        sut.Initialize ( ) ;
        sut.GetType ( ).GetProperty ( "IsAllowedToMove" )!.SetValue ( sut , true ) ;

        // Act
        await sut.StopMovement ( ) ;

        // Assert
        callOrder.Should ( ).HaveCount ( 2 ) ;
        callOrder [ 0 ].Should ( ).Be ( "MonitorStop" ,
                                      "monitor should stop before finished event" ) ;
        callOrder [ 1 ].Should ( ).Be ( "FinishedEmitted" ,
                                      "finished event should emit after monitor stops" ) ;
    }

    [ TestMethod ]
    public async Task StopMovement_WhenAlreadyStopped_DoesNotCallMonitorStopWatchdog ( )
    {
        // Arrange
        var monitor         = Substitute.For < IDeskMovementMonitor > ( ) ;
        using var inactivitySubject = new Subject < string > ( ) ;
        var initialProvider = Substitute.For < IInitialHeightProvider > ( ) ;
        _monitorFactory.Create ( _heightAndSpeed ).Returns ( monitor ) ;
        _providerFactory.Create ( _executor , _heightAndSpeed ).Returns ( initialProvider ) ;
        initialProvider!.Finished.Returns ( _finishedSubject ) ;
        _heightAndSpeed.HeightAndSpeedChanged.Returns ( _heightAndSpeedChangedSubject ) ;
        _guard.TargetHeightReached.Returns ( _finishedSubject ) ;
        monitor.InactivityDetected.Returns ( inactivitySubject ) ;

        using var sut = CreateSut ( ) ;
        sut.Initialize ( ) ;
        sut.GetType ( ).GetProperty ( "IsAllowedToMove" )!.SetValue ( sut , true ) ;

        // Act - stop twice
        await sut.StopMovement ( ) ;
        monitor.ClearReceivedCalls ( ) ; // Clear the first call
        await sut.StopMovement ( ) ;

        // Assert - should not call StopWatchdog again when already stopped
        monitor.DidNotReceive ( ).StopWatchdog ( ) ;
    }

    [ TestMethod ]
    public void InactivityDetected_CallsMonitorStopWatchdog ( )
    {
        // Arrange
        var monitor         = Substitute.For < IDeskMovementMonitor > ( ) ;
        using var inactivitySubject = new Subject < string > ( ) ;
        var initialProvider = Substitute.For < IInitialHeightProvider > ( ) ;

        _monitorFactory.Create ( _heightAndSpeed ).Returns ( monitor ) ;
        _providerFactory.Create ( _executor , _heightAndSpeed ).Returns ( initialProvider ) ;
        initialProvider!.Finished.Returns ( _finishedSubject ) ;
        _heightAndSpeed.HeightAndSpeedChanged.Returns ( _heightAndSpeedChangedSubject ) ;
        _guard.TargetHeightReached.Returns ( _finishedSubject ) ;
        monitor.InactivityDetected.Returns ( inactivitySubject ) ;

        using var sut = CreateSut ( ) ;
        sut.Initialize ( ) ;
        sut.GetType ( ).GetProperty ( "IsAllowedToMove" )!.SetValue ( sut , true ) ;

        // Act - emit inactivity event
        inactivitySubject.OnNext ( "No height updates received" ) ;

        // Assert
        monitor.Received ( 1 ).StopWatchdog ( ) ;
    }

    [ TestMethod ]
    public void TargetHeightReached_CallsMonitorStopWatchdog ( )
    {
        // Arrange
        var monitor         = Substitute.For < IDeskMovementMonitor > ( ) ;
        using var inactivitySubject = new Subject < string > ( ) ;
        using var targetHeightSubject = new Subject < uint > ( ) ;
        var initialProvider = Substitute.For < IInitialHeightProvider > ( ) ;

        _monitorFactory.Create ( _heightAndSpeed ).Returns ( monitor ) ;
        _providerFactory.Create ( _executor , _heightAndSpeed ).Returns ( initialProvider ) ;
        initialProvider!.Finished.Returns ( _finishedSubject ) ;
        _heightAndSpeed.HeightAndSpeedChanged.Returns ( _heightAndSpeedChangedSubject ) ;
        _guard.TargetHeightReached.Returns ( targetHeightSubject ) ;
        monitor.InactivityDetected.Returns ( inactivitySubject ) ;

        using var sut = CreateSut ( ) ;
        sut.Initialize ( ) ;
        sut.GetType ( ).GetProperty ( "IsAllowedToMove" )!.SetValue ( sut , true ) ;

        // Act - emit target height reached event
        targetHeightSubject.OnNext ( 1000u ) ;

        // Assert
        monitor.Received ( 1 ).StopWatchdog ( ) ;
    }

    [ TestMethod ]
    public void TargetHeightReached_CallsStopWatchdogBeforeEngineStop ( )
    {
        // Arrange
        var monitor         = Substitute.For < IDeskMovementMonitor > ( ) ;
        using var inactivitySubject = new Subject < string > ( ) ;
        using var targetHeightSubject = new Subject < uint > ( ) ;
        var initialProvider = Substitute.For < IInitialHeightProvider > ( ) ;
        var callOrder       = new List < string > ( ) ;

        _monitorFactory.Create ( _heightAndSpeed ).Returns ( monitor ) ;
        _providerFactory.Create ( _executor , _heightAndSpeed ).Returns ( initialProvider ) ;
        initialProvider!.Finished.Returns ( _finishedSubject ) ;
        _heightAndSpeed.HeightAndSpeedChanged.Returns ( _heightAndSpeedChangedSubject ) ;
        _guard.TargetHeightReached.Returns ( targetHeightSubject ) ;
        monitor.InactivityDetected.Returns ( inactivitySubject ) ;

        monitor.When ( m => m.StopWatchdog ( ) )
               .Do ( _ => callOrder.Add ( "MonitorStop" ) ) ;
        _engine.When ( e => e.StopMoveAsync ( ) )
               .Do ( _ => callOrder.Add ( "EngineStop" ) ) ;
        _logger.When ( l => l.Information ( "Reached target height={TargetHeight}" , Arg.Any < uint > ( ) ) )
               .Do ( _ => callOrder.Add ( "LogInfo" ) ) ;

        using var sut = CreateSut ( ) ;
        sut.Initialize ( ) ;
        sut.GetType ( ).GetProperty ( "IsAllowedToMove" )!.SetValue ( sut , true ) ;

        // Act
        targetHeightSubject.OnNext ( 1000u ) ;

        // Assert - monitor should stop before engine to prevent race condition
        callOrder.Should ( ).HaveCount ( 3 ) ;
        callOrder [ 0 ].Should ( ).Be ( "LogInfo" ,
                                      "logging should happen first" ) ;
        callOrder [ 1 ].Should ( ).Be ( "MonitorStop" ,
                                      "monitor should stop before engine" ) ;
        callOrder [ 2 ].Should ( ).Be ( "EngineStop" ,
                                      "engine should stop after monitor" ) ;
    }

    [ TestMethod ]
    public void InactivityDetected_CallsStopWatchdogBeforeEngineStop ( )
    {
        // Arrange
        var monitor         = Substitute.For < IDeskMovementMonitor > ( ) ;
        using var inactivitySubject = new Subject < string > ( ) ;
        var initialProvider = Substitute.For < IInitialHeightProvider > ( ) ;
        var callOrder       = new List < string > ( ) ;
        var reason          = "No height updates received" ;

        _monitorFactory.Create ( _heightAndSpeed ).Returns ( monitor ) ;
        _providerFactory.Create ( _executor , _heightAndSpeed ).Returns ( initialProvider ) ;
        initialProvider!.Finished.Returns ( _finishedSubject ) ;
        _heightAndSpeed.HeightAndSpeedChanged.Returns ( _heightAndSpeedChangedSubject ) ;
        _guard.TargetHeightReached.Returns ( _finishedSubject ) ;
        monitor.InactivityDetected.Returns ( inactivitySubject ) ;

        monitor.When ( m => m.StopWatchdog ( ) )
               .Do ( _ => callOrder.Add ( "MonitorStop" ) ) ;
        _engine.When ( e => e.StopMoveAsync ( ) )
               .Do ( _ => callOrder.Add ( "EngineStop" ) ) ;
        _logger.When ( l => l.Error ( "Movement stopped due to inactivity: {Reason}" , Arg.Any < string > ( ) ) )
               .Do ( _ => callOrder.Add ( "LogError" ) ) ;

        using var sut = CreateSut ( ) ;
        sut.Initialize ( ) ;
        sut.GetType ( ).GetProperty ( "IsAllowedToMove" )!.SetValue ( sut , true ) ;

        // Act
        inactivitySubject.OnNext ( reason ) ;

        // Assert - monitor should stop before engine to prevent race condition
        callOrder.Should ( ).HaveCount ( 3 ) ;
        callOrder [ 0 ].Should ( ).Be ( "LogError" ,
                                      "error logging should happen first" ) ;
        callOrder [ 1 ].Should ( ).Be ( "MonitorStop" ,
                                      "monitor should stop before engine" ) ;
        callOrder [ 2 ].Should ( ).Be ( "EngineStop" ,
                                      "engine should stop after monitor" ) ;
    }
}
