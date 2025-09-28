namespace Idasen.BluetoothLE.Core.Tests.DevicesDiscovery ;

using Windows.Devices.Bluetooth.Advertisement ;
using Core.DevicesDiscovery ;
using FluentAssertions ;

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
    public void Map_ForStatus_ReturnsWatcherStatus (
        BluetoothLEAdvertisementWatcherStatus bluetoothStatus ,
        Status status )
    {
        CreateSut ( ).Map ( bluetoothStatus )
                     .Should ( )
                     .Be ( status ) ;
    }

    [ TestMethod ]
    public void Map_ForUnknownStatus_ThrowsArgumentException ( )
    {
        var unknown = ( BluetoothLEAdvertisementWatcherStatus ) 999 ;

        Func < Status > action = ( ) => CreateSut ( ).Map ( unknown ) ;

        action.Should ( )
              .Throw < ArgumentException > ( )
              .WithMessage ( "Unknown status: '*" ) ;
    }

    private StatusMapper CreateSut ( )
    {
        var sut = new StatusMapper ( ) ;

        return sut ;
    }
}
