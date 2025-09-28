using Idasen.BluetoothLE.Linak.Control ;

namespace Idasen.BluetoothLE.Linak.Interfaces ;

/// <summary>
///     Provides raw command payloads for desk operations.
/// </summary>
public interface IDeskCommandsProvider
{
    /// <summary>
    ///     Tries to get the raw payload for the specified command.
    /// </summary>
    /// <param name="command">The command for which to resolve the byte payload.</param>
    /// <param name="bytes">When this method returns, contains the bytes if found; otherwise an empty sequence.</param>
    /// <returns>true if the command was found; otherwise, false.</returns>
    bool TryGetValue (
        DeskCommands             command ,
        out IEnumerable < byte > bytes ) ;
}
