using System.Reactive.Linq;
using System.Reactive.Subjects;
using Idasen.BluetoothLE.Linak.Interfaces;
using Microsoft.Reactive.Testing;
using NSubstitute;

namespace Idasen.BluetoothLE.Linak.Tests ;

[ TestClass ]
public class DeskHeightAndSpeedChangedTests
    : DeskRaiseEventForDeskBase < HeightSpeedDetails >
{
    protected override void SetSubscription ( IDesk         desk ,
                                              TestScheduler scheduler )
    {
        desk.HeightAndSpeedChanged
            .ObserveOn ( scheduler )
            .Subscribe ( OnRaised ) ;
    }

    protected override void SetSubject ( IDeskConnector                 connector ,
                                         Subject < HeightSpeedDetails > subject )
    {
        connector.HeightAndSpeedChanged
                 .Returns ( subject ) ;
    }
}
