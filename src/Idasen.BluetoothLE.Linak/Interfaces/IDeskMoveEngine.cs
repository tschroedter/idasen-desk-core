namespace Idasen.BluetoothLE.Linak.Interfaces ;

using Control ;

/// <summary>
///     Issues movement commands and tracks the currently commanded direction.
/// </summary>
public interface IDeskMoveEngine
{
    /// <summary>
    ///     The last direction commanded. None means not moving.
    /// </summary>
    Direction CurrentDirection { get ; }

    /// <summary>
    ///     True if a direction is currently commanded.
    /// </summary>
    bool IsMoving { get ; }

    /// <summary>
    ///     Request to move (or keep moving) in the desired direction. No-op when switching
    ///     from an already commanded opposite direction; the manager should stop first.
    /// </summary>
    void Move ( Direction desired , bool fromTimer ) ;

    /// <summary>
    ///     Issues a stop command (idempotent). Resets current direction on success.
    /// </summary>
    Task < bool > StopAsync ( ) ;
}
