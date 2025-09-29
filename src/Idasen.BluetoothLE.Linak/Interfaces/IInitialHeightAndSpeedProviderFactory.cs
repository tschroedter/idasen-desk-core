namespace Idasen.BluetoothLE.Linak.Interfaces ;

/// <summary>
///     Creates providers that fetch the initial height and speed values.
/// </summary>
public interface IInitialHeightAndSpeedProviderFactory
{
    /// <summary>
    ///     Creates a provider using the given command executor and height/speed source.
    /// </summary>
    /// <param name="executor">Executor to issue initial read commands.</param>
    /// <param name="heightAndSpeed">Source that exposes the height and speed raw values and notifications.</param>
    IInitialHeightProvider Create ( IDeskCommandExecutor executor ,
                                    IDeskHeightAndSpeed  heightAndSpeed ) ;
}