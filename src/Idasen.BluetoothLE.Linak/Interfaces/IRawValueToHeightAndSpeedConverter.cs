namespace Idasen.BluetoothLE.Linak.Interfaces;

/// <summary>
///     Converts raw byte sequences from the desk into typed height and speed values.
/// </summary>
public interface IRawValueToHeightAndSpeedConverter
{
    /// <summary>
    ///     Attempts to convert the provided raw bytes into height and speed values.
    /// </summary>
    /// <param name="bytes">The raw byte payload containing height and speed data.</param>
    /// <param name="height">
    ///     When this method returns, contains the parsed height (in device units) if the conversion
    ///     succeeded; otherwise 0.
    /// </param>
    /// <param name="speed">
    ///     When this method returns, contains the parsed speed (in device units) if the conversion succeeded;
    ///     otherwise 0.
    /// </param>
    /// <returns>
    ///     true if the bytes could be converted successfully; otherwise, false.
    /// </returns>
    bool TryConvert(
        IEnumerable<byte> bytes,
        out uint height,
        out int speed);
}
