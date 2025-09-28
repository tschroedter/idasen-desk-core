namespace Idasen.BluetoothLE.Linak.Interfaces ;

using Characteristics.Interfaces.Characteristics ;

/// <summary>
///     Factory for creating <see cref="IDeskCommandExecutor" /> instances bound to a control characteristic.
/// </summary>
public interface IDeskCommandExecutorFactory
{
    /// <summary>
    ///     Creates a command executor for the given control characteristic.
    /// </summary>
    IDeskCommandExecutor Create ( IControl control ) ;
}
