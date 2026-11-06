using FluentAssertions ;
using Idasen.BluetoothLE.Linak.Interfaces ;
using NSubstitute ;

namespace Idasen.BluetoothLE.Linak.Tests ;

[ TestClass ]
public class DeskTests
{
    [ TestMethod ]
    public void Connect_ForInvoked_CallsDeskConnector ( )
    {
        var connector = Substitute.For< IDeskConnector > ( ) ;
        using var sut = new Desk ( connector ) ;

        sut.Connect ( ) ;

        connector.Received ( )
                 .Connect ( ) ;
    }

    [ TestMethod ]
    public void MoveTo_ForTargetHeight_CallsConnector ( )
    {
        var connector = Substitute.For< IDeskConnector > ( ) ;
        using var sut = new Desk ( connector ) ;

        const uint targetHeight = 100u ;

        sut.MoveTo ( targetHeight ) ;

        connector.Received ( )
                 .MoveTo ( targetHeight ) ;
    }

    [ TestMethod ]
    public async Task MoveUpAsync_ForInvoked_CallsConnector ( )
    {
        var connector = Substitute.For< IDeskConnector > ( ) ;
        connector.MoveUpAsync ( ).Returns ( Task.FromResult ( true ) ) ;
        using var sut = new Desk ( connector ) ;

        await sut.MoveUpAsync ( ) ;

        await connector.Received ( )
                       .MoveUpAsync ( ) ;
    }

    [ TestMethod ]
    public async Task MoveDownAsync_ForInvoked_CallsConnector ( )
    {
        var connector = Substitute.For< IDeskConnector > ( ) ;
        connector.MoveDownAsync ( ).Returns ( Task.FromResult ( true ) ) ;
        using var sut = new Desk ( connector ) ;

        await sut.MoveDownAsync ( ) ;

        await connector.Received ( )
                       .MoveDownAsync ( ) ;
    }

    [ TestMethod ]
    public async Task MoveStopAsync_ForInvoked_CallsConnector ( )
    {
        var connector = Substitute.For< IDeskConnector > ( ) ;
        connector.MoveStopAsync ( ).Returns ( Task.FromResult ( true ) ) ;
        using var sut = new Desk ( connector ) ;

        await sut.MoveStopAsync ( ) ;

        await connector.Received ( )
                       .MoveStopAsync ( ) ;
    }

    [ TestMethod ]
    public void Dispose_ForInvoked_CallsDeskConnector ( )
    {
        var connector = Substitute.For< IDeskConnector > ( ) ;
        var sut = new Desk ( connector ) ;

        sut.Dispose ( ) ;

        connector.Received ( )
                 .Dispose ( ) ;
    }

    [ TestMethod ]
    public void DeviceName_ForInvoked_DeviceName ( )
    {
        var connector = Substitute.For< IDeskConnector > ( ) ;
        connector.DeviceName.Returns ( "Device" ) ;
        using var sut = new Desk ( connector ) ;

        sut.DeviceName
           .Should ( )
           .Be ( "Device" ) ;
    }

    [ TestMethod ]
    public void BluetoothAddress_ForInvoked_BluetoothAddress ( )
    {
        var connector = Substitute.For< IDeskConnector > ( ) ;
        connector.BluetoothAddress.Returns ( 123UL ) ;
        using var sut = new Desk ( connector ) ;

        sut.BluetoothAddress
           .Should ( )
           .Be ( 123UL ) ;
    }

    [ TestMethod ]
    public void BluetoothAddressType_ForInvoked_BluetoothAddressType ( )
    {
        var connector = Substitute.For< IDeskConnector > ( ) ;
        connector.BluetoothAddressType.Returns ( "type" ) ;
        using var sut = new Desk ( connector ) ;

        sut.BluetoothAddressType
           .Should ( )
           .Be ( "type" ) ;
    }

    [ TestMethod ]
    public void ConnectionStatus_ForConnected_ReturnsConnected ( )
    {
        var connector = Substitute.For< IDeskConnector > ( ) ;
        connector.ConnectionStatus
                 .Returns ( Windows.Devices.Bluetooth.BluetoothConnectionStatus.Connected ) ;
        using var sut = new Desk ( connector ) ;

        sut.ConnectionStatus
           .Should ( )
           .Be ( Windows.Devices.Bluetooth.BluetoothConnectionStatus.Connected ) ;
    }

    [ TestMethod ]
    public void ConnectionStatus_ForDisconnected_ReturnsDisconnected ( )
    {
        var connector = Substitute.For< IDeskConnector > ( ) ;
        connector.ConnectionStatus
                 .Returns ( Windows.Devices.Bluetooth.BluetoothConnectionStatus.Disconnected ) ;
        using var sut = new Desk ( connector ) ;

        sut.ConnectionStatus
           .Should ( )
           .Be ( Windows.Devices.Bluetooth.BluetoothConnectionStatus.Disconnected ) ;
    }
}
