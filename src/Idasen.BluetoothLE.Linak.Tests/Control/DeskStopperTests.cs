using System;
using FluentAssertions;
using Idasen.BluetoothLE.Linak.Control;
using Idasen.BluetoothLE.Linak.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Serilog;

namespace Idasen.BluetoothLE.Linak.Tests.Control;

[TestClass]
public class DeskStopperTests
{
    private ILogger _logger = null!;
    private DeskMoverSettings _settings = null!;
    private IDeskHeightMonitor _heightMonitor = null!;
    private IStoppingHeightCalculator _calculator = null!;

    [TestInitialize]
    public void Init()
    {
        _logger = Substitute.For<ILogger>();
        _settings = new DeskMoverSettings
        {
            TimerInterval = TimeSpan.FromMilliseconds(100),
            NearTargetBaseTolerance = 2,
            NearTargetMaxDynamicTolerance = 10
        };

        _heightMonitor = Substitute.For<IDeskHeightMonitor>();
        _heightMonitor.IsHeightChanging().Returns(true); // default: moving

        _calculator = Substitute.For<IStoppingHeightCalculator>();
        _calculator.Calculate().Returns(_calculator);
        _calculator.MoveIntoDirection.Returns(Direction.Up);
        _calculator.MovementUntilStop.Returns(0);
        _calculator.HasReachedTargetHeight.Returns(false);
    }

    private DeskStopper CreateSut() => new DeskStopper(_logger, _settings, _heightMonitor, _calculator);

    [TestMethod]
    public void Reset_CallsHeightMonitorReset()
    {
        var sut = CreateSut();
        sut.Reset();
        _heightMonitor.Received(1).Reset();
    }

    [TestMethod]
    public void ShouldStop_SetsCalculatorInputs_AndCallsCalculate_AndAddsHeight()
    {
        var sut = CreateSut();

        var height = 1000u;
        var speed = 50;
        var target = 2000u;
        var start = Direction.Up;
        var current = Direction.Up;

        _heightMonitor.IsHeightChanging().Returns(true);

        var result = sut.ShouldStop(height, speed, target, start, current);

        result.Should().NotBeNull();

        // calculator inputs were set
        _calculator.Received(1).Height = height;
        _calculator.Received(1).Speed = speed;
        _calculator.Received(1).TargetHeight = target;
        _calculator.Received(1).StartMovingIntoDirection = start;
        _calculator.Received(1).Calculate();

        // height monitor fed
        _heightMonitor.Received(1).AddHeight(height);
    }

    [TestMethod]
    public void ShouldStop_WhenWithinTolerance_Stops()
    {
        var sut = CreateSut();

        var height = 995u;         // 5 below target
        var target = 1000u;
        var speed = 100;

        _heightMonitor.IsHeightChanging().Returns(true);
        _calculator.MoveIntoDirection.Returns(Direction.Up);
        _calculator.MovementUntilStop.Returns(5); // tolerance becomes 5
        _calculator.HasReachedTargetHeight.Returns(false);

        var details = sut.ShouldStop(height, speed, target, Direction.Up, Direction.Up);

        details.ShouldStop.Should().BeTrue();
    }

    [TestMethod]
    public void ShouldStop_PredictiveCrossing_Up_Stops()
    {
        var sut = CreateSut();

        var height = 900u;
        var target = 1000u;

        _heightMonitor.IsHeightChanging().Returns(true);
        _calculator.MoveIntoDirection.Returns(Direction.Up);
        _calculator.MovementUntilStop.Returns(200); // predicted stop 1100 >= target
        _calculator.HasReachedTargetHeight.Returns(false);

        var details = sut.ShouldStop(height, speed: 100, target, Direction.Up, Direction.Up);

        details.ShouldStop.Should().BeTrue();
    }

    [TestMethod]
    public void ShouldStop_PredictiveCrossing_Down_Stops()
    {
        var sut = CreateSut();

        var height = 1200u;
        var target = 1100u;

        _heightMonitor.IsHeightChanging().Returns(true);
        _calculator.MoveIntoDirection.Returns(Direction.Down);
        _calculator.MovementUntilStop.Returns(-150); // abs -> 150, predicted stop 1050 <= target
        _calculator.HasReachedTargetHeight.Returns(false);

        var details = sut.ShouldStop(height, speed: -100, target, Direction.Down, Direction.Down);

        details.ShouldStop.Should().BeTrue();
    }

    [TestMethod]
    public void ShouldStop_DesiredNone_Stops()
    {
        var sut = CreateSut();

        _heightMonitor.IsHeightChanging().Returns(true);
        _calculator.MoveIntoDirection.Returns(Direction.None);
        _calculator.HasReachedTargetHeight.Returns(false);

        var details = sut.ShouldStop(height: 1000, speed: 0, targetHeight: 2000, startMovingIntoDirection: Direction.Up, currentCommandedDirection: Direction.Up);

        details.ShouldStop.Should().BeTrue();
    }

