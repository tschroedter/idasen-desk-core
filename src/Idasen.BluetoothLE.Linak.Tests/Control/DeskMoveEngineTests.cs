using FluentAssertions ;
using Idasen.BluetoothLE.Linak.Control ;
using Idasen.BluetoothLE.Linak.Interfaces ;
using NSubstitute ;
using Serilog ;

namespace Idasen.BluetoothLE.Linak.Tests.Control ;

[ TestClass ]
public class DeskMoveEngineTests
{
    private IDeskCommandExecutor _executor = null! ;
    private ILogger              _logger   = null! ;

    private DeskMoveEngine CreateSut ( )
    {
        return new DeskMoveEngine ( _logger ,
                                    _executor ) ;
    }

    [ TestInitialize ]
    public void Init ( )
    {
        _logger   = Substitute.For < ILogger > ( ) ;
        _executor = Substitute.For < IDeskCommandExecutor > ( ) ;
        _executor.Up ( ).Returns ( Task.FromResult ( true ) ) ;
        _executor.Down ( ).Returns ( Task.FromResult ( true ) ) ;
        _executor.StopMovement ( ).Returns ( Task.FromResult ( true ) ) ;
    }

    [ TestMethod ]
    public async Task Constructor_NullLogger_Throws ( )
    {
        DeskMoveEngine sut ;

        var act = async ( ) =>
                  {
                      sut = new DeskMoveEngine ( null! ,
                                                 _executor ) ;
                      _ = sut.ToString ( ) ;
                      await Task.Yield ( ) ;
                  } ;

        await act.Should ( ).ThrowAsync < ArgumentNullException > ( ) ;
    }

    [ TestMethod ]
    public async Task Constructor_NullExecutor_Throws ( )
    {
        DeskMoveEngine sut ;

        var act = async ( ) =>
                  {
                      sut = new DeskMoveEngine ( _logger ,
                                                 null! ) ;
                      _ = sut.ToString ( ) ;
                      await Task.Yield ( ) ;
                  } ;
        await act.Should ( ).ThrowAsync < ArgumentNullException > ( ) ;
    }

    [ TestMethod ]
    public async Task DelayInterval_Default_Is100ms ( )
    {
        var sut = CreateSut ( ) ;

        await Task.Yield ( ) ;

        sut.DelayInterval.Should ( ).Be ( TimeSpan.FromMilliseconds ( 100 ) ) ;
    }

    [ TestMethod ]
    public async Task DelayInterval_CanBeSet ( )
    {
        var sut = CreateSut ( ) ;

        sut.DelayInterval = TimeSpan.FromSeconds ( 1 ) ;

        await Task.Yield ( ) ;

        sut.DelayInterval.Should ( ).Be ( TimeSpan.FromSeconds ( 1 ) ) ;
    }

    [ TestMethod ]
    public async Task StartMove_None_DoesNothing ( )
    {
        var sut = CreateSut ( ) ;

        using var cts = new CancellationTokenSource ( 50 ) ;

        await sut.StartMoveAsync ( Direction.None ,
                                   cts.Token ) ;

        await _executor.DidNotReceive ( ).Up ( ) ;
        await _executor.DidNotReceive ( ).Down ( ) ;
    }

    [ TestMethod ]
    public async Task StartMove_Up_IssuesRepeatedUpCommands ( )
    {
        var sut = CreateSut ( ) ;

        sut.DelayInterval = TimeSpan.FromMilliseconds ( 10 ) ;

        using var cts = new CancellationTokenSource ( 50 ) ;

        await sut.StartMoveAsync ( Direction.Up ,
                                   cts.Token ) ;

        await _executor.Received ( ).Up ( ) ; // At least once
    }

    [ TestMethod ]
    public async Task StartMove_Down_IssuesRepeatedDownCommands ( )
    {
        var sut = CreateSut ( ) ;

        sut.DelayInterval = TimeSpan.FromMilliseconds ( 10 ) ;

        using var cts = new CancellationTokenSource ( 50 ) ;

        await sut.StartMoveAsync ( Direction.Down ,
                                   cts.Token ) ;

        await _executor.Received ( ).Down ( ) ; // At least once
    }

