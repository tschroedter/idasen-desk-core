namespace Idasen.BluetoothLE.Linak.Tests.Control ;

using FluentAssertions ;
using Interfaces ;
using Linak.Control ;
using NSubstitute ;
using Serilog ;

[ TestClass ]
public class DeskStopperTests
{
    private IStoppingHeightCalculator _calculator = null! ;
    private IDeskHeightMonitor _heightMonitor = null! ;
    private ILogger _logger = null! ;
    private DeskMoverSettings _settings = null! ;

    [ TestInitialize ]
    public void Init ( )
    {
        _logger = Substitute.For < ILogger > ( ) ;
        _settings = new DeskMoverSettings
        {
            TimerInterval = TimeSpan.FromMilliseconds ( 100 ) ,
            NearTargetBaseTolerance = 2 ,
            NearTargetMaxDynamicTolerance = 10
        } ;

        _heightMonitor = Substitute.For < IDeskHeightMonitor > ( ) ;
        _heightMonitor.IsHeightChanging ( ).Returns ( true ) ; // default: moving

        _calculator = Substitute.For < IStoppingHeightCalculator > ( ) ;
        _calculator.Calculate ( ).Returns ( _calculator ) ;
        _calculator.MoveIntoDirection.Returns ( Direction.Up ) ;
        _calculator.MovementUntilStop.Returns ( 0 ) ;
        _calculator.HasReachedTargetHeight.Returns ( false ) ;
    }

    private DeskStopper CreateSut ( )
    {
        return new DeskStopper ( _logger ,
                                 _settings ,
                                 _heightMonitor ,
                                 _calculator ) ;
    }

    [ TestMethod ]
    public void Reset_CallsHeightMonitorReset ( )
    {
        DeskStopper sut = CreateSut ( ) ;
        sut.Reset ( ) ;
        _heightMonitor.Received ( 1 ).Reset ( ) ;
    }

    [ TestMethod ]
    public void ShouldStop_SetsCalculatorInputs_AndCallsCalculate_AndAddsHeight ( )
    {
        DeskStopper sut = CreateSut ( ) ;

        var height = 1000u ;
        var speed = 50 ;
        var target = 2000u ;
        Direction start = Direction.Up ;
        Direction current = Direction.Up ;

        _heightMonitor.IsHeightChanging ( ).Returns ( true ) ;

        StopDetails result = sut.ShouldStop ( height ,
                                              speed ,
                                              target ,
                                              start ,
                                              current ) ;

        result.Should ( ).NotBeNull ( ) ;

        // calculator inputs were set
        _calculator.Received ( 1 ).Height = height ;
        _calculator.Received ( 1 ).Speed = speed ;
        _calculator.Received ( 1 ).TargetHeight = target ;
        _calculator.Received ( 1 ).StartMovingIntoDirection = start ;
        _calculator.Received ( 1 ).Calculate ( ) ;

        // height monitor fed (first distinct sample)
        _heightMonitor.Received ( 1 ).AddHeight ( height ) ;
    }

    [ TestMethod ]
    public void ShouldStop_WhenWithinTolerance_Stops ( )
    {
        DeskStopper sut = CreateSut ( ) ;

        var height = 995u ; // 5 below target
        var target = 1000u ;
        var speed = 100 ;

        _heightMonitor.IsHeightChanging ( ).Returns ( true ) ;
        _calculator.MoveIntoDirection.Returns ( Direction.Up ) ;
        _calculator.MovementUntilStop.Returns ( 5 ) ; // tolerance becomes 5
        _calculator.HasReachedTargetHeight.Returns ( false ) ;

        StopDetails details = sut.ShouldStop ( height ,
                                               speed ,
                                               target ,
                                               Direction.Up ,
                                               Direction.Up ) ;

        details.ShouldStop.Should ( ).BeTrue ( ) ;
    }

