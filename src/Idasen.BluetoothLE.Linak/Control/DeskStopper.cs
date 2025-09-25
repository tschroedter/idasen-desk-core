using Autofac.Extras.DynamicProxy;
using Idasen.Aop.Aspects;
using Idasen.BluetoothLE.Linak.Interfaces;
using Serilog;

namespace Idasen.BluetoothLE.Linak.Control;

/// <summary>
///     Encapsulates stop decision logic: near-target tolerance, predicted crossing, and stall detection.
///     Also computes the desired move direction via IStoppingHeightCalculator.
/// </summary>
[Intercept(typeof(LogAspect))]
internal class DeskStopper : IDeskStopper
{
    private readonly DeskMoverSettings _settings;
    private readonly IDeskHeightMonitor _heightMonitor;
    private readonly IStoppingHeightCalculator _calculator;
    private readonly ILogger _logger;

    private int _noMovementPolls;

    public DeskStopper(ILogger logger, DeskMoverSettings settings, IDeskHeightMonitor heightMonitor, IStoppingHeightCalculator calculator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(heightMonitor);
        ArgumentNullException.ThrowIfNull(calculator);

        _logger = logger;
        _settings = settings;
        _heightMonitor = heightMonitor;
        _calculator = calculator;
    }

    public void Reset()
    {
        _noMovementPolls = 0;
        _heightMonitor.Reset();
    }

    public StopDetails ShouldStop(uint height,
                                  int speed,
                                  uint targetHeight,
                                  Direction startMovingIntoDirection,
                                  Direction currentCommandedDirection)
    {
        // compute desired and movement using calculator
        _calculator.Height = height;
        _calculator.Speed = speed;
        _calculator.TargetHeight = targetHeight;
        _calculator.StartMovingIntoDirection = startMovingIntoDirection;
        _calculator.Calculate();

        var desired = _calculator.MoveIntoDirection;
        var movementAbs = (uint)Math.Abs(_calculator.MovementUntilStop);

        _heightMonitor.AddHeight(height);

        if (!_heightMonitor.IsHeightChanging())
        {
            _noMovementPolls++;

            var longStall = _noMovementPolls >= 20; // ~2s if 100ms
            var activelyCommanding = currentCommandedDirection != Direction.None;

            if (!activelyCommanding || longStall)
            {
                return new StopDetails(true, desired);
            }
        }
        else
        {
            _noMovementPolls = 0;
        }

        // tolerance based stop
        var toleranceDynamic = Math.Min(_settings.NearTargetMaxDynamicTolerance, movementAbs);
        var tolerance = Math.Max(_settings.NearTargetBaseTolerance, toleranceDynamic);
        var diff = height > targetHeight ? height - targetHeight : targetHeight - height;
        if (diff <= tolerance)
        {
            return new StopDetails(true, desired);
        }

        // predictive crossing
        if (desired == Direction.Up)
        {
            if (movementAbs > 0 && height < targetHeight)
            {
                var predictedStop = height + movementAbs;
                if (predictedStop >= targetHeight)
                {
                    return new StopDetails(true, desired);
                }
            }
        }
        else if (desired == Direction.Down)
        {
            if (movementAbs > 0 && height > targetHeight)
            {
                var predictedStop = height <= movementAbs ? 0u : height - movementAbs;
                if (predictedStop <= targetHeight)
                {
                    return new StopDetails(true, desired);
                }
            }
        }

        if (desired == Direction.None || _calculator.HasReachedTargetHeight)
        {
            return new StopDetails(true, desired);
        }

        return new StopDetails(false, desired);
    }
}