    [ TestMethod ]
    public async Task StartMove_SameDirection_DoesNotReissue ( )
    {
        var sut = CreateSut ( ) ;

        sut.DelayInterval = TimeSpan.FromMilliseconds ( 10 ) ;

        using var cts = new CancellationTokenSource ( 30 ) ;

        await sut.StartMoveAsync ( Direction.Up ,
                                   cts.Token ) ;

        // Try to start again in the same direction
        await sut.StartMoveAsync ( Direction.Up ,
                                   cts.Token ) ;

        await _executor.Received ( ).Up ( ) ;
    }

    [ TestMethod ]
    public async Task StartMove_Cancels_ResetsDirection ( )
    {
        var sut = CreateSut ( ) ;

        sut.DelayInterval = TimeSpan.FromMilliseconds ( 10 ) ;

        using var cts = new CancellationTokenSource ( 30 ) ;

        await sut.StartMoveAsync ( Direction.Up ,
                                   cts.Token ) ;

        // After cancellation, direction should be None
        await sut.StopMoveAsync ( ) ;

        // Assert that the direction is reset to None
        sut.CurrentDirection.Should ( ).Be ( Direction.None ) ;
    }

    [ TestMethod ]
    public async Task StopMove_ResetsDirection ( )
    {
        var sut = CreateSut ( ) ;

        await sut.StopMoveAsync ( ) ; // Should not throw

        // Assert that the direction is reset to None
        sut.CurrentDirection.Should ( ).Be ( Direction.None ) ;
    }

    [ TestMethod ]
    public async Task StopMoveAsync_CallsExecutorStopMovement ( )
    {
        var sut = CreateSut ( ) ;

        await sut.StopMoveAsync ( ) ;

        // Verify that StopMovement is called exactly once
        await _executor.Received ( 1 ).StopMovement ( ) ;

        // Verify that the direction is reset to None
        sut.CurrentDirection.Should ( ).Be ( Direction.None ) ;
    }

    [ TestMethod ]
    public async Task StartMoveAsync_CommandFails_LogsDebugMessage ( )
    {
        var sut = CreateSut ( ) ;

        // Simulate a failure in the Up command
        _executor.Up ( ).Returns ( Task.FromResult ( false ) ) ;

        sut.DelayInterval = TimeSpan.FromMilliseconds ( 10 ) ;

        using var cts = new CancellationTokenSource ( 50 ) ;

        await sut.StartMoveAsync ( Direction.Up ,
                                   cts.Token ) ;

        // Verify that the debug message is logged when the command fails
        _logger.Received ( ).Debug ( "StartMoveAsync command failed: {Desired} (consecutive failures: {Count})" ,
                                     Direction.Up ,
                                     Arg.Any < int > ( ) ) ;
    }

    [ TestMethod ]
    public async Task StartMoveAsync_ConsecutiveFailures_StopsAfterThreeFailures ( )
    {
        var sut = CreateSut ( ) ;

        // Simulate failures in the Up command
        _executor.Up ( ).Returns ( Task.FromResult ( false ) ) ;

        sut.DelayInterval = TimeSpan.FromMilliseconds ( 10 ) ;

        using var cts = new CancellationTokenSource ( 500 ) ;

        await sut.StartMoveAsync ( Direction.Up ,
                                   cts.Token ) ;

        // Verify that Up was called exactly 3 times before stopping
        await _executor.Received ( 3 ).Up ( ) ;

        // Verify that a warning was logged
        _logger.Received ( ).Warning ( "Stopping move due to {Count} consecutive failures" ,
                                       3 ) ;

        // Verify that the direction is reset to None
        sut.CurrentDirection.Should ( ).Be ( Direction.None ) ;
    }

    [ TestMethod ]
    public async Task StartMoveAsync_FailureThenSuccess_ResetsFailureCounter ( )
    {
        var sut = CreateSut ( ) ;

        var callCount = 0 ;

        // First call fails, second succeeds, then continue succeeding
        _executor.Up ( ).Returns ( _ =>
        {
            callCount++ ;
            return Task.FromResult ( callCount > 1 ) ; // False on first call, true afterward
        } ) ;

        sut.DelayInterval = TimeSpan.FromMilliseconds ( 10 ) ;

        using var cts = new CancellationTokenSource ( 100 ) ;

        await sut.StartMoveAsync ( Direction.Up ,
                                   cts.Token ) ;

        // Verify that Up was called multiple times (more than 3, proving counter was reset)
        await _executor.Received ( ).Up ( ) ;

        // Verify that no warning about consecutive failures was logged
        _logger.DidNotReceive ( ).Warning ( "Stopping move due to {Count} consecutive failures" ,
                                            Arg.Any < int > ( ) ) ;
    }

