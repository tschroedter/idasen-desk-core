namespace Idasen.BluetoothLE.Linak.Control ;

using Aop.Aspects ;
using Autofac.Extras.DynamicProxy ;
using Interfaces ;
using Serilog ;

/// <summary>
///     Encapsulates stop decision logic: near-target tolerance, predicted crossing, and stall detection.
///     Also computes the desired move direction via IStoppingHeightCalculator.
/// </summary>
[ Intercept ( typeof ( LogAspect ) ) ]
internal class DeskStopper : IDeskStopper
{
    // Treat as stalled only if speed is effectively zero while height hasn't changed
    private const int StallSpeedThreshold = 1 ; // absolute speed <= 1 considered no movement

    // Minimum absolute speed required before enabling predictive crossing + overshoot compensation logic
    private const int MinPredictiveSpeed = 5 ;

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
        _logger.Debug ( "Stopper reset" ) ;
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

        Direction desired = _calculator.MoveIntoDirection ;
        var movementAbs = ( uint ) Math.Abs ( _calculator.MovementUntilStop ) ;

        // Enable predictive crossing only after we have reached a minimum speed to avoid
        // an early tiny predicted movement triggering a premature S    top/Restart jitter.
        var predictiveActive = Math.Abs ( speed ) >= MinPredictiveSpeed ;

        // OvershootCompensation semantics (see DeskMoverSettings):
        // Only apply when predictive logic is active. Early in the motion (speed below threshold) we
        // keep raw prediction to avoid immediate stop pulses.
        var compensatedMovementForPrediction = movementAbs ;

        if ( predictiveActive )
        {
            try
            {
                checked
                {
                    compensatedMovementForPrediction = movementAbs + _settings.OvershootCompensation ;
                }
            }
            catch ( OverflowException )
            {
                // fall back to raw value on overflow.
                compensatedMovementForPrediction = movementAbs ;
            }

            if ( _settings.OvershootCompensation > 0 && movementAbs > 0 )
            {
                _logger.Debug ( "Predictive active speed={Speed} rawMove={Raw} comp={Comp} used={Used}" ,
                                speed ,
                                movementAbs ,
                                _settings.OvershootCompensation ,
                                compensatedMovementForPrediction ) ;
            }
        }
        else if ( movementAbs > 0 )
        {
            _logger.Debug ( "Predictive disabled speed={Speed} (<{Min}) rawMove={Raw}" ,
                            speed ,
                            MinPredictiveSpeed ,
                            movementAbs ) ;
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
            _logger.Debug ( "Stall tick count={Cnt} activelyCommanding={Commanding}" ,
                            _noMovementPolls ,
                            activelyCommanding ) ;

            if ( ! activelyCommanding )
            {
                _logger.Debug ( "Returning stop due to stall and not actively commanding" ) ;
                return new StopDetails ( true ,
                                         desired ) ;
            }
        }
        else if ( _noMovementPolls != 0 )
        {
            _logger.Debug ( "Reset stall counter from {Cnt}" ,
                            _noMovementPolls ) ;
            _noMovementPolls = 0 ;
        }

        // tolerance based stop (dynamic tolerance limited by predicted raw movement, NOT compensated one)
        var toleranceDynamic = Math.Min ( _settings.NearTargetMaxDynamicTolerance ,
                                          movementAbs ) ;
        var tolerance = Math.Max ( _settings.NearTargetBaseTolerance ,
                                   toleranceDynamic ) ;
        var diff = height > targetHeight
                       ? height - targetHeight
                       : targetHeight - height ;

        _logger.Debug ( "Tolerance eval diff={Diff} tol={Tol} dynTol={DynTol} movementAbs={MoveAbs}" ,
                        diff ,
                        tolerance ,
                        toleranceDynamic ,
                        movementAbs ) ;

        if ( diff <= tolerance )
        {
            _logger.Debug ( "Returning stop due to tolerance diff={Diff} tol={Tol}" ,
                            diff ,
                            tolerance ) ;
            return new StopDetails ( true ,
                                     desired ) ;
        }

        // predictive crossing (use compensated movement to trigger earlier stop) - only if active
        if ( predictiveActive )
        {
            if ( desired == Direction.Up )
            {
                if ( compensatedMovementForPrediction > 0 && height < targetHeight )
                {
                    var predictedStop = height + compensatedMovementForPrediction ;
                    _logger.Debug ( "Predictive Up predictedStop={Pred} target={Target}" ,
                                    predictedStop ,
                                    targetHeight ) ;

                    if ( predictedStop >= targetHeight )
                    {
                        _logger.Debug ( "Returning stop due to predictive up crossing" ) ;
                        return new StopDetails ( true ,
                                                 desired ) ;
                    }
                }
            }
            else if ( desired == Direction.Down )
            {
                if ( compensatedMovementForPrediction > 0 && height > targetHeight )
                {
                    var delta = compensatedMovementForPrediction > height
                                    ? height
                                    : compensatedMovementForPrediction ;
                    var predictedStop = height - delta ;

                    _logger.Debug ( "Predictive Down predictedStop={Pred} target={Target}" ,
                                    predictedStop ,
                                    targetHeight ) ;

                    if ( predictedStop <= targetHeight )
                    {
                        _logger.Debug ( "Returning stop due to predictive down crossing" ) ;
                        return new StopDetails ( true ,
                                                 desired ) ;
                    }
                }
            }
        }

        if ( desired == Direction.None || _calculator.HasReachedTargetHeight )
        {
            _logger.Debug ( "Returning stop due to calculator state desired={Desired} reached={Reached}" ,
                            desired ,
                            _calculator.HasReachedTargetHeight ) ;
            return new StopDetails ( true ,
                                     desired ) ;
        }

        _logger.Debug ( "Continue movement desired={Desired}" ,
                        desired ) ;
        return new StopDetails ( false ,
                                 desired ) ;
    }
}
