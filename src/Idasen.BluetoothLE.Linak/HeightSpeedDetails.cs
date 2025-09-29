namespace Idasen.BluetoothLE.Linak ;

/// <summary>
///     Immutable value object containing a timestamped height and speed sample.
/// </summary>
public sealed class HeightSpeedDetails ( DateTimeOffset timestamp ,
                                         uint           height ,
                                         int            speed )
{
    /// <summary>
    ///     Gets the sample timestamp.
    /// </summary>
    public DateTimeOffset Timestamp { get ; } = timestamp ;

    /// <summary>
    ///     Gets the desk height in device units.
    /// </summary>
    public uint Height { get ; } = height ;

    /// <summary>
    ///     Gets the desk speed in device units.
    /// </summary>
    public int Speed { get ; } = speed ;

    /// <summary>
    ///     Returns a human-readable representation.
    /// </summary>
    public override string ToString ( )
    {
        return $"Timestamp = {Timestamp:O}, " +
               $"Height = {Height}, "         +
               $"Speed = {Speed}" ;
    }
}