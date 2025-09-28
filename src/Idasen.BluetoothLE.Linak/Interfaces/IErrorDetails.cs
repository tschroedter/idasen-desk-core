// ReSharper disable UnusedMemberInSuper.Global

namespace Idasen.BluetoothLE.Linak.Interfaces ;

/// <summary>
///     Represents a reported error with optional exception and caller information.
/// </summary>
public interface IErrorDetails
{
    /// <summary>
    ///     Gets the human-readable error message.
    /// </summary>
    string Message { get ; }

    /// <summary>
    ///     Gets the optional exception associated with this error.
    /// </summary>
    Exception? Exception { get ; }

    /// <summary>
    ///     Gets the originating member name or component reporting the error.
    /// </summary>
    string Caller { get ; }
}
