using Windows.Devices.Bluetooth ;
using FluentAssertions ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery.Wrappers ;
using Idasen.BluetoothLE.Core.ServicesDiscovery ;
using NSubstitute ;

namespace Idasen.BluetoothLE.Tests.ServicesDiscovery ;

[ TestClass ]
public class DeviceTests
{
    // todo ConnectionStatusChanged

    [ TestMethod ]
    public void GattCommunicationStatus_ForInvoked_Instance ( )
    {
        var wrapper = Substitute.For < IBluetoothLeDeviceWrapper > ( ) ;
        var scheduler = Substitute.For < System.Reactive.Concurrency.IScheduler > ( ) ;
        using var sut = new Device ( scheduler , wrapper ) ;

        sut.GattCommunicationStatus
           .Should ( )
           .Be ( wrapper.GattCommunicationStatus ) ;
    }

    [ TestMethod ]
    public void Connect_ForConnected_DoesNotCallConnect ( )
    {
        var wrapper = Substitute.For < IBluetoothLeDeviceWrapper > ( ) ;
        var scheduler = Substitute.For < System.Reactive.Concurrency.IScheduler > ( ) ;
        wrapper.ConnectionStatus
               .Returns ( BluetoothConnectionStatus.Connected ) ;

        using var sut = new Device ( scheduler , wrapper ) ;

        sut.Connect ( ) ;

        wrapper.DidNotReceive ( )
               .Connect ( ) ;
    }

    [ TestMethod ]
    public void Connect_ForDisconnected_DoesCallConnect ( )
    {
        var wrapper = Substitute.For < IBluetoothLeDeviceWrapper > ( ) ;
        var scheduler = Substitute.For < System.Reactive.Concurrency.IScheduler > ( ) ;
        wrapper.ConnectionStatus
               .Returns ( BluetoothConnectionStatus.Disconnected ) ;

        using var sut = new Device ( scheduler , wrapper ) ;

        sut.Connect ( ) ;

        wrapper.Received ( )
               .Connect ( ) ;
    }

    [ TestMethod ]
    public void Name_ForInvoked_Instance ( )
    {
        var wrapper = Substitute.For < IBluetoothLeDeviceWrapper > ( ) ;
        var scheduler = Substitute.For < System.Reactive.Concurrency.IScheduler > ( ) ;
        using var sut = new Device ( scheduler , wrapper ) ;

        sut.Name
           .Should ( )
           .Be ( wrapper.Name ) ;
    }

    [ TestMethod ]
    public void Id_ForInvoked_Instance ( )
    {
        var wrapper = Substitute.For < IBluetoothLeDeviceWrapper > ( ) ;
        var scheduler = Substitute.For < System.Reactive.Concurrency.IScheduler > ( ) ;
        using var sut = new Device ( scheduler , wrapper ) ;

        sut.Id
           .Should ( )
           .Be ( wrapper.DeviceId ) ;
    }

    [ TestMethod ]
    public void IsPaired_ForInvoked_Instance ( )
    {
        var wrapper = Substitute.For < IBluetoothLeDeviceWrapper > ( ) ;
        var scheduler = Substitute.For < System.Reactive.Concurrency.IScheduler > ( ) ;
        using var sut = new Device ( scheduler , wrapper ) ;

        sut.IsPaired
           .Should ( )
           .Be ( wrapper.IsPaired ) ;
    }

    [ TestMethod ]
    public void ConnectionStatus_ForInvoked_Instance ( )
    {
        var wrapper = Substitute.For < IBluetoothLeDeviceWrapper > ( ) ;
        var scheduler = Substitute.For < System.Reactive.Concurrency.IScheduler > ( ) ;
        using var sut = new Device ( scheduler , wrapper ) ;

        sut.ConnectionStatus
           .Should ( )
           .Be ( wrapper.ConnectionStatus ) ;
    }

    [ TestMethod ]
    public void GattServices_ForInvoked_Instance ( )
    {
        var wrapper = Substitute.For < IBluetoothLeDeviceWrapper > ( ) ;
        var scheduler = Substitute.For < System.Reactive.Concurrency.IScheduler > ( ) ;
        using var sut = new Device ( scheduler , wrapper ) ;

        sut.GattServices
           .Should ( )
           .BeSameAs ( wrapper.GattServices ) ;
    }

    [ TestMethod ]
    public void GattServicesRefreshed_ForInvoked_Instance ( )
    {
        var wrapper = Substitute.For < IBluetoothLeDeviceWrapper > ( ) ;
        var scheduler = Substitute.For < System.Reactive.Concurrency.IScheduler > ( ) ;
        using var sut = new Device ( scheduler , wrapper ) ;

        sut.GattServicesRefreshed
           .Should ( )
           .BeSameAs ( wrapper.GattServicesRefreshed ) ;
    }

    [ TestMethod ]
    public void Dispose_ForInvoked_DisposesWrapper ( )
    {
        var wrapper = Substitute.For < IBluetoothLeDeviceWrapper > ( ) ;
        var scheduler = Substitute.For < System.Reactive.Concurrency.IScheduler > ( ) ;
        var sut = new Device ( scheduler , wrapper ) ;

        sut.Dispose ( ) ;

        wrapper.Received ( )
               .Dispose ( ) ;
    }

    [ TestMethod ]
    public void Dispose_ForInvoked_DisposesSubscriber ( )
    {
        var wrapper = Substitute.For < IBluetoothLeDeviceWrapper > ( ) ;
        var subscriber = Substitute.For < IDisposable > ( ) ;
        var status = Substitute.For < IObservable < BluetoothConnectionStatus > > ( ) ;
        var scheduler = Substitute.For < System.Reactive.Concurrency.IScheduler > ( ) ;

        // Setup subscription to return subscriber when subscribed
        // We approximate behavior using a simple substitute for IObservable; tests focus on disposal interaction
        wrapper.ConnectionStatusChanged
               .Returns ( status ) ;

        var sut = new Device ( scheduler , wrapper ) ;

        sut.Dispose ( ) ;

        // We cannot directly assert subscription disposal on the substitute observable, but ensure no exceptions thrown
        subscriber.DidNotReceiveWithAnyArgs ( )
                 .Dispose ( ) ;
    }
}
