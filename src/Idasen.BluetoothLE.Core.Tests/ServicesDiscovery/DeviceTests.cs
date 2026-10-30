using Windows.Devices.Bluetooth ;
using FluentAssertions ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery.Wrappers ;
using Idasen.BluetoothLE.Core.ServicesDiscovery ;
using NSubstitute ;

namespace Idasen.BluetoothLE.Core.Tests.ServicesDiscovery ;

[ TestClass ]
public class DeviceTests
{
    [ TestMethod ]
    public void ConnectionStatusChanged_ForInvoked_Instance ( )
    {
        var wrapper = Substitute.For< IBluetoothLeDeviceWrapper > ( ) ;
        var scheduler = Substitute.For< System.Reactive.Concurrency.IScheduler > ( ) ;
        using var sut = new Device ( scheduler , wrapper ) ;

        sut.ConnectionStatusChanged
           .Should ( )
           .Be ( wrapper.ConnectionStatusChanged ) ;
    }

    [ TestMethod ]
    public void GattCommunicationStatus_ForInvoked_Instance ( )
    {
        var wrapper = Substitute.For< IBluetoothLeDeviceWrapper > ( ) ;
        var scheduler = Substitute.For< System.Reactive.Concurrency.IScheduler > ( ) ;
        using var sut = new Device ( scheduler ,
                                     wrapper ) ;

        sut.GattCommunicationStatus
           .Should ( )
           .Be ( wrapper.GattCommunicationStatus ) ;
    }

    [ TestMethod ]
    public void Connect_ForConnected_DoesNotCallConnect ( )
    {
        var wrapper = Substitute.For< IBluetoothLeDeviceWrapper > ( ) ;
        wrapper.ConnectionStatus
               .Returns ( BluetoothConnectionStatus.Connected ) ;

        var scheduler = Substitute.For< System.Reactive.Concurrency.IScheduler > ( ) ;
        using var sut = new Device ( scheduler ,
                                     wrapper ) ;

        sut.Connect ( ) ;

        wrapper.DidNotReceive ( )
               .Connect ( ) ;
    }

    [ TestMethod ]
    public void Connect_ForDisconnected_DoesCallConnect ( )
    {
        var wrapper = Substitute.For< IBluetoothLeDeviceWrapper > ( ) ;
        wrapper.ConnectionStatus
               .Returns ( BluetoothConnectionStatus.Disconnected ) ;

        var scheduler = Substitute.For< System.Reactive.Concurrency.IScheduler > ( ) ;
        using var sut = new Device ( scheduler ,
                                     wrapper ) ;

        sut.Connect ( ) ;

        wrapper.Received ( )
               .Connect ( ) ;
    }

    [ TestMethod ]
    public void Name_ForInvoked_Instance ( )
    {
        var wrapper = Substitute.For< IBluetoothLeDeviceWrapper > ( ) ;
        var scheduler = Substitute.For< System.Reactive.Concurrency.IScheduler > ( ) ;
        using var sut = new Device ( scheduler ,
                                     wrapper ) ;

        sut.Name
           .Should ( )
           .Be ( wrapper.Name ) ;
    }

    [ TestMethod ]
    public void DeviceId_ForInvoked_Instance ( )
    {
        var wrapper = Substitute.For< IBluetoothLeDeviceWrapper > ( ) ;
        var scheduler = Substitute.For< System.Reactive.Concurrency.IScheduler > ( ) ;
        using var sut = new Device ( scheduler , wrapper ) ;

        sut.Id
           .Should ( )
           .Be ( wrapper.DeviceId ) ;
    }

    [ TestMethod ]
    public void IsPaired_ForInvoked_Instance ( )
    {
        var wrapper = Substitute.For< IBluetoothLeDeviceWrapper > ( ) ;
        var scheduler = Substitute.For< System.Reactive.Concurrency.IScheduler > ( ) ;
        using var sut = new Device ( scheduler ,
                                     wrapper ) ;

        sut.IsPaired
           .Should ( )
           .Be ( wrapper.IsPaired ) ;
    }

