using Windows.Devices.Bluetooth ;
using Windows.Devices.Bluetooth.GenericAttributeProfile ;
using FluentAssertions ;
using Idasen.BluetoothLE.Characteristics.Characteristics.Unknowns ;
using Idasen.BluetoothLE.Characteristics.Common ;

namespace Idasen.BluetoothLE.Characteristics.Tests.Characteristics.Unknowns ;

[ TestClass ]
public class DeviceTests
{
    [ TestMethod ]
    public void ConnectionStatusChanged_ForInvoked_Throws ( )
    {
        Action action = ( ) => CreateSut ( ).ConnectionStatusChanged
                                            .Subscribe ( ) ;

        action.Should ( )
              .Throw < NotInitializeException > ( ) ;
    }

    [ TestMethod ]
    public void GattServicesRefreshed_ForInvoked_Throws ( )
    {
        var action = ( ) =>
                     {
                         using var sut = CreateSut ( ) ;

                         sut.GattServicesRefreshed
                            .Subscribe ( ) ;
                     } ;

        action.Should ( )
              .Throw < NotInitializeException > ( ) ;
    }

    [ TestMethod ]
    public void GattCommunicationStatus_ForInvoked_Unreachable ( )
    {
        using var sut = CreateSut ( ) ;

        sut.GattCommunicationStatus
           .Should ( )
           .Be ( GattCommunicationStatus.Unreachable ) ;
    }

    [ TestMethod ]
    public void Name_ForInvoked_UnknownName ( )
    {
        using var sut = CreateSut ( ) ;

        sut.Name
           .Should ( )
           .Be ( Device.UnknownName ) ;
    }

    [ TestMethod ]
    public void Id_ForInvoked_UnknownId ( )
    {
        using var sut = CreateSut ( ) ;

        sut.Id
           .Should ( )
           .Be ( Device.UnknownId ) ;
    }

    [ TestMethod ]
    public void Constructor_ForInvoked_False ( )
    {
        using var sut = CreateSut ( ) ;

        sut.IsPaired
           .Should ( )
           .BeFalse ( ) ;
    }

    [ TestMethod ]
    public void GattServices_ForInvoked_Empty ( )
    {
        using var sut = CreateSut ( ) ;

        sut.GattServices
           .Should ( )
           .BeEmpty ( ) ;
    }

    [ TestMethod ]
    public void ConnectionStatus_ForInvoked_Disconnected ( )
    {
        using var sut = CreateSut ( ) ;

        sut.ConnectionStatus
           .Should ( )
           .Be ( BluetoothConnectionStatus.Disconnected ) ;
    }

    [ TestMethod ]
    public void Connect_ForInvoked_DoesNothing ( )
    {
        var action = ( ) => CreateSut ( ).Connect ( ) ;

        action.Should ( )
              .NotThrow < Exception > ( ) ;
    }

    [ TestMethod ]
    public void Dispose_ForInvoked_DoesNothing ( )
    {
        var action = ( ) => CreateSut ( ).Dispose ( ) ;

        action.Should ( )
              .NotThrow < Exception > ( ) ;
    }

    private Device CreateSut ( )
    {
        return new Device ( ) ;
    }
}