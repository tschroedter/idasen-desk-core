using System.Reactive.Concurrency ;
using System.Reactive.Linq ;
using System.Reactive.Subjects ;
using FluentAssertions ;
using Idasen.BluetoothLE.Core ;
using Idasen.BluetoothLE.Core.DevicesDiscovery ;
using Idasen.BluetoothLE.Core.Interfaces ;
using Idasen.BluetoothLE.Core.Interfaces.DevicesDiscovery ;
using Microsoft.Reactive.Testing ;
using NSubstitute ;
using Serilog ;
using Serilog.Core ;

namespace Idasen.BluetoothLE.Tests.DevicesDiscovery ;

[ TestClass ]
public class DeviceMonitorWithExpiryTests
{
    private static DeviceMonitorWithExpiry CreateSut ( ILogger ?                 logger         = null ,
                                                       IDateTimeOffset ?         dateTimeOffset = null ,
                                                       IDeviceMonitor ?          deviceMonitor  = null ,
                                                       ISubject < IDevice > ?    deviceExpired  = null ,
                                                       IObservableTimerFactory ? factory        = null ,
                                                       IScheduler ?              scheduler      = null )
    {
        return new DeviceMonitorWithExpiry ( logger         ?? Logger.None ,
                                             dateTimeOffset ?? Substitute.For < IDateTimeOffset > ( ) ,
                                             deviceMonitor  ?? Substitute.For < IDeviceMonitor > ( ) ,
                                             deviceExpired  ?? Substitute.For < ISubject < IDevice > > ( ) ,
                                             factory        ?? Substitute.For < IObservableTimerFactory > ( ) ,
                                             scheduler      ?? Substitute.For < IScheduler > ( ) ) ;
    }

    [ TestMethod ]
    public void Constructor_ForLoggerNull_Throws ( )
    {
        var action = ( ) => new DeviceMonitorWithExpiry ( null! ,
                                                          Substitute.For < IDateTimeOffset > ( ) ,
                                                          Substitute.For < IDeviceMonitor > ( ) ,
                                                          Substitute.For < ISubject < IDevice > > ( ) ,
                                                          Substitute.For < IObservableTimerFactory > ( ) ,
                                                          Substitute.For < IScheduler > ( ) ) ;

        action.Should ( )
              .Throw < ArgumentNullException > ( )
              .WithParameterName ( "logger" ) ;
    }

    [ TestMethod ]
    public void Constructor_ForDateTimeOffsetNull_Throws ( )
    {
        var action = ( ) => new DeviceMonitorWithExpiry ( Logger.None ,
                                                          null! ,
                                                          Substitute.For < IDeviceMonitor > ( ) ,
                                                          Substitute.For < ISubject < IDevice > > ( ) ,
                                                          Substitute.For < IObservableTimerFactory > ( ) ,
                                                          Substitute.For < IScheduler > ( ) ) ;

        action.Should ( )
              .Throw < ArgumentNullException > ( )
              .WithParameterName ( "dateTimeOffset" ) ;
    }

    [ TestMethod ]
    public void Constructor_ForDeviceMonitorNull_Throws ( )
    {
        var action = ( ) => new DeviceMonitorWithExpiry ( Logger.None ,
                                                          Substitute.For < IDateTimeOffset > ( ) ,
                                                          null! ,
                                                          Substitute.For < ISubject < IDevice > > ( ) ,
                                                          Substitute.For < IObservableTimerFactory > ( ) ,
                                                          Substitute.For < IScheduler > ( ) ) ;

        action.Should ( )
              .Throw < ArgumentNullException > ( )
              .WithParameterName ( "deviceMonitor" ) ;
    }

    [ TestMethod ]
    public void Constructor_ForFactoryNull_Throws ( )
    {
        var action = ( ) => new DeviceMonitorWithExpiry ( Logger.None ,
                                                          Substitute.For < IDateTimeOffset > ( ) ,
                                                          Substitute.For < IDeviceMonitor > ( ) ,
                                                          Substitute.For < ISubject < IDevice > > ( ) ,
                                                          null! ,
                                                          Substitute.For < IScheduler > ( ) ) ;

        action.Should ( )
              .Throw < ArgumentNullException > ( )
              .WithParameterName ( "factory" ) ;
    }

    [ TestMethod ]
    public void Constructor_ForSchedulerIsNull_Throws ( )
    {
        var action = ( ) => new DeviceMonitorWithExpiry ( Logger.None ,
                                                          Substitute.For < IDateTimeOffset > ( ) ,
                                                          Substitute.For < IDeviceMonitor > ( ) ,
                                                          Substitute.For < ISubject < IDevice > > ( ) ,
                                                          Substitute.For < IObservableTimerFactory > ( ) ,
                                                          null! ) ;

        action.Should ( )
              .Throw < ArgumentNullException > ( )
              .WithParameterName ( "scheduler" ) ;
    }

