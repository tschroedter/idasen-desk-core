namespace Idasen.BluetoothLE.Linak.Control ;

/// <summary>
///     Control commands supported by the desk.
/// </summary>
public enum DeskCommands
{
    /// <summary>
    ///     No command.
    /// </summary>
    None = 0 ,

    /// <summary>
    ///     Move up command.
    /// </summary>
    MoveUp = 1 ,

    /// <summary>
    ///     Move down command.
    /// </summary>
    MoveDown = 2 ,

    /// <summary>
    ///     StopListening movement command.
    /// </summary>
    MoveStop = 3
}