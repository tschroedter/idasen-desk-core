namespace Idasen.BluetoothLE.Core.Tests.ServicesDiscovery.Wrapper ;

using Core.ServicesDiscovery.Wrappers ;
using FluentAssertions ;
using Selkie.AutoMocking ;

[ TestClass ]
public class GattCharacteristicValueChangedDetailsTests
{
    [ AutoDataTestMethod ]
    public void Constructor_ForInvoked_SetsUuid (
        GattCharacteristicValueChangedDetails sut ,
        [ Freeze ] Guid uuid )
    {
        sut.Uuid
           .Should ( )
           .Be ( uuid ) ;
    }

    [ AutoDataTestMethod ]
    public void Constructor_ForInvoked_SetsValue (
        GattCharacteristicValueChangedDetails sut ,
        [ Freeze ] IEnumerable < byte > value )
    {
        sut.Value
           .Should ( )
           .BeEquivalentTo ( value ) ;
    }

    [ AutoDataTestMethod ]
    public void Constructor_ForInvoked_SetsTimestamp (
        GattCharacteristicValueChangedDetails sut ,
        [ Freeze ] DateTimeOffset timestamp )
    {
        sut.Timestamp
           .Should ( )
           .Be ( timestamp ) ;
    }
}
