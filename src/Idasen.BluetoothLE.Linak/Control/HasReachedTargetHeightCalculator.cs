namespace Idasen.BluetoothLE.Linak.Control ;

using System.Text.Json ;
using Aop.Aspects ;
using Autofac.Extras.DynamicProxy ;
using Interfaces ;
using Serilog ;

/// <inheritdoc />
[ Intercept ( typeof ( LogAspect ) ) ]
public class HasReachedTargetHeightCalculator
    : IHasReachedTargetHeightCalculator
{
    private readonly ILogger _logger ;

    /// <summary>
    ///     Initializes a new instance of the <see cref="HasReachedTargetHeightCalculator" /> class.
    /// </summary>
    public HasReachedTargetHeightCalculator ( ILogger logger )
    {
        ArgumentNullException.ThrowIfNull ( logger ) ;

        _logger = logger ;
    }

    /// <inheritdoc />
    public int MovementUntilStop { get ; set ; }

    /// <inheritdoc />
    public Direction MoveIntoDirection { get ; set ; } = Direction.None ;

    /// <inheritdoc />
    public uint StoppingHeight { get ; set ; }

    /// <inheritdoc />
    public uint TargetHeight { get ; set ; }

    /// <inheritdoc />
    public uint Delta { get ; private set ; }

    /// <inheritdoc />
    public Direction StartMovingIntoDirection { get ; set ; }

    /// <inheritdoc />
    public bool HasReachedTargetHeight { get ; private set ; }

    /// <inheritdoc />
    public IHasReachedTargetHeightCalculator Calculate ( )
    {
        // Compute absolute difference without casts/Math.Abs (unsigned arithmetic)
        Delta = TargetHeight >= StoppingHeight
                    ? TargetHeight - StoppingHeight
                    : StoppingHeight - TargetHeight ;

        if ( StartMovingIntoDirection != MoveIntoDirection )
        {
            // StoppingHeight must be 'behind' TargetHeight when direction changed
            HasReachedTargetHeight = true ;

            return this ;
        }

        if ( IsPastTargetHeight ( ) )
        {
            HasReachedTargetHeight = true ;

            return this ;
        }

        var isCloseToTargetHeight = Delta <= Math.Abs ( MovementUntilStop ) ;

        HasReachedTargetHeight = MoveIntoDirection switch
                                 {
                                     Direction.Up => isCloseToTargetHeight || StoppingHeight >= TargetHeight ,
                                     Direction.Down => isCloseToTargetHeight || StoppingHeight <= TargetHeight ,
                                     _ => true
                                 } ;

        _logger.Debug (
                       "ReachedCalc Target={TargetHeight} Stop={StoppingHeight} Move={MoveIntoDirection} Start={StartMovingIntoDirection} Delta={Delta} UntilStop={MovementUntilStop} Reached={HasReachedTargetHeight}" ,
                       TargetHeight ,
                       StoppingHeight ,
                       MoveIntoDirection ,
                       StartMovingIntoDirection ,
                       Delta ,
                       MovementUntilStop ,
                       HasReachedTargetHeight ) ;

        return this ;
    }

    private bool IsPastTargetHeight ( )
    {
        switch (MoveIntoDirection)
        {
            case Direction.Up:
                return StoppingHeight >= TargetHeight ;
            case Direction.Down:
                return StoppingHeight <= TargetHeight ;
            case Direction.None:
                return true ;
            default:
                throw new ArgumentOutOfRangeException ( nameof ( MoveIntoDirection ) ,
                                                        MoveIntoDirection ,
                                                        "Unknown direction" ) ;
        }
    }

    /// <summary>
    ///     Returns a JSON representation of the calculation state.
    /// </summary>
    public override string ToString ( ) => $"{JsonSerializer.Serialize ( this )}" ;
}
