namespace Idasen.BluetoothLE.Linak.Interfaces ;

/// <summary>
///     Runs actions on a background thread with cancellation support.
/// </summary>
public interface ITaskRunner
{
    /// <summary>
    ///     Runs the specified action until completion or cancellation.
    /// </summary>
    /// <param name="action">The action to run.</param>
    /// <param name="token">Cancellation token to stop execution.</param>
    Task Run (
        Action            action ,
        CancellationToken token ) ;
}
