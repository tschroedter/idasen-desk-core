namespace Idasen.BluetoothLE.Linak.Control ;

/// <summary>
///     Configuration settings for DeskMover behavior.
/// </summary>
public class DeskMoverSettings
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
    ///     Provides a shared default <see cref="DeskMoverSettings" /> instance with standard values.
    /// </summary>
    public static DeskMoverSettings Default { get ; } = new ( ) ;
}