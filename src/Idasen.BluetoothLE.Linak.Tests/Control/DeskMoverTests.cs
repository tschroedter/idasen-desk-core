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
    private          IDeskMoveEngine                     _engine                       = null! ;
    private          IDeskCommandExecutor                  _executor                     = null! ;
    private          IDeskMoveGuard                        _guard                        = null! ;
    private          IDeskHeightAndSpeed                   _heightAndSpeed               = null! ;
    private          ILogger                               _logger                       = null! ;
    private          IDeskMovementMonitorFactory           _monitorFactory               = null! ;
    private          DeskMover                             _mover                        = null! ;
    private          IInitialHeightAndSpeedProviderFactory _providerFactory              = null! ;
    private          IScheduler                            _scheduler                    = null! ;
    private          ISubject < uint >                     _subjectFinished              = null! ;

    public void Dispose ( )
    {
        _mover.Dispose ( ) ;

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

        _mover = new DeskMover ( _logger ,
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

        // Act
        _mover.Initialize ( ) ;

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
        _mover.Initialize ( ) ;

        // Act
        _mover.Start ( ) ;

        // Assert
        initialProvider.Received ( 1 ).Start ( ) ;
    }

    [ TestMethod ]
    public async Task Up_WhenAllowedToMove_CallsExecutorUp ( )
    {
        // Arrange
        _mover.GetType ( ).GetProperty ( "IsAllowedToMove" )!.SetValue ( _mover ,
                                                                         true ) ;
        _executor.Up ( ).Returns ( Task.FromResult ( true ) ) ;

        // Act
        var result = await _mover.Up ( ) ;

        // Assert
        result.Should ( ).BeTrue ( ) ;
        await _executor.Received ( 1 ).Up ( ) ;
    }

    [ TestMethod ]
    public async Task Up_WhenNotAllowedToMove_ReturnsFalse ( )
    {
        // Arrange
        _mover.GetType ( ).GetProperty ( "IsAllowedToMove" )!.SetValue ( _mover ,
                                                                         false ) ;

        // Act
        var result = await _mover.Up ( ) ;

        // Assert
        result.Should ( ).BeFalse ( ) ;
        await _executor.DidNotReceive ( ).Up ( ) ;
    }

    [ TestMethod ]
    public async Task Down_WhenAllowedToMove_CallsExecutorDown ( )
    {
        // Arrange
        _mover.GetType ( ).GetProperty ( "IsAllowedToMove" )!.SetValue ( _mover ,
                                                                         true ) ;
        _executor.Down ( ).Returns ( Task.FromResult ( true ) ) ;

        // Act
        var result = await _mover.Down ( ) ;

        // Assert
        result.Should ( ).BeTrue ( ) ;
        await _executor.Received ( 1 ).Down ( ) ;
    }

    [ TestMethod ]
    public async Task Down_WhenNotAllowedToMove_ReturnsFalse ( )
    {
        // Arrange
        _mover.GetType ( ).GetProperty ( "IsAllowedToMove" )!.SetValue ( _mover ,
                                                                         false ) ;

        // Act
        var result = await _mover.Down ( ) ;

        // Assert
        result.Should ( ).BeFalse ( ) ;
        await _executor.DidNotReceive ( ).Down ( ) ;
    }

    [ TestMethod ]
    public async Task StopMovement_StopsEngineAndEmitsFinished ( )
    {
        // Arrange
        _mover.GetType ( ).GetProperty ( "IsAllowedToMove" )!.SetValue ( _mover ,
                                                                         true ) ;
        _calculator.MoveIntoDirection.Returns ( Direction.Up ) ;

        // Act
        var result = await _mover.StopMovement ( ) ;

        // Assert
        result.Should ( ).BeTrue ( ) ;
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
        _mover.Initialize ( ) ;

        // Act
        var act = ( ) => _mover.Dispose ( ) ;

        // Assert
        act.Should ( ).NotThrow ( ) ;
    }
}
