using System.Reactive.Subjects ;
using Windows.Devices.Bluetooth ;
using Windows.Devices.Bluetooth.GenericAttributeProfile ;
using FluentAssertions ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery.Wrappers ;
using Idasen.BluetoothLE.Core.ServicesDiscovery ;
using NSubstitute ;
using Serilog.Core ;

namespace Idasen.BluetoothLE.Tests.ServicesDiscovery ;

[ TestClass ]
public class GattServicesProviderTests
{
    private static GattServicesProvider CreateSut (
        IGattServices ?                        services  = null ,
        ISubject < GattCommunicationStatus > ? refreshed = null ,
        IBluetoothLeDeviceWrapper ?            device    = null )
    {
        return new GattServicesProvider (
            Logger.None ,
            services  ?? Substitute.For < IGattServices > ( ) ,
            refreshed ?? Substitute.For < ISubject < GattCommunicationStatus > > ( ) ,
            device    ?? Substitute.For < IBluetoothLeDeviceWrapper > ( ) ) ;
    }
    [ TestMethod ]
    public void Constructor_ForLoggerNull_Throws ( )
    {
        var action = ( ) => new GattServicesProvider (
            null! ,
            Substitute.For < IGattServices > ( ) ,
            Substitute.For < ISubject < GattCommunicationStatus > > ( ) ,
            Substitute.For < IBluetoothLeDeviceWrapper > ( ) ) ;

        action.Should ( )
              .Throw < ArgumentNullException > ( )
              .WithParameterName ( "logger" ) ;
    }

    [ TestMethod ]
    public void Constructor_ForServicesNull_Throws ( )
    {
        var action = ( ) => new GattServicesProvider (
            Logger.None ,
            null! ,
            Substitute.For < ISubject < GattCommunicationStatus > > ( ) ,
            Substitute.For < IBluetoothLeDeviceWrapper > ( ) ) ;

        action.Should ( )
              .Throw < ArgumentNullException > ( )
              .WithParameterName ( "services" ) ;
    }

    [ TestMethod ]
    public void Constructor_ForRefreshedNull_Throws ( )
    {
        var action = ( ) => new GattServicesProvider (
            Logger.None ,
            Substitute.For < IGattServices > ( ) ,
            null! ,
            Substitute.For < IBluetoothLeDeviceWrapper > ( ) ) ;

        action.Should ( )
              .Throw < ArgumentNullException > ( )
              .WithParameterName ( "refreshed" ) ;
    }

    [ TestMethod ]
    public void Constructor_ForDeviceNull_Throws ( )
    {
        var action = ( ) => new GattServicesProvider (
            Logger.None ,
            Substitute.For < IGattServices > ( ) ,
            Substitute.For < ISubject < GattCommunicationStatus > > ( ) ,
            null! ) ;

        action.Should ( )
              .Throw < ArgumentNullException > ( )
              .WithParameterName ( "device" ) ;
    }

    [ TestMethod ]
    public void GattCommunicationStatus_ForGattResultIsNull_Unreachable ( )
    {
        using var sut = CreateSut ( ) ;

        sut.GattCommunicationStatus
           .Should ( )
           .Be ( GattCommunicationStatus.Unreachable ) ;
    }

    [ TestMethod ]
    public async Task Refresh_ForDisconnected_SetsGattCommunicationStatusUnreachable ( )
    {
        var device = Substitute.For < IBluetoothLeDeviceWrapper > ( ) ;
        device.ConnectionStatus
              .Returns ( BluetoothConnectionStatus.Disconnected ) ;
        using var sut = CreateSut ( device : device ) ;

        await sut.Refresh ( ) ;

        sut.GattCommunicationStatus
           .Should ( )
           .Be ( GattCommunicationStatus.Unreachable ) ;
    }

    [ TestMethod ]
    public async Task GattCommunicationStatus_ForConnectedAndServicesAvailable_Success ( )
    {
        var device        = Substitute.For < IBluetoothLeDeviceWrapper > ( ) ;
        var resultWrapper = Substitute.For < IGattDeviceServicesResultWrapper > ( ) ;

        resultWrapper.Status
                     .Returns ( GattCommunicationStatus.Success ) ;

        device.ConnectionStatus
              .Returns ( BluetoothConnectionStatus.Connected ) ;

        device.GetGattServicesAsync ( )
              .Returns ( resultWrapper ) ;

        using var sut = CreateSut ( device : device ) ;

        await sut.Refresh ( ) ;

        sut.GattCommunicationStatus
           .Should ( )
           .Be ( resultWrapper.Status ) ;
    }

    [ TestMethod ]
    public async Task Refresh_ForDisconnected_Notifies ( )
    {
        var device    = Substitute.For < IBluetoothLeDeviceWrapper > ( ) ;
        var refreshed = Substitute.For < ISubject < GattCommunicationStatus > > ( ) ;

        device.ConnectionStatus
              .Returns ( BluetoothConnectionStatus.Disconnected ) ;

        using var sut = CreateSut ( refreshed : refreshed ,
                                    device : device ) ;

        await sut.Refresh ( ) ;

        refreshed.Received ( )
                 .OnNext ( GattCommunicationStatus.Unreachable ) ;
    }