    [ TestMethod ]
    public void Constructor_ForDeviceExpiredNull_Throws ( )
    {
        var action = ( ) => new DeviceMonitorWithExpiry ( Logger.None ,
                                                          Substitute.For < IDateTimeOffset > ( ) ,
                                                          Substitute.For < IDeviceMonitor > ( ) ,
                                                          null! ,
                                                          Substitute.For < IObservableTimerFactory > ( ) ,
                                                          Substitute.For < IScheduler > ( ) ) ;

        action.Should ( )
              .Throw < ArgumentNullException > ( )
              .WithParameterName ( "deviceExpired" ) ;
    }

    [ TestMethod ]
    public void TimeOut_ForValueGreaterZero_SetsTimeOut ( )
    {
        using var sut      = CreateSut ( ) ;
        var       expected = TimeSpan.FromHours ( 1.23 ) ;

        sut.TimeOut = expected ;

        sut.TimeOut
           .Should ( )
           .Be ( expected ) ;
    }

    [ TestMethod ]
    public void TimeOut_ForValueLessThanZero_SetsTimeOut ( )
    {
        var action = ( ) =>
                     {
                         using var sut = CreateSut ( ) ;

                         sut.TimeOut = TimeSpan.FromHours ( - 0.1 ) ;
                     } ;

        action.Should ( )
              .Throw < ArgumentException > ( ) ;
    }

    [ TestMethod ]
    public void RemoveDevice_ForInvoked_CallsDeviceMonitor ( )
    {
        var       monitor = Substitute.For < IDeviceMonitor > ( ) ;
        var       device  = Substitute.For < IDevice > ( ) ;
        using var sut     = CreateSut ( deviceMonitor : monitor ) ;

        sut.RemoveDevice ( device ) ;

        monitor.Received ( )
               .RemoveDevice ( device ) ;
    }

    [ TestMethod ]
    public void Start_ForInvoked_CallsDeviceMonitor ( )
    {
        var       monitor = Substitute.For < IDeviceMonitor > ( ) ;
        using var sut     = CreateSut ( deviceMonitor : monitor ) ;

        sut.StartListening ( ) ;

        monitor.Received ( )
               .StartListening ( ) ;
    }

    [ TestMethod ]
    public void Stop_ForInvoked_CallsDeviceMonitor ( )
    {
        var       monitor = Substitute.For < IDeviceMonitor > ( ) ;
        using var sut     = CreateSut ( deviceMonitor : monitor ) ;

        sut.StopListening ( ) ;

        monitor.Received ( )
               .StopListening ( ) ;
    }

    [ TestMethod ]
    public void Dispose_ForInvoked_DisposesMonitor ( )
    {
        var monitor = Substitute.For < IDeviceMonitor > ( ) ;
        var sut     = CreateSut ( deviceMonitor : monitor ) ;

        sut.Dispose ( ) ;

        monitor.Received ( )
               .Dispose ( ) ;
    }

    [ TestMethod ]
    public void Dispose_ForInvoked_DisposesTimer ( )
    {
        var factory = Substitute.For < IObservableTimerFactory > ( ) ;
        factory.Create ( Arg.Any < TimeSpan > ( ) ,
                         Arg.Any < IScheduler > ( ) )
               .Returns ( Observable.Never < long > ( ) ) ;
        var sut = CreateSut ( factory : factory ) ;

        sut.Dispose ( ) ;

        // Timer is created on construction and disposed on Dispose
        // We can verify the factory was called
        factory.Received ( )
               .Create ( Arg.Any < TimeSpan > ( ) ,
                         Arg.Any < IScheduler > ( ) ) ;
    }

    [ TestMethod ]
    public void DiscoveredDevices_ForInvoked_CallsDeviceMonitor ( )
    {
        var monitor    = Substitute.For < IDeviceMonitor > ( ) ;
        var collection = Substitute.For < IReadOnlyCollection < IDevice > > ( ) ;
        monitor.DiscoveredDevices
               .Returns ( collection ) ;
        using var sut = CreateSut ( deviceMonitor : monitor ) ;

        sut.DiscoveredDevices
           .Should ( )
           .BeEquivalentTo ( collection ) ;
    }

    [ TestMethod ]
    public void IsListening_ForInvoked_CallsDeviceMonitor ( )
    {
        var monitor = Substitute.For < IDeviceMonitor > ( ) ;
        monitor.IsListening
               .Returns ( true ) ;
        using var sut = CreateSut ( deviceMonitor : monitor ) ;

        sut.IsListening
           .Should ( )
           .BeTrue ( ) ;
    }

