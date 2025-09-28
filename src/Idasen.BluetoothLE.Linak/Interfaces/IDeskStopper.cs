namespace Idasen.BluetoothLE.Linak.Interfaces ;

using Control ;

/// <summary>
///     Encapsulates logic to decide when to stop the desk and the desired direction to move next.
/// </summary>
public interface IDeskStopper
{
    /// <summary>
    ///     Resets internal counters and state at the beginning of a movement cycle.
    /// </summary>
    void Reset ( ) ;

    /// <summary>
    ///     Evaluates current state and returns stop decision and desired direction.
    /// </summary>
    StopDetails ShouldStop ( uint height ,
                             int speed ,
                             uint targetHeight ,
                             Direction startMovingIntoDirection ,
                             Direction currentCommandedDirection ) ;
}
