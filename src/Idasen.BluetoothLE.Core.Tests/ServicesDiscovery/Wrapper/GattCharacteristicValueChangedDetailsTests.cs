using FluentAssertions ;
using Idasen.BluetoothLE.Core.ServicesDiscovery.Wrappers ;

namespace Idasen.BluetoothLE.Core.Tests.ServicesDiscovery.Wrapper ;

[ TestClass ]
public class GattCharacteristicValueChangedDetailsTests
{
    [ TestMethod ]
    public void Constructor_ForInvoked_SetsUuid ( )
    {
        var uuid = Guid.NewGuid ( ) ;
        var sut  = new GattCharacteristicValueChangedDetails ( uuid , [] , DateTimeOffset.UtcNow ) ;

        sut.Uuid
           .Should ( )
           .Be ( uuid ) ;
    }

    [ TestMethod ]
    public void Constructor_ForInvoked_SetsValue ( )
    {
        var value = new byte [ ] { 1 , 2 , 3 } ;
        var sut = new GattCharacteristicValueChangedDetails ( Guid.NewGuid ( ) , value , DateTimeOffset.UtcNow ) ;

        sut.Value
           .Should ( )
           .BeEquivalentTo ( value ) ;
    }

    [ TestMethod ]
    public void Constructor_ForInvoked_SetsTimestamp ( )
    {
        var timestamp = DateTimeOffset.UtcNow ;
        var sut       = new GattCharacteristicValueChangedDetails ( Guid.NewGuid ( ) , [] , timestamp ) ;

        sut.Timestamp
           .Should ( )
           .Be ( timestamp ) ;
    }
}