    [TestMethod]
    public void ShouldStop_WhenHasReachedTarget_Stops()
    {
        var sut = CreateSut();

        _heightMonitor.IsHeightChanging().Returns(true);
        _calculator.MoveIntoDirection.Returns(Direction.Up);
        _calculator.HasReachedTargetHeight.Returns(true);

        var details = sut.ShouldStop(height: 1500, speed: 10, targetHeight: 1500, startMovingIntoDirection: Direction.Up, currentCommandedDirection: Direction.Up);

        details.ShouldStop.Should().BeTrue();
    }

    [TestMethod]
    public void ShouldStop_NoMovement_AndNotActivelyCommanding_StopsImmediately()
    {
        var sut = CreateSut();

        _heightMonitor.IsHeightChanging().Returns(false);
        _calculator.MoveIntoDirection.Returns(Direction.Up);
        _calculator.MovementUntilStop.Returns(0);
        _calculator.HasReachedTargetHeight.Returns(false);

        var details = sut.ShouldStop(height: 1000, speed: 10, targetHeight: 5000, startMovingIntoDirection: Direction.Up, currentCommandedDirection: Direction.None);

        details.ShouldStop.Should().BeTrue();
    }

    [TestMethod]
    public void ShouldStop_NoMovement_LongStall_WhileActivelyCommanding_StopsOnThreshold()
    {
        var sut = CreateSut();

        _heightMonitor.IsHeightChanging().Returns(false);
        _calculator.MoveIntoDirection.Returns(Direction.Up);
        _calculator.MovementUntilStop.Returns(0);
        _calculator.HasReachedTargetHeight.Returns(false);

        // 19 polls -> below threshold, should not stop (actively commanding)
        for (var i = 0; i < 19; i++)
        {
            var d = sut.ShouldStop(height: 1000, speed: 10, targetHeight: 5000, startMovingIntoDirection: Direction.Up, currentCommandedDirection: Direction.Up);
            d.ShouldStop.Should().BeFalse();
        }

        // 20th poll -> reaches threshold, should stop
        var details = sut.ShouldStop(height: 1000, speed: 10, targetHeight: 5000, startMovingIntoDirection: Direction.Up, currentCommandedDirection: Direction.Up);
        details.ShouldStop.Should().BeTrue();
    }

    [TestMethod]
    public void ShouldStop_NoMovement_ResetsCounter_WhenHeightStartsChanging()
    {
        var sut = CreateSut();

        _calculator.MoveIntoDirection.Returns(Direction.Up);
        _calculator.MovementUntilStop.Returns(0);
        _calculator.HasReachedTargetHeight.Returns(false);

        // 19 non-changing polls while actively commanding
        _heightMonitor.IsHeightChanging().Returns(false);
        for (var i = 0; i < 19; i++)
        {
            var d = sut.ShouldStop(height: 1000, speed: 10, targetHeight: 5000, startMovingIntoDirection: Direction.Up, currentCommandedDirection: Direction.Up);
            d.ShouldStop.Should().BeFalse();
        }

        // one changing poll -> reset internal counter
        _heightMonitor.IsHeightChanging().Returns(true);
        var resetEffect = sut.ShouldStop(height: 1001, speed: 10, targetHeight: 5000, startMovingIntoDirection: Direction.Up, currentCommandedDirection: Direction.Up);
        resetEffect.ShouldStop.Should().BeFalse(); // no other stop condition met

        // back to non-changing: should not stop immediately (counter restarted)
        _heightMonitor.IsHeightChanging().Returns(false);
        var afterReset = sut.ShouldStop(height: 1002, speed: 10, targetHeight: 5000, startMovingIntoDirection: Direction.Up, currentCommandedDirection: Direction.Up);
        afterReset.ShouldStop.Should().BeFalse();
    }

    [TestMethod]
    public void ShouldStop_FarFromTarget_NoPredictive_NoStall_ReturnsFalse()
    {
        var sut = CreateSut();

        _heightMonitor.IsHeightChanging().Returns(true);
        _calculator.MoveIntoDirection.Returns(Direction.Up);
        _calculator.MovementUntilStop.Returns(0);
        _calculator.HasReachedTargetHeight.Returns(false);

        var details = sut.ShouldStop(height: 1000, speed: 10, targetHeight: 5000, startMovingIntoDirection: Direction.Up, currentCommandedDirection: Direction.Up);

        details.ShouldStop.Should().BeFalse();
    }
}
