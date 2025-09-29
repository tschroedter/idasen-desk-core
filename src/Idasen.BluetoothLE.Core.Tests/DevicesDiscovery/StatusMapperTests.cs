using Windows.Devices.Bluetooth.Advertisement ;
using FluentAssertions ;
using Idasen.BluetoothLE.Core.DevicesDiscovery ;

namespace Idasen.BluetoothLE.Core.Tests.DevicesDiscovery ;

[ TestClass ]
public class StatusMapperTests
{
    [ TestMethod ]
    [ DataRow ( BluetoothLEAdvertisementWatcherStatus.Started ,
                Status.Started ) ]
    [ DataRow ( BluetoothLEAdvertisementWatcherStatus.Aborted ,
                Status.Aborted ) ]
    [ DataRow ( BluetoothLEAdvertisementWatcherStatus.Created ,
                Status.Created ) ]
    [ DataRow ( BluetoothLEAdvertisementWatcherStatus.Stopped ,
                Status.Stopped ) ]
    [ DataRow ( BluetoothLEAdvertisementWatcherStatus.Stopping ,
                Status.Stopping ) ]
    public void Map_ForStatus_ReturnsWatcherStatus ( BluetoothLEAdvertisementWatcherStatus bluetoothStatus ,
                                                     Status                                status )
    {
        CreateSut ( ).Map ( bluetoothStatus )
                     .Should ( )
                     .Be ( status ) ;
    }

    [ TestMethod ]
    public void Map_ForUnknownStatus_ThrowsArgumentException ( )
    {
        var unknown = ( BluetoothLEAdvertisementWatcherStatus )999 ;

        var action = ( ) => CreateSut ( ).Map ( unknown ) ;

        action.Should ( )
              .Throw < ArgumentException > ( )
              .WithMessage ( "Unknown status: '*" ) ;
    }

    private static StatusMapper CreateSut ( )
    {
        var sut = new StatusMapper ( ) ;

        return sut ;
    }
}
