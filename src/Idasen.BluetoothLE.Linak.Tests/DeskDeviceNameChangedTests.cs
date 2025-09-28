namespace Idasen.BluetoothLE.Linak.Tests ;

using System.Reactive.Linq ;
using System.Reactive.Subjects ;
using Interfaces ;
using Microsoft.Reactive.Testing ;
using NSubstitute ;
using Selkie.AutoMocking ;

[ AutoDataTestClass ]
public sealed class DeskDeviceNameChangedTests
    : DeskRaiseEventForDeskBase < IEnumerable < byte > >
{
    protected override void SetSubscription ( IDesk desk ,
                                              TestScheduler scheduler )
    {
        ArgumentNullException.ThrowIfNull ( desk ) ;
        ArgumentNullException.ThrowIfNull ( scheduler ) ;

        desk.DeviceNameChanged
            .ObserveOn ( scheduler )
            .Subscribe ( OnRaised ) ;
    }

    protected override void SetSubject ( IDeskConnector connector ,
                                         Subject < IEnumerable < byte > > subject )
    {
        ArgumentNullException.ThrowIfNull ( connector ) ;
        ArgumentNullException.ThrowIfNull ( subject ) ;

        connector.DeviceNameChanged
                 .Returns ( subject ) ;
    }
}
