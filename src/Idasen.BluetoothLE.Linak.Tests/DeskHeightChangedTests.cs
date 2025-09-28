namespace Idasen.BluetoothLE.Linak.Tests ;

using System.Reactive.Linq ;
using System.Reactive.Subjects ;
using Interfaces ;
using Microsoft.Reactive.Testing ;
using NSubstitute ;
using Selkie.AutoMocking ;

[ AutoDataTestClass ]
public sealed class DeskHeightChangedTests
    : DeskRaiseEventForDeskBase < uint >
{
    protected override void SetSubscription ( IDesk desk ,
                                              TestScheduler scheduler )
    {
        ArgumentNullException.ThrowIfNull ( desk ) ;
        ArgumentNullException.ThrowIfNull ( scheduler ) ;

        desk.HeightChanged
            .ObserveOn ( scheduler )
            .Subscribe ( OnRaised ) ;
    }

    protected override void SetSubject ( IDeskConnector connector ,
                                         Subject < uint > subject )
    {
        ArgumentNullException.ThrowIfNull ( connector ) ;
        ArgumentNullException.ThrowIfNull ( subject ) ;

        connector.HeightChanged
                 .Returns ( subject ) ;
    }
}
