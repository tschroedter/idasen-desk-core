namespace Idasen.BluetoothLE.Linak ;

using System.Diagnostics.CodeAnalysis ;
using Interfaces ;

/// <inheritdoc />
[ ExcludeFromCodeCoverage ]
public sealed class TaskRunner
    : ITaskRunner
{
    /// <inheritdoc />
    public Task Run (
        Action action ,
        CancellationToken token )
    {
        ArgumentNullException.ThrowIfNull ( action ) ;

        return Task.Run ( action ,
                          token ) ;
    }
}
