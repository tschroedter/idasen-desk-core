using System.Reactive ;
using System.Reactive.Subjects ;
using Windows.Devices.Bluetooth ;
using Windows.Devices.Bluetooth.GenericAttributeProfile ;
using FluentAssertions ;
using Idasen.BluetoothLE.Common.Tests ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery.Wrappers ;
using Idasen.BluetoothLE.Core.ServicesDiscovery ;
using NSubstitute ;
using Serilog ;
using Serilog.Core ;

namespace Idasen.BluetoothLE.Core.Tests.ServicesDiscovery ;

[ TestClass ]
public class GattServicesProviderTests
{
    private ILogger                              _logger    = null! ;
    private IGattServices                        _services  = null! ;
    private ISubject < GattCommunicationStatus > _refreshed = null! ;
    private IBluetoothLeDeviceWrapper            _device    = null! ;

    [ TestInitialize ]
    public void Setup ( )
    {
        _logger    = Logger.None ;
        _services  = Substitute.For < IGattServices > ( ) ;
        _refreshed = Substitute.For < ISubject < GattCommunicationStatus > > ( ) ;
        _device    = Substitute.For < IBluetoothLeDeviceWrapper > ( ) ;
    }

    private GattServicesProvider CreateSut ( )
    {
        return new GattServicesProvider ( _logger ,
                                          _services ,
                                          _refreshed ,
                                          _device ) ;
    }
    [ TestMethod ]
    public void Constructor_ForLoggerNull_Throws ( )
    {
        // ReSharper disable once ObjectCreationAsStatement
        var action = ( ) =>
                     {
                         new GattServicesProvider ( null! ,
                                                    _services ,
                                                    _refreshed ,
                                                    _device ) ;
                     } ;

        action.Should ( )
              .Throw < ArgumentNullException > ( )
              .WithParameter ( "logger" ) ;
    }

    [ TestMethod ]
    public void Constructor_ForServicesNull_Throws ( )
    {
        // ReSharper disable once ObjectCreationAsStatement
        var action = ( ) =>
                     {
                         new GattServicesProvider ( _logger ,
                                                    null! ,
                                                    _refreshed ,
                                                    _device ) ;
                     } ;

        action.Should ( )
              .Throw < ArgumentNullException > ( )
              .WithParameter ( "services" ) ;
    }

    [ TestMethod ]
    public void Constructor_ForRefreshedNull_Throws ( )
    {
        // ReSharper disable once ObjectCreationAsStatement
        var action = ( ) =>
                     {
                         new GattServicesProvider ( _logger ,
                                                    _services ,
                                                    null! ,
                                                    _device ) ;
                     } ;

        action.Should ( )
              .Throw < ArgumentNullException > ( )
              .WithParameter ( "refreshed" ) ;
    }

    [ TestMethod ]
    public void Constructor_ForDeviceNull_Throws ( )
    {
        // ReSharper disable once ObjectCreationAsStatement
        var action = ( ) =>
                     {
                         new GattServicesProvider ( _logger ,
                                                    _services ,
                                                    _refreshed ,
                                                    null! ) ;
                     } ;

        action.Should ( )
              .Throw < ArgumentNullException > ( )
              .WithParameter ( "device" ) ;
    }

    [ TestMethod ]
    public void GattCommunicationStatus_ForGattResultIsNull_Unreachable ( )
    {
        var sut = CreateSut ( ) ;

        sut.GattCommunicationStatus
           .Should ( )
           .Be ( GattCommunicationStatus.Unreachable ) ;
    }

    [ TestMethod ]
    public async Task Refresh_ForDisconnected_SetsGattCommunicationStatusUnreachable ( )
    {
        var sut = CreateSut ( ) ;

        _device.ConnectionStatus
               .Returns ( BluetoothConnectionStatus.Disconnected ) ;

        await sut.Refresh ( ) ;

        sut.GattCommunicationStatus
           .Should ( )
           .Be ( GattCommunicationStatus.Unreachable ) ;
    }

    [ TestMethod ]
    public async Task GattCommunicationStatus_ForConnectedAndServicesAvailable_Success ( )
    {
        var sut    = CreateSut ( ) ;
        var result = Substitute.For < IGattDeviceServicesResultWrapper > ( ) ;

        result.Status
              .Returns ( GattCommunicationStatus.Success ) ;

        _device.ConnectionStatus
               .Returns ( BluetoothConnectionStatus.Connected ) ;

        _device.GetGattServicesAsync ( )
               .Returns ( result ) ;

        await sut.Refresh ( ) ;

        sut.GattCommunicationStatus
           .Should ( )
           .Be ( result.Status ) ;
    }

    [ TestMethod ]
    public async Task Refresh_ForDisconnected_Notifies ( )
    {
        var sut = CreateSut ( ) ;

        _device.ConnectionStatus
               .Returns ( BluetoothConnectionStatus.Disconnected ) ;

        await sut.Refresh ( ) ;

        _refreshed.Received ( )
                  .OnNext ( GattCommunicationStatus.Unreachable ) ;
    }

