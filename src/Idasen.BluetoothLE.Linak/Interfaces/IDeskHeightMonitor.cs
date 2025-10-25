using JetBrains.Annotations ;

namespace Idasen.BluetoothLE.Linak.Interfaces ;

/// <summary>
///     Tracks recent height samples to determine if the desk is moving.
/// </summary>
public interface IDeskHeightMonitor
{
    /// <summary>
    ///     Indicates whether the height has been changing over the recent window.
    /// </summary>
    [ UsedImplicitly ]
    bool IsHeightChanging ( ) ;

    /// <summary>
    ///     Resets the internal state and sample window.
    /// </summary>
    [ UsedImplicitly ]
    void Reset ( ) ;

    /// <summary>
    ///     Adds the latest height sample to the monitor.
    /// </summary>
    [ UsedImplicitly ]
    void AddHeight ( uint height ) ;
}