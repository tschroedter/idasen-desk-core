using System.Reactive.Subjects ;
using FluentAssertions ;
using Idasen.BluetoothLE.Linak.Control ;
using Idasen.BluetoothLE.Linak.Interfaces ;
using NSubstitute ;
using Serilog ;

namespace Idasen.BluetoothLE.Linak.Tests.Control ;

[ TestClass ]
public class DeskMoveGuardTests : IDisposable
{
    private IStoppingHeightCalculator      _calculator            = null! ;
    private IDeskHeightAndSpeed            _heightAndSpeed        = null! ;
    private Subject < HeightSpeedDetails > _heightAndSpeedChanged = null! ;
    private ILogger                        _logger                = null! ;

    public void Dispose ( )
    {
        _heightAndSpeedChanged.Dispose ( ) ;

        GC.SuppressFinalize ( this ) ;
    }

    [ TestInitialize ]
    public void Setup ( )
    {
        _heightAndSpeed        = Substitute.For < IDeskHeightAndSpeed > ( ) ;
        _calculator            = Substitute.For < IStoppingHeightCalculator > ( ) ;
        _logger                = Substitute.For < ILogger > ( ) ;
        _heightAndSpeedChanged = new Subject < HeightSpeedDetails > ( ) ;
        _heightAndSpeed.HeightAndSpeedChanged.Returns ( _heightAndSpeedChanged ) ;
    }

    private DeskMoveGuard CreateSut ( )
    {
        return new DeskMoveGuard ( _logger ,
                                   _heightAndSpeed ,
                                   _calculator ) ;
    }

    [ TestCleanup ]
    public void Cleanup ( )
    {
        Dispose ( ) ;
    }

    [ TestMethod ]
    public void Emits_TargetHeightReached_And_Stops_When_Up_Reaches_Target ( )
    {
        // Arrange
        uint target = 1000 ;
        _calculator.StoppingHeight.Returns ( target ) ;
        _calculator.Calculate ( ).Returns ( _calculator ) ;

        uint ?    emitted      = null ;
        using var sut          = CreateSut ( ) ;
        using var subscription = sut.TargetHeightReached.Subscribe ( x => emitted = x ) ;

        // Act
        sut.StartGuarding ( Direction.Up ,
                            target ,
                            CancellationToken.None ) ;
        _heightAndSpeedChanged.OnNext ( new HeightSpeedDetails ( DateTimeOffset.Now ,
                                                                 target ,
                                                                 10 ) ) ;

        // Assert
        emitted.Should ( ).Be ( target ) ;
    }

    [ TestMethod ]
    public void Emits_TargetHeightReached_And_Stops_When_Down_Reaches_Target ( )
    {
        // Arrange
        uint target = 500 ;
        _calculator.StoppingHeight.Returns ( target ) ;
        _calculator.Calculate ( ).Returns ( _calculator ) ;

        uint ?    emitted      = null ;
        using var sut          = CreateSut ( ) ;
        using var subscription = sut.TargetHeightReached.Subscribe ( x => emitted = x ) ;

        // Act
        sut.StartGuarding ( Direction.Down ,
                            target ,
                            CancellationToken.None ) ;
        _heightAndSpeedChanged.OnNext ( new HeightSpeedDetails ( DateTimeOffset.Now ,
                                                                 target ,
                                                                 - 10 ) ) ;

        // Assert
        emitted.Should ( ).Be ( target ) ;
    }

    [ TestMethod ]
    public void Does_Not_Emit_When_Not_Guarding ( )
    {
        // Arrange
        uint ?    emitted      = null ;
        using var sut          = CreateSut ( ) ;
        using var subscription = sut.TargetHeightReached.Subscribe ( x => emitted = x ) ;

        // Act
        _heightAndSpeedChanged.OnNext ( new HeightSpeedDetails ( DateTimeOffset.Now ,
                                                                 1000 ,
                                                                 10 ) ) ;

        // Assert
        emitted.Should ( ).BeNull ( ) ;
    }

    [ TestMethod ]
    public void Dispose_Stops_Guarding_And_Completes ( )
    {
        // Arrange
        var completed = false ;
        var sut       = CreateSut ( ) ;
        using var subscription = sut.TargetHeightReached.Subscribe ( _ => { } ,
                                                                     ( ) => completed = true ) ;

        sut.StartGuarding ( Direction.Up ,
                            1000 ,
                            CancellationToken.None ) ;

        // Act
        sut.Dispose ( ) ;

        // Assert
        completed.Should ( ).BeTrue ( ) ;
    }

    [ TestMethod ]
    public void Emits_TargetHeightReached_Immediately_When_Up_And_Height_Already_Above_Target ( )
    {
        // Arrange
        uint target        = 1000 ;
        uint currentHeight = 1200 ; // already above target
        _calculator.StoppingHeight.Returns ( currentHeight ) ;
        _calculator.Calculate ( ).Returns ( _calculator ) ;

        uint ?    emitted      = null ;
        using var sut          = CreateSut ( ) ;
        using var subscription = sut.TargetHeightReached.Subscribe ( x => emitted = x ) ;

        // Act
        sut.StartGuarding ( Direction.Up ,
                            target ,
                            CancellationToken.None ) ;
        _heightAndSpeedChanged.OnNext ( new HeightSpeedDetails ( DateTimeOffset.Now ,
                                                                 currentHeight ,
                                                                 10 ) ) ;

        // Assert
        emitted.Should ( ).Be ( currentHeight ) ;
    }

    [ TestMethod ]
    public void Emits_TargetHeightReached_Immediately_When_Down_And_Height_Already_Below_Target ( )
    {
        // Arrange
        uint target        = 1000 ;
        uint currentHeight = 800 ; // already below target
        _calculator.StoppingHeight.Returns ( currentHeight ) ;
        _calculator.Calculate ( ).Returns ( _calculator ) ;

        uint ?    emitted      = null ;
        using var sut          = CreateSut ( ) ;
        using var subscription = sut.TargetHeightReached.Subscribe ( x => emitted = x ) ;

        // Act
        sut.StartGuarding ( Direction.Down ,
                            target ,
                            CancellationToken.None ) ;
        _heightAndSpeedChanged.OnNext ( new HeightSpeedDetails ( DateTimeOffset.Now ,
                                                                 currentHeight ,
                                                                 - 10 ) ) ;

        // Assert
        emitted.Should ( ).Be ( currentHeight ) ;
    }
}