    [ TestMethod ]
    public async Task Refresh_ForConnected_SetsGattCommunicationStatusUnreachable ( )
    {
        var device = Substitute.For < IBluetoothLeDeviceWrapper > ( ) ;
        var result = Substitute.For < IGattDeviceServicesResultWrapper > ( ) ;

        result.Status
              .Returns ( GattCommunicationStatus.Unreachable ) ;

        device.ConnectionStatus
              .Returns ( BluetoothConnectionStatus.Connected ) ;

        device.GetGattServicesAsync ( )
              .Returns ( Task.FromResult ( result ) ) ;

        using var sut = CreateSut ( device : device ) ;

        await sut.Refresh ( ) ;

        sut.GattCommunicationStatus
           .Should ( )
           .Be ( GattCommunicationStatus.Unreachable ) ;
    }

    [ TestMethod ]
    public async Task Refresh_ForInvoked_ClearsServices ( )
    {
        var services = Substitute.For < IGattServices > ( ) ;
        using var sut = CreateSut ( services : services ) ;

        await sut.Refresh ( ) ;

        services.Received ( )
                .Clear ( ) ;
    }

    [ TestMethod ]
    public async Task Refresh_ForConnected_Notifies ( )
    {
        var device    = Substitute.For < IBluetoothLeDeviceWrapper > ( ) ;
        var refreshed = Substitute.For < ISubject < GattCommunicationStatus > > ( ) ;
        var result    = Substitute.For < IGattDeviceServicesResultWrapper > ( ) ;
        var expected = GattCommunicationStatus.ProtocolError ;

        result.Status
              .Returns ( expected ) ;

        device.ConnectionStatus
              .Returns ( BluetoothConnectionStatus.Connected ) ;

        device.GetGattServicesAsync ( )
              .Returns ( Task.FromResult ( result ) ) ;

        using var sut = CreateSut ( refreshed : refreshed ,
                                    device : device ) ;

        await sut.Refresh ( ) ;

        refreshed.Received ( )
                 .OnNext ( expected ) ;
    }

    [ TestMethod ]
    public async Task Refresh_ForConnectedAndCharacteristicsSuccess_AddsService ( )
    {
        var device          = Substitute.For < IBluetoothLeDeviceWrapper > ( ) ;
        var refreshed       = Substitute.For < ISubject < GattCommunicationStatus > > ( ) ;
        var services        = Substitute.For < IGattServices > ( ) ;
        var result          = Substitute.For < IGattDeviceServicesResultWrapper > ( ) ;
        var service         = Substitute.For < IGattDeviceServiceWrapper > ( ) ;
        var characteristics = Substitute.For < IGattCharacteristicsResultWrapper > ( ) ;

        result.Status
              .Returns ( GattCommunicationStatus.Success ) ;

        result.Services
              .Returns ( [service] ) ;

        device.ConnectionStatus
              .Returns ( BluetoothConnectionStatus.Connected ) ;

        device.GetGattServicesAsync ( )
              .Returns ( Task.FromResult ( result ) ) ;

        characteristics.Status
                       .Returns ( GattCommunicationStatus.Success ) ;

        service.GetCharacteristicsAsync ( )
               .Returns ( characteristics ) ;

        using var sut = CreateSut ( services : services ,
                                    refreshed : refreshed ,
                                    device : device ) ;

        await sut.Refresh ( ) ;

        services [ service ]
           .Should ( )
           .Be ( characteristics ) ;
    }

    [ TestMethod ]
    public async Task Refresh_ForConnectedAndCharacteristicsUnreachable_DoesNotAddService ( )
    {
        var device          = Substitute.For < IBluetoothLeDeviceWrapper > ( ) ;
        var services        = Substitute.For < IGattServices > ( ) ;
        var result          = Substitute.For < IGattDeviceServicesResultWrapper > ( ) ;
        var service         = Substitute.For < IGattDeviceServiceWrapper > ( ) ;
        var characteristics = Substitute.For < IGattCharacteristicsResultWrapper > ( ) ;

        result.Status
              .Returns ( GattCommunicationStatus.Success ) ;

        result.Services
              .Returns ( [service] ) ;

        device.ConnectionStatus
              .Returns ( BluetoothConnectionStatus.Connected ) ;

        device.GetGattServicesAsync ( )
              .Returns ( Task.FromResult ( result ) ) ;

        characteristics.Status
                       .Returns ( GattCommunicationStatus.Unreachable ) ;

        service.GetCharacteristicsAsync ( )
               .Returns ( characteristics ) ;

        using var sut = CreateSut ( services : services ,
                                    device : device ) ;

        await sut.Refresh ( ) ;

        services [ service ]
           .Should ( )
           .NotBe ( characteristics ) ;
    }

    // todo integration tests
}
