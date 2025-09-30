using Idasen.BluetoothLE.Linak.Control ;

namespace Idasen.BluetoothLE.Linak.Interfaces ;

/// <summary>
///     Interface for DeskMoveGuard, which guards desk movement based on height and direction.
/// </summary>
public interface IDeskMoveGuard : IDisposable
{
    IObservable < uint > TargetHeightReached { get ; }

    /// <summary>
    ///     Starts guarding the movement in the specified direction until the target height is reached.
    /// </summary>
    void StartGuarding ( Direction         direction ,
                         uint              targetHeight ,
                         CancellationToken none ) ;

    /// <summary>
    ///     Stops guarding the movement.
    /// </summary>
    void StopGuarding ( ) ;
}
