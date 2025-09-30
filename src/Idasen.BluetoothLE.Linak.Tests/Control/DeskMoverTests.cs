using System.Reactive.Concurrency ;
using System.Reactive.Subjects ;
using FluentAssertions ;
using Idasen.BluetoothLE.Linak.Control ;
using Idasen.BluetoothLE.Linak.Interfaces ;
using NSubstitute ;
using Serilog ;

namespace Idasen.BluetoothLE.Linak.Tests.Control ;

[ TestClass ]
public class DeskMoverTests : IDisposable
{
    private readonly Subject < uint >                      _finishedSubject              = new( ) ;
    private readonly Subject < HeightSpeedDetails >        _heightAndSpeedChangedSubject = new( ) ;
    private          IStoppingHeightCalculator             _calculator                   = null! ;
    private          IDeskMoveEngine                       _engine                       = null! ;
    private          IDeskCommandExecutor                  _executor                     = null! ;
    private          IDeskMoveGuard                        _guard                        = null! ;
    private          IDeskHeightAndSpeed                   _heightAndSpeed               = null! ;
    private          ILogger                               _logger                       = null! ;
    private          IDeskMovementMonitorFactory           _monitorFactory               = null! ;
    private          IInitialHeightAndSpeedProviderFactory _providerFactory              = null! ;
    private          IScheduler                            _scheduler                    = null! ;
    private          ISubject < uint >                     _subjectFinished              = null! ;

    public void Dispose ( )
    {
        ( _subjectFinished as IDisposable )?.Dispose ( ) ;
        _finishedSubject.Dispose ( ) ;
        _heightAndSpeedChangedSubject.Dispose ( ) ;

        GC.SuppressFinalize ( this ) ;
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
    }

    private DeskMover CreateSut ( )
    {
        return new DeskMover ( _logger ,
                               _scheduler ,
                               _providerFactory ,
                               _monitorFactory ,
                               _executor ,
                               _heightAndSpeed ,
                               _calculator ,
                               _subjectFinished ,
                               _engine ,
                               _guard ) ;
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
}