    [ TestMethod ]
    public async Task Refresh_ForConnected_SetsGattCommunicationStatusUnreachable ( )
    {
        var sut    = CreateSut ( ) ;
        var result = Substitute.For < IGattDeviceServicesResultWrapper > ( ) ;

        result.Status
              .Returns ( GattCommunicationStatus.Unreachable ) ;

        _device.ConnectionStatus
               .Returns ( BluetoothConnectionStatus.Connected ) ;

        _device.GetGattServicesAsync ( )
               .Returns ( Task.FromResult ( result ) ) ;

        await sut.Refresh ( ) ;

        sut.GattCommunicationStatus
           .Should ( )
           .Be ( GattCommunicationStatus.Unreachable ) ;
    }

    [ TestMethod ]
    public async Task Refresh_ForInvoked_ClearsServices ( )
    {
        var sut = CreateSut ( ) ;

        await sut.Refresh ( ) ;

        _services.Received ( )
                 .Clear ( ) ;
    }

    [ TestMethod ]
    public async Task Refresh_ForConnected_Notifies ( )
    {
        var sut      = CreateSut ( ) ;
        var result   = Substitute.For < IGattDeviceServicesResultWrapper > ( ) ;
        var expected = GattCommunicationStatus.ProtocolError ;

        result.Status
              .Returns ( expected ) ;

        _device.ConnectionStatus
               .Returns ( BluetoothConnectionStatus.Connected ) ;

        _device.GetGattServicesAsync ( )
               .Returns ( Task.FromResult ( result ) ) ;

        await sut.Refresh ( ) ;

        _refreshed.Received ( )
                  .OnNext ( expected ) ;
    }

    [ TestMethod ]
    public async Task Refresh_ForConnectedAndCharacteristicsSuccess_AddsService ( )
    {
        var sut             = CreateSut ( ) ;
        var result          = Substitute.For < IGattDeviceServicesResultWrapper > ( ) ;
        var service         = Substitute.For < IGattDeviceServiceWrapper > ( ) ;
        var characteristics = Substitute.For < IGattCharacteristicsResultWrapper > ( ) ;

        result.Status
              .Returns ( GattCommunicationStatus.Success ) ;

        result.Services
              .Returns ( [service] ) ;

        _device.ConnectionStatus
               .Returns ( BluetoothConnectionStatus.Connected ) ;

        _device.GetGattServicesAsync ( )
               .Returns ( Task.FromResult ( result ) ) ;

        characteristics.Status
                       .Returns ( GattCommunicationStatus.Success ) ;

        service.GetCharacteristicsAsync ( )
               .Returns ( characteristics ) ;

        await sut.Refresh ( ) ;

        _services [ service ]
           .Should ( )
           .Be ( characteristics ) ;
    }

    [ TestMethod ]
    public async Task Refresh_ForConnectedAndCharacteristicsUnreachable_DoesNotAddService ( )
    {
        var sut             = CreateSut ( ) ;
        var result          = Substitute.For < IGattDeviceServicesResultWrapper > ( ) ;
        var service         = Substitute.For < IGattDeviceServiceWrapper > ( ) ;
        var characteristics = Substitute.For < IGattCharacteristicsResultWrapper > ( ) ;

        result.Status
              .Returns ( GattCommunicationStatus.Success ) ;

        result.Services
              .Returns ( [service] ) ;

        _device.ConnectionStatus
               .Returns ( BluetoothConnectionStatus.Connected ) ;

        _device.GetGattServicesAsync ( )
               .Returns ( Task.FromResult ( result ) ) ;

        characteristics.Status
                       .Returns ( GattCommunicationStatus.Unreachable ) ;

        service.GetCharacteristicsAsync ( )
               .Returns ( characteristics ) ;

        await sut.Refresh ( ) ;

        _services [ service ]
           .Should ( )
           .NotBe ( characteristics ) ;
    }

    [ TestMethod ]
    public void Refreshed_ForInvoked_CallsSubject ( )
    {
        var sut = CreateSut ( ) ;

        using var disposable = sut.Refreshed
                                  .Subscribe ( DoNothing ) ;

        _refreshed.Received ( )
                  .Subscribe ( Arg.Any < AnonymousObserver < GattCommunicationStatus > > ( ) ) ;
    }

    [ TestMethod ]
    public void Services_ForInvoked_CallsServices ( )
    {
        var sut = CreateSut ( ) ;

        var readOnlyDict = new Dictionary < IGattDeviceServiceWrapper , IGattCharacteristicsResultWrapper > ( )
                          .AsReadOnly ( ) ;

        _services.ReadOnlyDictionary
                 .Returns ( readOnlyDict ) ;

        sut.Services
           .Should ( )
           .BeEquivalentTo ( readOnlyDict ) ;
    }

    [ TestMethod ]
    public void Dispose_ForInvoked_CallsServices ( )
    {
        var sut = CreateSut ( ) ;

        sut.Dispose ( ) ;

        _services.Received ( )
                 .Dispose ( ) ;
    }

    private void DoNothing ( GattCommunicationStatus status )
    {
    }

    // todo integration tests
}
