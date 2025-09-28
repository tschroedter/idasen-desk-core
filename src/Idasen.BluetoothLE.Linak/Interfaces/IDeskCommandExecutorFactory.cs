using Idasen.BluetoothLE.Characteristics.Interfaces.Characteristics ;

namespace Idasen.BluetoothLE.Linak.Interfaces ;

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
