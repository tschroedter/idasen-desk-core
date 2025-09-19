using System.Text.Json ;
using Autofac.Extras.DynamicProxy ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Core ;
using Idasen.BluetoothLE.Linak.Interfaces ;
using Serilog ;

namespace Idasen.BluetoothLE.Linak.Control ;

[ Intercept ( typeof ( LogAspect ) ) ]
/// <summary>
///     Calculates whether the target height has been reached based on current height, speed, and stopping distance.
/// </summary>
public class HasReachedTargetHeightCalculator
    : IHasReachedTargetHeightCalculator
{
    private readonly ILogger _logger ;

    /// <summary>
    ///     Initializes a new instance of the <see cref="HasReachedTargetHeightCalculator" /> class.
    /// </summary>
    public HasReachedTargetHeightCalculator ( ILogger logger )
    {
        Guard.ArgumentNotNull ( logger ,
                                nameof ( logger ) ) ;

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
        // ReSharper disable MathAbsMethodIsRedundant
        Delta = TargetHeight >= StoppingHeight
                    ? ( uint ) Math.Abs ( TargetHeight - StoppingHeight )
                    : ( uint ) Math.Abs ( StoppingHeight - TargetHeight ) ;
        // ReSharper restore MathAbsMethodIsRedundant

        if ( StartMovingIntoDirection != MoveIntoDirection )
        {
            // must be StoppingHeight must be 'behind' TargetHeight
            HasReachedTargetHeight = true ;

            return this ;
        }

        if ( CalculatePastTargetHeight ( ) )
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

        _logger.Debug ( ToString ( ) ) ;

        return this ;
    }

    private bool CalculatePastTargetHeight ( )
    {
        switch (MoveIntoDirection)
        {
            case Direction.Up:
                if ( StoppingHeight >= TargetHeight )
                {
                    return true ;
                }

                break ;
            case Direction.Down:
                if ( StoppingHeight <= TargetHeight )
                {
                    return true ;
                }

                break ;
            case Direction.None:
                return true ;
            default:
                throw new ArgumentOutOfRangeException ( ) ;
        }

        return false ;
    }

    /// <summary>
    ///     Returns a JSON representation of the calculation state.
    /// </summary>
    public override string ToString ( )
    {
        return $"{JsonSerializer.Serialize ( this )}" ;
    }
}