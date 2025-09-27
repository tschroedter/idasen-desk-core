using Autofac.Extras.DynamicProxy ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Linak.Interfaces ;
using Serilog ;

namespace Idasen.BluetoothLE.Linak.Control ;

/// <summary>
///     Encapsulates stop decision logic: near-target tolerance, predicted crossing, and stall detection.
///     Also computes the desired move direction via IStoppingHeightCalculator.
/// </summary>
[ Intercept ( typeof ( LogAspect ) ) ]
internal class DeskStopper : IDeskStopper
{
    // Treat as stalled only if speed is effectively zero while height hasn't changed
    private const int StallSpeedThreshold = 1 ; // absolute speed <= 1 considered no movement
    private readonly IStoppingHeightCalculator _calculator ;
    private readonly IDeskHeightMonitor _heightMonitor ;
    private readonly ILogger _logger ;
    private readonly DeskMoverSettings _settings ;
    private uint? _lastHeight ;

    private int _noMovementPolls ;

    public DeskStopper ( ILogger logger , DeskMoverSettings settings , IDeskHeightMonitor heightMonitor , IStoppingHeightCalculator calculator )
    {
        ArgumentNullException.ThrowIfNull ( logger ) ;
        ArgumentNullException.ThrowIfNull ( settings ) ;
        ArgumentNullException.ThrowIfNull ( heightMonitor ) ;
        ArgumentNullException.ThrowIfNull ( calculator ) ;

        _logger = logger ;
        _settings = settings ;
        _heightMonitor = heightMonitor ;
        _calculator = calculator ;
    }

    public void Reset ( )
    {
        _noMovementPolls = 0 ;
        _lastHeight = null ;
        _heightMonitor.Reset ( ) ;
    }

    public StopDetails ShouldStop ( uint height ,
                                    int speed ,
                                    uint targetHeight ,
                                    Direction startMovingIntoDirection ,
                                    Direction currentCommandedDirection )
    {
        // compute desired and movement using calculator
        _calculator.Height = height ;
        _calculator.Speed = speed ;
        _calculator.TargetHeight = targetHeight ;
        _calculator.StartMovingIntoDirection = startMovingIntoDirection ;
        _calculator.Calculate ( ) ;

        var desired = _calculator.MoveIntoDirection ;
        var movementAbs = ( uint ) Math.Abs ( _calculator.MovementUntilStop ) ;

        // Apply overshoot compensation (systematic delay/latency). We only adjust if still moving towards target.
        if ( desired != Direction.None && movementAbs > 0 )
        {
            var compensated = movementAbs > _settings.OvershootCompensation
                                   ? movementAbs - _settings.OvershootCompensation
                                   : 0u ;
            if ( compensated != movementAbs )
            {
                _logger.Debug ( "Applying overshoot compensation: raw={Raw} compensated={Compensated} (comp={Comp})" ,
                                movementAbs ,
                                compensated ,
                                _settings.OvershootCompensation ) ;
            }
            movementAbs = compensated ;
        }

        // Only push distinct heights to the monitor to avoid false "no change" from repeated evaluations
        var isNewSample = _lastHeight is null || _lastHeight.Value != height ;

        if ( isNewSample )
        {
            _heightMonitor.AddHeight ( height ) ;
            _lastHeight = height ;
        }

        // Stalling detection: only count when height hasn't changed AND speed is effectively zero
        var activelyCommanding = currentCommandedDirection != Direction.None ;
        var stalledTick = ! isNewSample && Math.Abs ( speed ) <= StallSpeedThreshold ;

        if ( stalledTick )
        {
            _noMovementPolls++ ;

            // Previous logic stopped after a fixed number of stalled ticks even while actively commanding.
            // This caused long movements (> ~10cm) to stutter periodically. We now only stop immediately
            // when we are NOT actively commanding (lost command) – but we keep counting otherwise so that
            // future adaptive logic could use it (without triggering spurious stops).
            if ( ! activelyCommanding )
            {
                return new StopDetails ( true ,
                                         desired ) ;
            }
        }
        else
        {
            // Reset counter on any movement or speed change
            if ( _noMovementPolls != 0 )
            {
                _noMovementPolls = 0 ;
            }
        }

        // tolerance based stop
        var toleranceDynamic = Math.Min ( _settings.NearTargetMaxDynamicTolerance ,
                                          movementAbs ) ;
        var tolerance = Math.Max ( _settings.NearTargetBaseTolerance ,
                                   toleranceDynamic ) ;
        var diff = height > targetHeight
                       ? height - targetHeight
                       : targetHeight - height ;

        if ( diff <= tolerance )
        {
            return new StopDetails ( true ,
                                     desired ) ;
        }

        // predictive crossing (with compensated movementAbs)
        if ( desired == Direction.Up )
        {
            if ( movementAbs > 0 && height < targetHeight )
            {
                var predictedStop = height + movementAbs ;

                if ( predictedStop >= targetHeight )
                {
                    return new StopDetails ( true ,
                                             desired ) ;
                }
            }
        }
        else if ( desired == Direction.Down )
        {
            if ( movementAbs > 0 && height > targetHeight )
            {
                var predictedStop = height <= movementAbs
                                        ? 0u
                                        : height - movementAbs ;

                if ( predictedStop <= targetHeight )
                {
                    return new StopDetails ( true ,
                                             desired ) ;
                }
            }
        }

        if ( desired == Direction.None || _calculator.HasReachedTargetHeight )
        {
            return new StopDetails ( true ,
                                     desired ) ;
        }

        return new StopDetails ( false ,
                                 desired ) ;
    }
}