namespace Idasen.BluetoothLE.Linak.Interfaces ;

/// <summary>
///     Executes raw movement commands against the desk.
/// </summary>
public interface IDeskCommandExecutor
{
    /// <summary>
    ///     Sends the command to move up.
    /// </summary>
    Task < bool > Up ( ) ;

    /// <summary>
    ///     Sends the command to move down.
    /// </summary>
    Task < bool > Down ( ) ;

    /// <summary>
    ///     Sends the command to stop moving.
    /// </summary>
    Task < bool > StopMovement ( ) ;
}
