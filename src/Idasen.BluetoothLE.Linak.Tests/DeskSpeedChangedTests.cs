using System.Reactive.Linq ;
using System.Reactive.Subjects ;
using Idasen.BluetoothLE.Linak.Interfaces ;
using Microsoft.Reactive.Testing ;
using NSubstitute ;
using Selkie.AutoMocking ;

namespace Idasen.BluetoothLE.Linak.Tests ;

[ AutoDataTestClass ]
public sealed class DeskSpeedChangedTests
    : DeskRaiseEventForDeskBase < int >
{
    protected override void SetSubscription (
        IDesk         desk ,
        TestScheduler scheduler )
    {
        ArgumentNullException.ThrowIfNull ( desk ) ;
        ArgumentNullException.ThrowIfNull ( scheduler ) ;

        desk.SpeedChanged
            .ObserveOn ( scheduler )
            .Subscribe ( OnRaised ) ;
    }

    protected override void SetSubject (
        IDeskConnector  connector ,
        Subject < int > subject )
    {
        ArgumentNullException.ThrowIfNull ( connector ) ;
        ArgumentNullException.ThrowIfNull ( subject ) ;

        connector.SpeedChanged
                 .Returns ( subject ) ;
    }
}
