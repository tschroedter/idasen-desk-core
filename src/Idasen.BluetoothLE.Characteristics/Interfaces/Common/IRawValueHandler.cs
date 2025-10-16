namespace Idasen.BluetoothLE.Characteristics.Interfaces.Common;

/// <summary>
/// Handles reading and writing raw byte values to and from GATT characteristics.
/// Combines the functionality of <see cref="IRawValueReader" /> and <see cref="IRawValueWriter" />.
/// </summary>
public interface IRawValueHandler : IRawValueReader, IRawValueWriter
{
    // This interface inherits all members from IRawValueReader and IRawValueWriter.
}
