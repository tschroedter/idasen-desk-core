using Idasen.BluetoothLE.Core ;
using Idasen.BluetoothLE.Linak.Interfaces ;

namespace Idasen.BluetoothLE.Linak ;

/// <summary>
///     Default implementation of <see cref="IErrorDetails" /> that carries an error message, optional exception, and
///     caller.
/// </summary>
public class ErrorDetails // todo testing
    : IErrorDetails
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ErrorDetails" /> class.
    /// </summary>
    /// <param name="message">The human-readable message.</param>
    /// <param name="caller">The originating member name or component.</param>
    /// <param name="exception">Optional exception to include.</param>
    public ErrorDetails (
        string message ,
        string caller ,
        Exception? exception = null )
    {
        Guard.ArgumentNotNull ( message ,
                                nameof ( message ) ) ;
        Guard.ArgumentNotNull ( caller ,
                                nameof ( caller ) ) ;

        Message = message ;
        Exception = exception ;
        Caller = caller ;
    }

    /// <inheritdoc />
    public string Message { get ; }

    /// <inheritdoc />
    public Exception? Exception { get ; }

    /// <inheritdoc />
    public string Caller { get ; }

    /// <summary>
    ///     Returns a readable representation of this error.
    /// </summary>
    public override string ToString ( )
    {
        return Exception == null
                   ? $"[{Caller}] {Message}"
                   : $"[{Caller}] {Message} ({Exception.Message})" ;
    }
}