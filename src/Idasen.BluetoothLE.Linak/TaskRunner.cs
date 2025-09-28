using System.Diagnostics.CodeAnalysis ;
using Idasen.BluetoothLE.Linak.Interfaces ;

namespace Idasen.BluetoothLE.Linak ;

/// <inheritdoc />
[ ExcludeFromCodeCoverage ]
public sealed class TaskRunner
    : ITaskRunner
{
    /// <inheritdoc />
    public Task Run (
        Action            action ,
        CancellationToken token )
    {
        ArgumentNullException.ThrowIfNull ( action ) ;

        return Task.Run (
                         action ,
                         token ) ;
    }
}