    [ TestMethod ]
    public void ShouldStop_PredictiveCrossing_Up_Stops ( )
    {
        DeskStopper sut = CreateSut ( ) ;

        var height = 900u ;
        var target = 1000u ;

        _heightMonitor.IsHeightChanging ( ).Returns ( true ) ;
        _calculator.MoveIntoDirection.Returns ( Direction.Up ) ;
        _calculator.MovementUntilStop.Returns ( 200 ) ; // predicted stop 1100 >= target
        _calculator.HasReachedTargetHeight.Returns ( false ) ;

        StopDetails details = sut.ShouldStop ( height ,
                                               100 ,
                                               target ,
                                               Direction.Up ,
                                               Direction.Up ) ;

        details.ShouldStop.Should ( ).BeTrue ( ) ;
    }

    [ TestMethod ]
    public void ShouldStop_PredictiveCrossing_Down_Stops ( )
    {
        DeskStopper sut = CreateSut ( ) ;

        var height = 1200u ;
        var target = 1100u ;

        _heightMonitor.IsHeightChanging ( ).Returns ( true ) ;
        _calculator.MoveIntoDirection.Returns ( Direction.Down ) ;
        _calculator.MovementUntilStop.Returns ( - 150 ) ; // abs -> 150, predicted stop 1050 <= target
        _calculator.HasReachedTargetHeight.Returns ( false ) ;

        StopDetails details = sut.ShouldStop ( height ,
                                               - 100 ,
                                               target ,
                                               Direction.Down ,
                                               Direction.Down ) ;

        details.ShouldStop.Should ( ).BeTrue ( ) ;
    }

    [ TestMethod ]
    public void ShouldStop_DesiredNone_Stops ( )
    {
        DeskStopper sut = CreateSut ( ) ;

        _heightMonitor.IsHeightChanging ( ).Returns ( true ) ;
        _calculator.MoveIntoDirection.Returns ( Direction.None ) ;
        _calculator.HasReachedTargetHeight.Returns ( false ) ;

        StopDetails details = sut.ShouldStop ( 1000 ,
                                               0 ,
                                               2000 ,
                                               Direction.Up ,
                                               Direction.Up ) ;

        details.ShouldStop.Should ( ).BeTrue ( ) ;
    }

    [ TestMethod ]
    public void ShouldStop_WhenHasReachedTarget_Stops ( )
    {
        DeskStopper sut = CreateSut ( ) ;

        _heightMonitor.IsHeightChanging ( ).Returns ( true ) ;
        _calculator.MoveIntoDirection.Returns ( Direction.Up ) ;
        _calculator.HasReachedTargetHeight.Returns ( true ) ;

        StopDetails details = sut.ShouldStop ( 1500 ,
                                               10 ,
                                               1500 ,
                                               Direction.Up ,
                                               Direction.Up ) ;

        details.ShouldStop.Should ( ).BeTrue ( ) ;
    }

    [ TestMethod ]
    public void ShouldStop_NoMovement_AndNotActivelyCommanding_StopsImmediately ( )
    {
        DeskStopper sut = CreateSut ( ) ;

        // Simulate no movement and not actively commanding with near-zero speed
        _heightMonitor.IsHeightChanging ( ).Returns ( false ) ;
        _calculator.MoveIntoDirection.Returns ( Direction.Up ) ;
        _calculator.MovementUntilStop.Returns ( 0 ) ;
        _calculator.HasReachedTargetHeight.Returns ( false ) ;

        // First call seeds last height; second call with same height and speed 0 triggers stall
        StopDetails d1 = sut.ShouldStop ( 1000 ,
                                          0 ,
                                          5000 ,
                                          Direction.Up ,
                                          Direction.None ) ;
        d1.ShouldStop.Should ( ).BeFalse ( ) ;

        StopDetails details = sut.ShouldStop ( 1000 ,
                                               0 ,
                                               5000 ,
                                               Direction.Up ,
                                               Direction.None ) ;

        details.ShouldStop.Should ( ).BeTrue ( ) ;
    }