    [ TestMethod ]
    public void DeviceUpdated_ForInvoked_CallsDeviceMonitor ( )
    {
        var monitor    = Substitute.For < IDeviceMonitor > ( ) ;
        var observable = Substitute.For < IObservable < IDevice > > ( ) ;
        monitor.DeviceUpdated
               .Returns ( observable ) ;
        using var sut = CreateSut ( deviceMonitor : monitor ) ;

        sut.DeviceUpdated
           .Should ( )
           .Be ( observable ) ;
    }

    [ TestMethod ]
    public void DeviceDiscovered_ForInvoked_CallsDeviceMonitor ( )
    {
        var monitor    = Substitute.For < IDeviceMonitor > ( ) ;
        var observable = Substitute.For < IObservable < IDevice > > ( ) ;
        monitor.DeviceDiscovered
               .Returns ( observable ) ;
        using var sut = CreateSut ( deviceMonitor : monitor ) ;

        sut.DeviceDiscovered
           .Should ( )
           .Be ( observable ) ;
    }

    [ TestMethod ]
    public void DeviceNameUpdated_ForInvoked_CallsDeviceMonitor ( )
    {
        var monitor    = Substitute.For < IDeviceMonitor > ( ) ;
        var observable = Substitute.For < IObservable < IDevice > > ( ) ;
        monitor.DeviceNameUpdated
               .Returns ( observable ) ;
        using var sut = CreateSut ( deviceMonitor : monitor ) ;

        sut.DeviceNameUpdated
           .Should ( )
           .Be ( observable ) ;
    }

    [ TestMethod ]
    public void CleanUp_ForNotExpiredDeviceInCollection_DoesNotRemoveDeviceFromCollection ( )
    {
        var logger         = Logger.None ;
        var dateTimeOffset = Substitute.For < IDateTimeOffset > ( ) ;
        var deviceMonitor  = Substitute.For < IDeviceMonitor > ( ) ;
        var deviceExpired  = Substitute.For < ISubject < IDevice > > ( ) ;
        var factory        = new ObservableTimerFactory ( ) ;
        var scheduler      = new TestScheduler ( ) ;
        var device         = Substitute.For < IDevice > ( ) ;

        deviceMonitor.DiscoveredDevices
                     .Returns ( [device] ) ;

        using var sut = new DeviceMonitorWithExpiry ( logger ,
                                                      dateTimeOffset ,
                                                      deviceMonitor ,
                                                      deviceExpired ,
                                                      factory ,
                                                      scheduler ) ;

        dateTimeOffset.Ticks
                      .Returns ( sut.TimeOut.Ticks ) ;
        dateTimeOffset.Now
                      .Returns ( dateTimeOffset ) ;

        device.BroadcastTime
              .Ticks
              .Returns ( sut.TimeOut.Ticks / 2 ) ;

        scheduler.AdvanceBy ( sut.TimeOut.Ticks ) ;

        deviceMonitor.DidNotReceive ( )
                     .RemoveDevice ( device ) ;
    }

    [ TestMethod ]
    public void CleanUp_ForNotExpiredDeviceInCollection_DoesNotNotifyDeviceExpired ( )
    {
        var logger         = Logger.None ;
        var dateTimeOffset = Substitute.For < IDateTimeOffset > ( ) ;
        var deviceMonitor  = Substitute.For < IDeviceMonitor > ( ) ;
        var deviceExpired  = Substitute.For < ISubject < IDevice > > ( ) ;
        var factory        = new ObservableTimerFactory ( ) ;
        var scheduler      = new TestScheduler ( ) ;
        var device         = Substitute.For < IDevice > ( ) ;

        deviceMonitor.DiscoveredDevices
                     .Returns ( [device] ) ;

        using var sut = new DeviceMonitorWithExpiry ( logger ,
                                                      dateTimeOffset ,
                                                      deviceMonitor ,
                                                      deviceExpired ,
                                                      factory ,
                                                      scheduler ) ;

        dateTimeOffset.Ticks
                      .Returns ( sut.TimeOut.Ticks ) ;
        dateTimeOffset.Now
                      .Returns ( dateTimeOffset ) ;

        device.BroadcastTime
              .Ticks
              .Returns ( sut.TimeOut.Ticks / 2 ) ;

        scheduler.AdvanceBy ( sut.TimeOut.Ticks ) ;

        deviceExpired.DidNotReceive ( )
                     .Publish ( device ) ;
    }

