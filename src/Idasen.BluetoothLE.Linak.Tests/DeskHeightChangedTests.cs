using System.Reactive.Linq ;
using System.Reactive.Subjects ;
using Idasen.BluetoothLE.Linak.Interfaces ;
using Microsoft.Reactive.Testing ;
using NSubstitute ;
// Selkie.AutoMocking removed; tests use explicit setup

namespace Idasen.BluetoothLE.Linak.Tests ;

[ TestClass ]
public sealed class DeskHeightChangedTests
    : DeskRaiseEventForDeskBase < uint >
{
    protected override void SetSubscription ( IDesk         desk ,
                                              TestScheduler scheduler )
    {
        ArgumentNullException.ThrowIfNull ( desk ) ;
        ArgumentNullException.ThrowIfNull ( scheduler ) ;

        desk.HeightChanged
            .ObserveOn ( scheduler )
            .Subscribe ( OnRaised ) ;
    }

    protected override void SetSubject ( IDeskConnector   connector ,
                                         Subject < uint > subject )
    {
        ArgumentNullException.ThrowIfNull ( connector ) ;
        ArgumentNullException.ThrowIfNull ( subject ) ;

        connector.HeightChanged
                 .Returns ( subject ) ;
    }
}
