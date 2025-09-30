namespace Idasen.BluetoothLE.Linak.Interfaces ;

/// <summary>
///     Factory for creating configured <see cref="IDeskMover" /> instances.
/// </summary>
public interface IDeskMoverFactory
{
    /// <summary>
    ///     Creates a new <see cref="IDeskMover" /> using the given command executor and height/speed source.
    /// </summary>
    /// <param name="executor">Executor used to send movement commands.</param>
    /// <param name="heightAndSpeed">Source providing height and speed values and notifications.</param>
    /// <returns>A configured <see cref="IDeskMover" />.</returns>
    IDeskMover Create ( IDeskCommandExecutor executor ,
                        IDeskHeightAndSpeed  heightAndSpeed ) ;
}