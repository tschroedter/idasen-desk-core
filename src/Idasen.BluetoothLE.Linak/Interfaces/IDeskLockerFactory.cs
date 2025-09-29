namespace Idasen.BluetoothLE.Linak.Interfaces ;

/// <summary>
///     Creates <see cref="IDeskLocker" /> instances for lock/unlock operations.
/// </summary>
public interface IDeskLockerFactory
{
    /// <summary>
    ///     Creates a locker using the given mover, executor, and height/speed source.
    /// </summary>
    IDeskLocker Create ( IDeskMover           deskMover ,
                         IDeskCommandExecutor executor ,
                         IDeskHeightAndSpeed  heightAndSpeed ) ;
}