    [ TestMethod ]
    public async Task StartMoveAsync_AlternatingFailuresAndSuccesses_DoesNotStop ( )
    {
        var sut = CreateSut ( ) ;

        var callCount = 0 ;

        // Alternate between success and failure
        _executor.Up ( ).Returns ( _ =>
        {
            callCount++ ;
            return Task.FromResult ( callCount % 2 == 0 ) ; // Success on even calls
        } ) ;

        sut.DelayInterval = TimeSpan.FromMilliseconds ( 10 ) ;

        using var cts = new CancellationTokenSource ( 100 ) ;

        await sut.StartMoveAsync ( Direction.Up ,
                                   cts.Token ) ;

        // Verify that Up was called multiple times (more than 3, proving it didn't stop)
        await _executor.Received ( ).Up ( ) ;

        // Verify that no warning about consecutive failures was logged
        _logger.DidNotReceive ( ).Warning ( "Stopping move due to {Count} consecutive failures" ,
                                            Arg.Any < int > ( ) ) ;
    }

    [ TestMethod ]
    public async Task StartMoveAsync_ExceptionThrown_CountsAsFailure ( )
    {
        var sut = CreateSut ( ) ;

        // Simulate exceptions in the Up command
        _executor.Up ( ).Returns ( Task.FromException < bool > ( new InvalidOperationException ( "Test exception" ) ) ) ;

        sut.DelayInterval = TimeSpan.FromMilliseconds ( 10 ) ;

        using var cts = new CancellationTokenSource ( 500 ) ;

        await sut.StartMoveAsync ( Direction.Up ,
                                   cts.Token ) ;

        // Verify that Up was called exactly 3 times before stopping
        await _executor.Received ( 3 ).Up ( ) ;

        // Verify that errors were logged
        _logger.Received ( ).Error ( Arg.Any < Exception > ( ) ,
                                     "StartMoveAsync command threw exception: {Desired} (consecutive failures: {Count})" ,
                                     Direction.Up ,
                                     Arg.Any < int > ( ) ) ;

        // Verify that a warning was logged
        _logger.Received ( ).Warning ( "Stopping move due to {Count} consecutive failures" ,
                                       3 ) ;

        // Verify that the direction is reset to None
        sut.CurrentDirection.Should ( ).Be ( Direction.None ) ;
    }

    [ TestMethod ]
    public async Task StartMoveAsync_TwoFailuresThenSuccess_ContinuesWorking ( )
    {
        var sut = CreateSut ( ) ;

        var callCount = 0 ;

        // First two calls fail, then succeed
        _executor.Up ( ).Returns ( _ =>
        {
            callCount++ ;
            return Task.FromResult ( callCount > 2 ) ; // False for first two, true afterward
        } ) ;

        sut.DelayInterval = TimeSpan.FromMilliseconds ( 10 ) ;

        using var cts = new CancellationTokenSource ( 100 ) ;

        await sut.StartMoveAsync ( Direction.Up ,
                                   cts.Token ) ;

        // Verify that Up was called more than 3 times (proving it continued after reset)
        await _executor.Received ( ).Up ( ) ;

        // The failure counter should have been reset after success
        _logger.DidNotReceive ( ).Warning ( "Stopping move due to {Count} consecutive failures" ,
                                            Arg.Any < int > ( ) ) ;
    }

    [ TestMethod ]
    public async Task StartMoveAsync_DownCommandConsecutiveFailures_StopsAfterThreeFailures ( )
    {
        var sut = CreateSut ( ) ;

        // Simulate failures in the Down command
        _executor.Down ( ).Returns ( Task.FromResult ( false ) ) ;

        sut.DelayInterval = TimeSpan.FromMilliseconds ( 10 ) ;

        using var cts = new CancellationTokenSource ( 500 ) ;

        await sut.StartMoveAsync ( Direction.Down ,
                                   cts.Token ) ;

        // Verify that Down was called exactly 3 times before stopping
        await _executor.Received ( 3 ).Down ( ) ;

        // Verify that a warning was logged
        _logger.Received ( ).Warning ( "Stopping move due to {Count} consecutive failures" ,
                                       3 ) ;

        // Verify that the direction is reset to None
        sut.CurrentDirection.Should ( ).Be ( Direction.None ) ;
    }
}

