// ReSharper disable UnusedMemberInSuper.Global

namespace Idasen.BluetoothLE.Linak.Interfaces ;

/// <summary>
///     The desk doesn't always provide the current height, so we have to
///     move the desk to get a reading.
/// </summary>
public interface IInitialHeightProvider
    : IDisposable
{
    /// <summary>
    ///     Notifies listeners when the provider was able to determine the
    ///     height of the desk.
    /// </summary>
    IObservable < uint > Finished { get ; }

    /// <summary>
    ///     The current height of the desk.
    /// </summary>
    uint Height { get ; }

    /// <summary>
    ///     Indicates if the current desk height is available or not.
    /// </summary>
    bool HasReceivedHeightAndSpeed { get ; }

    /// <summary>
    ///     Initializes the provider and subscribes to the necessary sources.
    /// </summary>
    void Initialize ( ) ;

    /// <summary>
    ///     StartListening the process of checking and getting the current height
    ///     of the desk.
    /// </summary>
    /// <returns>
    ///     A Task.
    /// </returns>
    Task Start ( ) ;
}