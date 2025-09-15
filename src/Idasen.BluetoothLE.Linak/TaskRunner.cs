using System.Diagnostics.CodeAnalysis ;
using Idasen.BluetoothLE.Core ;
using Idasen.BluetoothLE.Linak.Interfaces ;

namespace Idasen.BluetoothLE.Linak ;

[ ExcludeFromCodeCoverage ]
public class TaskRunner
    : ITaskRunner
{
    public Task Run (
        Action action ,
        CancellationToken token )
    {
        Guard.ArgumentNotNull ( action ,
                                nameof ( action ) ) ;

        return Task.Run ( action ,
                          token ) ;
    }
}