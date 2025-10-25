using Autofac.Extras.DynamicProxy ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Linak.Interfaces ;
using Serilog ;

namespace Idasen.BluetoothLE.Linak.Control ;

/// <inheritdoc />
[ Intercept ( typeof ( LogAspect ) ) ]
public class StoppingHeightCalculator
    : IStoppingHeightCalculator
{
    private const int DefaultMaxSpeed = 6200 ; // rpm/10

    private readonly IHasReachedTargetHeightCalculator _calculator ;

    private readonly ILogger _logger ;

    /// <summary>
    ///     Initializes a new instance of the <see cref="StoppingHeightCalculator" /> class.
    /// </summary>
    public StoppingHeightCalculator ( ILogger                           logger ,
                                      IHasReachedTargetHeightCalculator calculator )
    {
        ArgumentNullException.ThrowIfNull ( logger ) ;
        ArgumentNullException.ThrowIfNull ( calculator ) ;

        _logger     = logger ;
        _calculator = calculator ;
    }

    /// <inheritdoc />
    public uint MaxSpeedToStopMovement { get ; set ; } = StoppingHeightCalculatorSettings.MaxSpeedToStopMovement ;

    /// <inheritdoc />
    public int MaxSpeed { get ; set ; } = DefaultMaxSpeed ;

    /// <inheritdoc />
    public int Speed { get ; set ; }

    /// <inheritdoc />
    public float FudgeFactor { get ; set ; } = StoppingHeightCalculatorSettings.FudgeFactor ;

    /// <inheritdoc />
    public uint TargetHeight { get ; set ; }

    /// <inheritdoc />
    public uint Height { get ; set ; }

    /// <inheritdoc />
    public uint Delta { get ; private set ; }

    /// <inheritdoc />
    public uint StoppingHeight { get ; private set ; }

    /// <inheritdoc />
    public int MovementUntilStop { get ; set ; }

    /// <inheritdoc />
    public bool HasReachedTargetHeight { get ; private set ; }

    /// <inheritdoc />
    public Direction MoveIntoDirection { get ; set ; } = Direction.None ;

    /// <inheritdoc />
    public Direction StartMovingIntoDirection { get ; set ; }

    /// <inheritdoc />
    public IStoppingHeightCalculator Calculate ( )
    {
        MoveIntoDirection = CalculateMoveIntoDirection ( ) ;

        _logger.Debug ( "Height={Height}, Speed={Speed}, StartMove={StartMovingIntoDirection}, Move={MoveIntoDirection}" ,
                        Height ,
                        Speed ,
                        StartMovingIntoDirection ,
                        MoveIntoDirection ) ;

        if ( Speed == 0 )
            CalculateForSpeedZero ( ) ;
        else
            CalculateForSpeed ( ) ;

        return this ;
    }

    private Direction CalculateMoveIntoDirection ( )
    {
        var diff      = Height - ( long )TargetHeight ;
        var threshold = ( double )MaxSpeedToStopMovement * FudgeFactor ;

        if ( Math.Abs ( diff ) <= threshold )
            return Direction.None ;

        return Height > TargetHeight
                   ? Direction.Down
                   : Direction.Up ;
    }

    private void CalculateForSpeed ( )
    {
        // Preserve sign semantics from original implementation
        MovementUntilStop = ( int )( ( float )Speed /
                                     MaxSpeed               *
                                     MaxSpeedToStopMovement *
                                     FudgeFactor ) ;

        // Original behavior: add MovementUntilStop (signed) to current height
        var stopping = Height + MovementUntilStop ;

        if ( stopping < 0 )
            stopping = 0 ;

        if ( stopping > uint.MaxValue )
            stopping = uint.MaxValue ;

        StoppingHeight = ( uint )stopping ;

        var (hasReachedTargetHeight , delta) = CalculateHasReachedTargetHeight ( ) ;

        Delta                  = delta ;
        HasReachedTargetHeight = hasReachedTargetHeight ;

        LogStatus ( ) ;
    }

    private void LogStatus ( )
    {
        _logger.Debug ( "Height={Height}, Speed={Speed}, TargetHeight={TargetHeight}, StoppingHeight={StoppingHeight}, MovementUntilStop={MovementUntilStop}, Delta={Delta}" ,
                        Height ,
                        Speed ,
                        TargetHeight ,
                        StoppingHeight ,
                        MovementUntilStop ,
                        Delta ) ;
    }

    private void CalculateForSpeedZero ( )
    {
        MovementUntilStop = 0 ;

        StoppingHeight = Height ;

        var (hasReachedTargetHeight , delta) = CalculateHasReachedTargetHeight ( ) ;

        Delta                  = delta ;
        HasReachedTargetHeight = hasReachedTargetHeight ;

        LogStatus ( ) ;
    }

    private (bool hasReachedTargetHeight , uint delta) CalculateHasReachedTargetHeight ( )
    {
        _calculator.TargetHeight             = TargetHeight ;
        _calculator.StoppingHeight           = StoppingHeight ;
        _calculator.MovementUntilStop        = MovementUntilStop ;
        _calculator.MoveIntoDirection        = MoveIntoDirection ;
        _calculator.StartMovingIntoDirection = StartMovingIntoDirection ;
        _calculator.Calculate ( ) ;

        return ( _calculator.HasReachedTargetHeight , _calculator.Delta ) ;
    }
}