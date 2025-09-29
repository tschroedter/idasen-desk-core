namespace Idasen.BluetoothLE.Linak.Control ;

/// <summary>
///     Configuration settings for DeskMover behavior.
/// </summary>
public class DeskMoverSettings // todo make all the properties settable via ISettings
{
    /// <summary>
    ///     Interval for movement evaluation and sampling.
    /// </summary>
    public TimeSpan TimerInterval { get ; init ; } = TimeSpan.FromMilliseconds ( 100 ) ;

    /// <summary>
    ///     Minimum tolerance band when approaching the target height.
    /// </summary>
    public uint NearTargetBaseTolerance { get ; init ; } = 2u ;

    /// <summary>
    ///     Maximum dynamic tolerance based on predicted movement until stop.
    /// </summary>
    public uint NearTargetMaxDynamicTolerance { get ; init ; } = 10u ;

    /// <summary>
    ///     Compensation (in mm units) added to predicted movement to counter systematic overshoot.
    ///     Increase if desk still stops beyond target, decrease if it now undershoots. Default targets ~1cm correction.
    /// </summary>
    public uint OvershootCompensation { get ; init ; } = 10u ;

    /// <summary>
    ///     Minimum elapsed time between re-issued keep-alive move commands while already moving.
    ///     Throttling reduces BLE command spam that can cause perceived stutter on some controllers.
    /// </summary>
    public TimeSpan KeepAliveInterval { get ; init ; } = TimeSpan.FromMilliseconds ( 400 ) ;

    /// <summary>
    ///     Provides a shared default <see cref="DeskMoverSettings" /> instance with standard values.
    /// </summary>
    public static DeskMoverSettings Default { get ; } = new( ) ;
}