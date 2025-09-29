// ReSharper disable UnusedMemberInSuper.Global

namespace Idasen.BluetoothLE.Linak.Interfaces ;

/// <summary>
///     Observes and exposes the desk height and speed, with change notifications.
/// </summary>
public interface IDeskHeightAndSpeed
    : IDisposable
{
    /// <summary>
    ///     Notifies when the height changes.
    /// </summary>
    IObservable < uint > HeightChanged { get ; }

    /// <summary>
    ///     Notifies when the speed changes.
    /// </summary>
    IObservable < int > SpeedChanged { get ; }

    /// <summary>
    ///     Gets the latest height value.
    /// </summary>
    uint Height { get ; }

    /// <summary>
    ///     Gets the latest speed value.
    /// </summary>
    int Speed { get ; }

    /// <summary>
    ///     Notifies when a combined height/speed sample is available.
    /// </summary>
    IObservable < HeightSpeedDetails > HeightAndSpeedChanged { get ; }

    /// <summary>
    ///     Refreshes the underlying data from the device.
    /// </summary>
    Task Refresh ( ) ;

    /// <summary>
    ///     Initializes subscriptions and returns itself.
    /// </summary>
    IDeskHeightAndSpeed Initialize ( ) ;
}