    [ TestMethod ]
    public void ShouldNotStop_LongStall_WhileActivelyCommanding ( )
    {
        DeskStopper sut = CreateSut ( ) ;

        _heightMonitor.IsHeightChanging ( ).Returns ( false ) ;
        _calculator.MoveIntoDirection.Returns ( Direction.Up ) ;
        _calculator.MovementUntilStop.Returns ( 0 ) ;
        _calculator.HasReachedTargetHeight.Returns ( false ) ;

        // Seed first distinct sample so that subsequent calls count as stall ticks
        StopDetails seed = sut.ShouldStop ( 1000 ,
                                            0 ,
                                            5000 ,
                                            Direction.Up ,
                                            Direction.Up ) ;
        seed.ShouldStop.Should ( ).BeFalse ( ) ;

        // 50 polls with no movement while actively commanding should NOT trigger stop anymore
        for (var i = 0; i < 50; i++)
        {
            StopDetails d = sut.ShouldStop ( 1000 ,
                                             0 ,
                                             5000 ,
                                             Direction.Up ,
                                             Direction.Up ) ;
            d.ShouldStop.Should ( ).BeFalse ( ) ;
        }
    }

    [ TestMethod ]
    public void ShouldStop_NoMovement_ResetsCounter_WhenHeightStartsChanging ( )
    {
        DeskStopper sut = CreateSut ( ) ;

        _calculator.MoveIntoDirection.Returns ( Direction.Up ) ;
        _calculator.MovementUntilStop.Returns ( 0 ) ;
        _calculator.HasReachedTargetHeight.Returns ( false ) ;

        // Seed first distinct sample
        _heightMonitor.IsHeightChanging ( ).Returns ( false ) ;
        StopDetails seed = sut.ShouldStop ( 1000 ,
                                            0 ,
                                            5000 ,
                                            Direction.Up ,
                                            Direction.Up ) ;
        seed.ShouldStop.Should ( ).BeFalse ( ) ;

        // 10 non-changing polls while actively commanding (speed 0 so they count as stall ticks)
        for (var i = 0; i < 10; i++)
        {
            StopDetails d = sut.ShouldStop ( 1000 ,
                                             0 ,
                                             5000 ,
                                             Direction.Up ,
                                             Direction.Up ) ;
            d.ShouldStop.Should ( ).BeFalse ( ) ;
        }

        // one changing poll -> reset internal counter
        _heightMonitor.IsHeightChanging ( ).Returns ( true ) ;
        StopDetails resetEffect = sut.ShouldStop ( 1001 ,
                                                   10 ,
                                                   5000 ,
                                                   Direction.Up ,
                                                   Direction.Up ) ;
        resetEffect.ShouldStop.Should ( ).BeFalse ( ) ; // no other stop condition met

        // back to non-changing: should not stop immediately (counter restarted)
        _heightMonitor.IsHeightChanging ( ).Returns ( false ) ;
        StopDetails afterReset = sut.ShouldStop ( 1002 ,
                                                  0 ,
                                                  5000 ,
                                                  Direction.Up ,
                                                  Direction.Up ) ;
        afterReset.ShouldStop.Should ( ).BeFalse ( ) ;
    }

    [ TestMethod ]
    public void ShouldStop_FarFromTarget_NoPredictive_NoStall_ReturnsFalse ( )
    {
        DeskStopper sut = CreateSut ( ) ;

        _heightMonitor.IsHeightChanging ( ).Returns ( true ) ;
        _calculator.MoveIntoDirection.Returns ( Direction.Up ) ;
        _calculator.MovementUntilStop.Returns ( 0 ) ;
        _calculator.HasReachedTargetHeight.Returns ( false ) ;

        StopDetails details = sut.ShouldStop ( 1000 ,
                                               10 ,
                                               5000 ,
                                               Direction.Up ,
                                               Direction.Up ) ;

        details.ShouldStop.Should ( ).BeFalse ( ) ;
    }
}
