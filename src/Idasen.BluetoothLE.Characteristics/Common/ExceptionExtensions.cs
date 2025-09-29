using Idasen.BluetoothLE.Core;
using Serilog;

namespace Idasen.BluetoothLE.Characteristics.Common;

/// <summary>
///     Extensions for handling and logging Bluetooth-related exceptions.
/// </summary>
public static class ExceptionExtensions
{
    /// <summary>
    ///     Determines whether the given exception indicates that Bluetooth is disabled or otherwise unavailable.
    /// </summary>
    /// <param name="exception">The exception to inspect.</param>
    /// <returns>
    ///     True if the exception's HResult matches a known Bluetooth-disabled/unavailable code; otherwise, false.
    /// </returns>
    public static bool IsBluetoothDisabledException(this Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var hresult = (uint)exception.HResult;

        return hresult == 0x8007048F || // ERROR_DEVICE_NOT_CONNECTED
               hresult == 0x800710DF || // ERROR_REMOTE_SESSION_LIMIT_EXCEEDED (used by stack)
               hresult == 0x8000FFFF; // E_UNEXPECTED
    }

    /// <summary>
    ///     Logs a structured information message about a Bluetooth status exception including its HResult.
    /// </summary>
    /// <param name="exception">The exception to log.</param>
    /// <param name="log">The logger.</param>
    /// <param name="message">Optional message to append.</param>
    public static void LogBluetoothStatusException(
        this Exception exception,
        ILogger log,
        string? message)
    {
        ArgumentNullException.ThrowIfNull(exception);
        Guard.ArgumentNotNull(
            log,
            nameof(log));

        // Prefer structured logging to preserve HResult as number and message pieces separately
        log.Information(
            "{Base} (0x{HResult:X}) {Message}",
            Constants.CheckAndEnableBluetooth,
            exception.HResult,
            message ?? string.Empty);
    }
}