    [ TestMethod ]
    public void ConnectionStatus_ForInvoked_Instance ( )
    {
        var wrapper = Substitute.For< IBluetoothLeDeviceWrapper > ( ) ;
        var scheduler = Substitute.For< System.Reactive.Concurrency.IScheduler > ( ) ;
        using var sut = new Device ( scheduler ,
                                     wrapper ) ;

        sut.ConnectionStatus
           .Should ( )
           .Be ( wrapper.ConnectionStatus ) ;
    }

    [ TestMethod ]
    public void GattServices_ForInvoked_Instance ( )
    {
        var wrapper = Substitute.For< IBluetoothLeDeviceWrapper > ( ) ;
        var scheduler = Substitute.For< System.Reactive.Concurrency.IScheduler > ( ) ;
        using var sut = new Device ( scheduler ,
                                     wrapper ) ;

        sut.GattServices
           .Should ( )
           .BeSameAs ( wrapper.GattServices ) ;
    }

    [ TestMethod ]
    public void GattServicesRefreshed_ForInvoked_Instance ( )
    {
        var wrapper = Substitute.For< IBluetoothLeDeviceWrapper > ( ) ;
        var scheduler = Substitute.For< System.Reactive.Concurrency.IScheduler > ( ) ;
        using var sut = new Device ( scheduler ,
                                     wrapper ) ;

        sut.GattServicesRefreshed
           .Should ( )
           .BeSameAs ( wrapper.GattServicesRefreshed ) ;
    }

    [ TestMethod ]
    public void Dispose_ForInvoked_DisposesWrapper ( )
    {
        var wrapper = Substitute.For< IBluetoothLeDeviceWrapper > ( ) ;
        var scheduler = Substitute.For< System.Reactive.Concurrency.IScheduler > ( ) ;
        var sut = new Device ( scheduler ,
                               wrapper ) ;

        sut.Dispose ( ) ;

        wrapper.Received ( )
               .Dispose ( ) ;
    }

    [ TestMethod ]
    public void Dispose_ForInvoked_DisposesSubscriber ( )
    {
        var wrapper = Substitute.For< IBluetoothLeDeviceWrapper > ( ) ;
        var subscriber = Substitute.For< IDisposable > ( ) ;
        var status = Substitute.For< IObservable< BluetoothConnectionStatus > > ( ) ;

        status.Subscribe ( Arg.Any< IObserver<BluetoothConnectionStatus> > ( ) )
              .ReturnsForAnyArgs ( subscriber ) ;

        wrapper.ConnectionStatusChanged
               .Returns ( status ) ;

        var scheduler = System.Reactive.Concurrency.Scheduler.Immediate ;
        var sut = new Lazy< Device > ( ( ) => new Device ( scheduler , wrapper ) ) ;

        sut.Value
           .Dispose ( ) ;

        subscriber.Received ( )
                  .Dispose ( ) ;
    }

    [ TestMethod ]
    public void BluetoothAddress_ForInvoked_BluetoothAddress ( )
    {
        var wrapper = Substitute.For< IBluetoothLeDeviceWrapper > ( ) ;
        wrapper.BluetoothAddress
               .Returns ( 123UL ) ;

        var scheduler = Substitute.For< System.Reactive.Concurrency.IScheduler > ( ) ;
        using var sut = new Device ( scheduler ,
                                     wrapper ) ;

        sut.BluetoothAddress
           .Should ( )
           .Be ( 123UL ) ;
    }

    [ TestMethod ]
    public void BluetoothAddressType_ForInvoked_BluetoothAddressType ( )
    {
        var wrapper = Substitute.For< IBluetoothLeDeviceWrapper > ( ) ;
        wrapper.BluetoothAddressType
               .Returns ( "type" ) ;

        var scheduler = Substitute.For< System.Reactive.Concurrency.IScheduler > ( ) ;
        using var sut = new Device ( scheduler ,
                                     wrapper ) ;

        sut.BluetoothAddressType
           .Should ( )
           .Be ( "type" ) ;
    }
}
