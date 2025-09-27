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
    private ILogger _logger = null! ;

    [ TestInitialize ]
    public void Init ( )
    {
        _logger = Substitute.For < ILogger > ( ) ;
        _executor = Substitute.For < IDeskCommandExecutor > ( ) ;

        // Default: succeed immediately for all commands unless overridden in a test
        _executor.Up ( ).Returns ( true ) ;
        _executor.Down ( ).Returns ( true ) ;
        _executor.Stop ( ).Returns ( true ) ;
    }

    private DeskMoveEngine CreateSut ( DeskMoverSettings? settings = null )
    {
        return new DeskMoveEngine ( _logger ,
                                    _executor ,
                                    settings ) ;
    }

    private DeskMoveEngine CreateSutWithKeepAlive ( TimeSpan keepAlive )
    {
        return CreateSut ( new DeskMoverSettings
        {
            KeepAliveInterval = keepAlive
        } ) ;
    }

    [ TestMethod ]
    public void Initial_State_IsIdle ( )
    {
        var sut = CreateSut ( ) ;

        sut.CurrentDirection.Should ( ).Be ( Direction.None ) ;
        sut.IsMoving.Should ( ).BeFalse ( ) ;
    }

    [ TestMethod ]
    public void Move_None_DoesNothing ( )
    {
        var sut = CreateSut ( ) ;

        sut.Move ( Direction.None ,
                   false ) ;

        _ = _executor.DidNotReceive ( ).Up ( ) ;
        _ = _executor.DidNotReceive ( ).Down ( ) ;
    }

    [ TestMethod ]
    public void Move_Up_FromIdle_SendsUpAndSetsDirection ( )
    {
        var sut = CreateSut ( ) ;

        sut.Move ( Direction.Up ,
                   false ) ;

        _ = _executor.Received ( 1 ).Up ( ) ;
        sut.CurrentDirection.Should ( ).Be ( Direction.Up ) ;
        sut.IsMoving.Should ( ).BeTrue ( ) ;
    }

    [ TestMethod ]
    public void Move_Down_FromIdle_SendsDownAndSetsDirection ( )
    {
        var sut = CreateSut ( ) ;

        sut.Move ( Direction.Down ,
                   false ) ;

        _ = _executor.Received ( 1 ).Down ( ) ;
        sut.CurrentDirection.Should ( ).Be ( Direction.Down ) ;
        sut.IsMoving.Should ( ).BeTrue ( ) ;
    }

    [ TestMethod ]
    public void Move_SameDirection_WithoutTimer_DoesNotReissue ( )
    {
        var sut = CreateSut ( ) ;

        sut.Move ( Direction.Up ,
                   false ) ;
        sut.Move ( Direction.Up ,
                   false ) ;

        _ = _executor.Received ( 1 ).Up ( ) ;
    }

    [ TestMethod ]
    public void Move_SameDirection_WithTimer_Reissues_WhenPreviousCompleted ( )
    {
        // disable throttling for this behavior test
        var sut = CreateSutWithKeepAlive ( TimeSpan.Zero ) ;

        sut.Move ( Direction.Up ,
                   false ) ; // first start
        sut.Move ( Direction.Up ,
                   true ) ; // keep-alive should reissue immediately

        _ = _executor.Received ( 2 ).Up ( ) ;
    }

    [ TestMethod ]
    public void Move_SameDirection_WithTimer_DoesNotReissue_WhilePending ( )
    {
        var sut = CreateSutWithKeepAlive ( TimeSpan.Zero ) ;

        var tcs = new TaskCompletionSource < bool > ( ) ;
        _executor.Up ( ).Returns ( _ => tcs.Task ) ;

        sut.Move ( Direction.Up ,
                   false ) ; // first call returns pending
        sut.Move ( Direction.Up ,
                   true ) ; // should be throttled due to pending task

        _ = _executor.Received ( 1 ).Up ( ) ;

        // complete first, then keep-alive should be sent again
        tcs.SetResult ( true ) ;
        sut.Move ( Direction.Up ,
                   true ) ;

        _ = _executor.Received ( 2 ).Up ( ) ;
    }

    [ TestMethod ]
    public void Move_OppositeDirection_WhileMoving_IsIgnored ( )
    {
        var sut = CreateSut ( ) ;

        sut.Move ( Direction.Up ,
                   false ) ;
        sut.Move ( Direction.Down ,
                   false ) ;

        _ = _executor.Received ( 1 ).Up ( ) ;
        _ = _executor.DidNotReceive ( ).Down ( ) ;
        sut.CurrentDirection.Should ( ).Be ( Direction.Up ) ;
    }

    [ TestMethod ]
    public async Task StopAsync_CallsExecutor_AndResetsDirection_OnSuccess ( )
    {
        var sut = CreateSut ( ) ;
        sut.Move ( Direction.Down ,
                   false ) ;
        sut.CurrentDirection.Should ( ).Be ( Direction.Down ) ;

        var ok = await sut.StopAsync ( ) ;

        ok.Should ( ).BeTrue ( ) ;
        _ = _executor.Received ( 1 ).Stop ( ) ;
        sut.CurrentDirection.Should ( ).Be ( Direction.None ) ;
        sut.IsMoving.Should ( ).BeFalse ( ) ;
    }

    [ TestMethod ]
    public void Move_CommandFailure_ResetsDirectionToNone ( )
    {
        var sut = CreateSut ( ) ;
        _executor.Up ( ).Returns ( false ) ; // completes synchronously with failure

        sut.Move ( Direction.Up ,
                   false ) ;

        sut.CurrentDirection.Should ( ).Be ( Direction.None ) ;
        _ = _executor.Received ( 1 ).Up ( ) ;
    }

    [ TestMethod ]
    public void Move_CommandFaulted_ResetsDirectionToNone ( )
    {
        var sut = CreateSut ( ) ;
        _executor.Up ( ).Returns ( Task.FromException < bool > ( new Exception ( "boom" ) ) ) ;

        sut.Move ( Direction.Up ,
                   false ) ;

        sut.CurrentDirection.Should ( ).Be ( Direction.None ) ;
        _ = _executor.Received ( 1 ).Up ( ) ;
    }

    [ TestMethod ]
    public async Task StopAsync_IsIdempotent_WhilePending ( )
    {
        var sut = CreateSut ( ) ;
        sut.Move ( Direction.Up ,
                   false ) ;

        var tcs = new TaskCompletionSource < bool > ( ) ;
        _executor.Stop ( ).Returns ( _ => tcs.Task ) ;

        var stop1 = sut.StopAsync ( ) ;
        var stop2 = sut.StopAsync ( ) ;

        stop2.Should ( ).BeSameAs ( stop1 ) ;
        _ = _executor.Received ( 1 ).Stop ( ) ;

        tcs.SetResult ( true ) ;
        await stop1 ;

        sut.CurrentDirection.Should ( ).Be ( Direction.None ) ;
    }

    [ TestMethod ]
    public async Task StopAsync_Failure_DoesNotResetDirection ( )
    {
        var sut = CreateSut ( ) ;
        sut.Move ( Direction.Up ,
                   false ) ;

        _executor.Stop ( ).Returns ( false ) ;

        var ok = await sut.StopAsync ( ) ;

        ok.Should ( ).BeFalse ( ) ;
        // Direction remains unchanged because stop failed
        sut.CurrentDirection.Should ( ).Be ( Direction.Up ) ;
    }
}