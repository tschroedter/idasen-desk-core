using System.Diagnostics ;
using Idasen.BluetoothLE.Linak.Interfaces ;

namespace Idasen.BluetoothLE.Linak ;

/// <inheritdoc />
[ DebuggerDisplay ( "{ToString(),nq}" ) ]
public sealed class ErrorDetails
    : IErrorDetails
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ErrorDetails" /> class.
    /// </summary>
    /// <param name="message">The human-readable message.</param>
    /// <param name="caller">The originating member name or component.</param>
    /// <param name="exception">Optional exception to include.</param>
    public ErrorDetails ( string      message ,
                          string      caller ,
                          Exception ? exception = null )
    {
        ArgumentNullException.ThrowIfNull ( message ) ;
        ArgumentNullException.ThrowIfNull ( caller ) ;

        Message   = message ;
        Exception = exception ;
        Caller    = caller ;
    }

    /// <inheritdoc />
    public string Message { get ; }

    /// <inheritdoc />
    public Exception ? Exception { get ; }

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