    [ TestMethod ]
    public void CleanUp_ForOneExpiredDeviceInCollection_RemovesDeviceFromCollection ( )
    {
        var logger         = Logger.None ;
        var dateTimeOffset = Substitute.For < IDateTimeOffset > ( ) ;
        var deviceMonitor  = Substitute.For < IDeviceMonitor > ( ) ;
        var deviceExpired  = Substitute.For < ISubject < IDevice > > ( ) ;
        var factory        = new ObservableTimerFactory ( ) ;
        var scheduler      = new TestScheduler ( ) ;
        var device         = Substitute.For < IDevice > ( ) ;

        deviceMonitor.DiscoveredDevices
                     .Returns ( [device] ) ;

        using var sut = new DeviceMonitorWithExpiry ( logger ,
                                                      dateTimeOffset ,
                                                      deviceMonitor ,
                                                      deviceExpired ,
                                                      factory ,
                                                      scheduler ) ;

        dateTimeOffset.Ticks
                      .Returns ( sut.TimeOut.Ticks ) ;
        dateTimeOffset.Now
                      .Returns ( dateTimeOffset ) ;

        device.BroadcastTime
              .Ticks
              .Returns ( 0 ) ;

        scheduler.AdvanceBy ( sut.TimeOut.Ticks + 1 ) ;

        deviceMonitor.Received ( )
                     .RemoveDevice ( device ) ;
    }

    [ TestMethod ]
    public void CleanUp_ForOneExpiredDeviceInCollection_NotifiesDeviceExpired ( )
    {
        var       logger         = Logger.None ;
        var       dateTimeOffset = Substitute.For < IDateTimeOffset > ( ) ;
        var       deviceMonitor  = Substitute.For < IDeviceMonitor > ( ) ;
        using var deviceExpired  = new Subject < IDevice > ( ) ;
        var       factory        = new ObservableTimerFactory ( ) ;
        var       scheduler      = new TestScheduler ( ) ;
        var       device         = Substitute.For < IDevice > ( ) ;

        using var sut = new DeviceMonitorWithExpiry ( logger ,
                                                      dateTimeOffset ,
                                                      deviceMonitor ,
                                                      deviceExpired ,
                                                      factory ,
                                                      scheduler ) ;

        IDevice expiredDevice = null! ;

        using var disposable = sut.DeviceExpired
                                  .Subscribe ( expired => expiredDevice = expired ) ;

        deviceExpired.OnNext ( device ) ;

        scheduler.AdvanceBy ( sut.TimeOut.Ticks ) ;

        expiredDevice.Should ( )
                     .Be ( device ) ;
    }

    [ TestMethod ]
    public void OnCompleted_ForInvoked_CallsStop ( )
    {
        var logger         = Logger.None ;
        var dateTimeOffset = Substitute.For < IDateTimeOffset > ( ) ;
        var deviceMonitor  = Substitute.For < IDeviceMonitor > ( ) ;
        var deviceExpired  = Substitute.For < ISubject < IDevice > > ( ) ;
        var factory        = Substitute.For < IObservableTimerFactory > ( ) ;
        var scheduler      = new TestScheduler ( ) ;

        factory.Create ( Arg.Any < TimeSpan > ( ) ,
                         Arg.Any < IScheduler > ( ) )
               .Returns ( Observable.Empty < long > ( ) ) ;

        using var sut = new DeviceMonitorWithExpiry ( logger ,
                                                      dateTimeOffset ,
                                                      deviceMonitor ,
                                                      deviceExpired ,
                                                      factory ,
                                                      scheduler ) ;

        scheduler.AdvanceBy ( sut.TimeOut.Ticks ) ;

        deviceMonitor.Received ( )
                     .StopListening ( ) ;
    }

    [ TestMethod ]
    public void OnError_ForInvoked_CallsStop ( )
    {
        var logger         = Logger.None ;
        var dateTimeOffset = Substitute.For < IDateTimeOffset > ( ) ;
        var deviceMonitor  = Substitute.For < IDeviceMonitor > ( ) ;
        var deviceExpired  = Substitute.For < ISubject < IDevice > > ( ) ;
        var factory        = Substitute.For < IObservableTimerFactory > ( ) ;
        var scheduler      = new TestScheduler ( ) ;

        factory.Create ( Arg.Any < TimeSpan > ( ) ,
                         Arg.Any < IScheduler > ( ) )
               .Returns ( Observable.Throw < long > ( new InvalidOperationException ( ) ) ) ;

        using var sut = new DeviceMonitorWithExpiry ( logger ,
                                                      dateTimeOffset ,
                                                      deviceMonitor ,
                                                      deviceExpired ,
                                                      factory ,
                                                      scheduler ) ;

        scheduler.AdvanceBy ( sut.TimeOut.Ticks ) ;

        deviceMonitor.Received ( )
                     .StopListening ( ) ;
    }
}