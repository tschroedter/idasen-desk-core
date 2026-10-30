using System.Reactive.Linq ;
using System.Reactive.Subjects ;
using Idasen.BluetoothLE.Linak.Interfaces ;
using Microsoft.Reactive.Testing ;
using NSubstitute ;
// Selkie.AutoMocking removed; tests use explicit setup

namespace Idasen.BluetoothLE.Linak.Tests ;

[ TestClass ]
public sealed class DeskDeviceNameChangedTests
    : DeskRaiseEventForDeskBase < IEnumerable < byte > >
{
    protected override void SetSubscription ( IDesk         desk ,
                                              TestScheduler scheduler )
    {
        ArgumentNullException.ThrowIfNull ( desk ) ;
        ArgumentNullException.ThrowIfNull ( scheduler ) ;

        desk.DeviceNameChanged
            .ObserveOn ( scheduler )
            .Subscribe ( OnRaised ) ;
    }

    protected override void SetSubject ( IDeskConnector                   connector ,
                                         Subject < IEnumerable < byte > > subject )
    {
        ArgumentNullException.ThrowIfNull ( connector ) ;
        ArgumentNullException.ThrowIfNull ( subject ) ;

        connector.DeviceNameChanged
                 .Returns ( subject ) ;
    }
}
