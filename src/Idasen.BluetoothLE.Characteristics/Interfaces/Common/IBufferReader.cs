using Windows.Storage.Streams ;

namespace Idasen.BluetoothLE.Characteristics.Interfaces.Common ;

/// <summary>
///     Reads raw byte arrays from Windows runtime <see cref="IBuffer" /> instances.
/// </summary>
public interface IBufferReader
{
    /// <summary>
    ///     Attempts to copy the remaining bytes from the given buffer to a managed byte array.
    /// </summary>
    /// <param name="buffer">The Windows runtime buffer to read.</param>
    /// <param name="bytes">When this method returns, contains the copied bytes if successful; otherwise an empty array.</param>
    /// <returns><c>true</c> if bytes were read; otherwise, <c>false</c>.</returns>
    bool TryReadValue (
        IBuffer buffer ,
        out byte [ ] bytes ) ;
}