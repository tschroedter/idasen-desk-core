using System.Reactive ;
using System.Reactive.Subjects ;
using FluentAssertions ;
using Idasen.BluetoothLE.Core.DevicesDiscovery ;
using Idasen.BluetoothLE.Core.Interfaces.DevicesDiscovery ;
using Microsoft.Reactive.Testing ;
using NSubstitute ;
using Serilog ;

namespace Idasen.BluetoothLE.Tests.DevicesDiscovery ;

[ TestClass ]
public class DeviceMonitorTests
{
    private IDevice                        _device             = null! ;
    private IDevice                        _deviceNewName      = null! ;
    private IDevice                        _deviceOtherNewName = null! ;
    private IDevices                       _devices            = null! ;
    private Func < ISubject < IDevice > >  _factory            = null! ;
    private ILogger                        _logger             = null! ;
    private TestScheduler                  _scheduler          = null! ;
    private Queue < ISubject < IDevice > > _subjects           = null! ;
    private ISubject < IDevice >           _subjectStarted     = null! ;
    private ISubject < IDevice >           _subjectStopped     = null! ;
    private ISubject < IDevice >           _subjectUpdated     = null! ;
    private IWatcher                       _watcher            = null! ;

    [ TestInitialize ]
    public void Initialize ( )
    {
        _scheduler = new TestScheduler ( ) ;

        _logger = Substitute.For < ILogger > ( ) ;

        _device = Substitute.For < IDevice > ( ) ;
        _device.Name
               .Returns ( ( string )null! ) ;

        _deviceNewName = Substitute.For < IDevice > ( ) ;
        _deviceNewName.Name
                      .Returns ( "New Name" ) ;

        _deviceOtherNewName = Substitute.For < IDevice > ( ) ;
        _deviceOtherNewName.Name
                           .Returns ( "Other New Name" ) ;

        _factory = Factory ;

        _subjectStarted = new Subject < IDevice > ( ) ;
        _subjectStopped = new Subject < IDevice > ( ) ;
        _subjectUpdated = new Subject < IDevice > ( ) ;

        _devices = new Devices ( _logger ) ;
        _watcher = Substitute.For < IWatcher > ( ) ;

        _subjects = new Queue < ISubject < IDevice > > ( ) ;
        _subjects.Enqueue ( _subjectStarted ) ;
        _subjects.Enqueue ( _subjectStopped ) ;
        _subjects.Enqueue ( _subjectUpdated ) ;
    }

    private Recorded < Notification < IDevice > > [ ] OnMultipleNext ( IEnumerable < IDevice > devices )
    {
        var list = new List < Recorded < Notification < IDevice > > > ( ) ;

        var time = 0 ;

        foreach ( var device in devices )
            list.Add ( OnNext ( time ++ ,
                                device ) ) ;

        return list.ToArray ( ) ;
    }

    private Recorded < Notification < IDevice > > OnNext ( long    time ,
                                                           IDevice device )
    {
        return new Recorded < Notification < IDevice > > ( time ,
                                                           Notification.CreateOnNext ( device ) ) ;
    }

    private ISubject < IDevice > Factory ( )
    {
        return _subjects.Dequeue ( ) ;
    }

    [ TestMethod ]
    public void Constructor_ForLoggerNull_Throws ( )
    {
        _logger = null! ;

        var action = ( ) => { CreateSut ( ) ; } ;

        action.Should ( )
              .Throw < ArgumentNullException > ( )
              .WithParameter ( "logger" ) ;
    }

    [ TestMethod ]
    public void Constructor_ForFactoryNull_Throws ( )
    {
        _factory = null! ;

        var action = ( ) => { CreateSut ( ) ; } ;

        action.Should ( )
              .Throw < ArgumentNullException > ( )
              .WithParameter ( "factory" ) ;
    }

    [ TestMethod ]
    public void Constructor_ForDevicesNull_Throws ( )
    {
        _devices = null! ;

        var action = ( ) => { CreateSut ( ) ; } ;

        action.Should ( )
              .Throw < ArgumentNullException > ( )
              .WithParameter ( "devices" ) ;
    }

    [ TestMethod ]
    public void Constructor_ForWatcherNull_Throws ( )
    {
        _watcher = null! ;

        var action = ( ) => { CreateSut ( ) ; } ;

        action.Should ( )
              .Throw < ArgumentNullException > ( )
              .WithParameter ( "watcher" ) ;
    }

    [ TestMethod ]
    public void Start_ForInvoked_CallsStart ( )
    {
        using var sut = CreateSut ( ) ;
        sut.Start ( ) ;

        _watcher.Received ( )
                .Start ( ) ;
    }

    [ TestMethod ]
    public void Stop_ForInvoked_CallsStop ( )
    {
        using var sut = CreateSut ( ) ;
        sut.Stop ( ) ;

        _watcher.Received ( )
                .Stop ( ) ;
    }

    [ TestMethod ]
    public void OnDeviceUpdated_ForNewDevice_AddsDevice ( )
    {
        ConfigureDeviceDiscovered ( ) ;

        using var sut = CreateSut ( ) ;

        sut.Start ( ) ;

        _scheduler.Start ( ) ;

        _devices.ContainsDevice ( _device )
                .Should ( )
                .BeTrue ( ) ;
    }

    [ TestMethod ]
    public void OnDeviceUpdated_ForNewDevice_RaisesDeviceDiscovered ( )
    {
        ConfigureDeviceDiscovered ( ) ;

        using var sut = CreateSut ( ) ;

        IDevice discovered = null! ;

        using var observer = sut.DeviceDiscovered
                                .Subscribe ( x => discovered = x ) ;

        sut.Start ( ) ;

        _scheduler.Start ( ) ;

        discovered.Should ( )
                  .Be ( _device ) ;
    }

    [ TestMethod ]
    public void OnDeviceUpdated_ForExistingDevice_UpdatesDevices ( )
    {
        ConfigureNameUpdated ( ) ;

        using var sut = CreateSut ( ) ;

        sut.Start ( ) ;

        _scheduler.Start ( ) ;

        _devices.TryGetDevice ( _device.Address ,
                                out var device )
                .Should ( )
                .BeTrue ( ) ;

        device?.Name
               .Should ( )
               .Be ( _deviceNewName.Name ) ;
    }

    [ TestMethod ]
    public void OnDeviceUpdated_ForExistingDevice_RaisesDeviceUpdated ( )
    {
        ConfigureSameDevice ( ) ;

        using var sut = CreateSut ( ) ;

        IDevice updated = null! ;

        using var observer = sut.DeviceUpdated
                                .Subscribe ( x => updated = x ) ;

        sut.Start ( ) ;

        _scheduler.Start ( ) ;

        updated.Should ( )
               .Be ( _device ) ;
    }

    [ TestMethod ]
    public void OnDeviceUpdated_ForExistingDeviceWithNewName_KeepsFirstName ( )
    {
        ConfigureNameUpdatedTwice ( ) ; // maybe, later allow name change?

        using var sut = CreateSut ( ) ;

        sut.Start ( ) ;

        _scheduler.Start ( ) ;

        _devices.TryGetDevice ( _device.Address ,
                                out var device )
                .Should ( )
                .BeTrue ( ) ;

        device?.Name
               .Should ( )
               .Be ( _deviceNewName.Name ) ;
    }

    [ TestMethod ]
    public void OnDeviceUpdated_ForExistingDeviceWithNewName_RaisesDeviceUpdated ( )
    {
        ConfigureNameUpdated ( ) ;

        using var sut = CreateSut ( ) ;

        IDevice updated = null! ;

        using var observer = sut.DeviceUpdated
                                .Subscribe ( x => updated = x ) ;

        sut.Start ( ) ;

        _scheduler.Start ( ) ;

        updated.Should ( )
               .Be ( _deviceNewName ) ;
    }

    private void ConfigureNameUpdatedTwice ( )
    {
        var messages = new [ ]
                       {
                           _device ,
                           _deviceNewName ,
                           _deviceOtherNewName
                       } ;

        _watcher.Received
                .Returns ( _scheduler.CreateColdObservable ( OnMultipleNext ( messages ) ) ) ;
    }

    private void ConfigureNameUpdated ( )
    {
        var messages = new [ ]
                       {
                           _device ,
                           _deviceNewName
                       } ;

        _watcher.Received
                .Returns ( _scheduler.CreateColdObservable ( OnMultipleNext ( messages ) ) ) ;
    }

    private void ConfigureDeviceDiscovered ( )
    {
        var messages = new [ ]
                       {
                           _device
                       } ;

        _watcher.Received
                .Returns ( _scheduler.CreateColdObservable ( OnMultipleNext ( messages ) ) ) ;
    }

    private void ConfigureSameDevice ( )
    {
        var messages = new [ ]
                       {
                           _device ,
                           _device
                       } ;

        _watcher.Received
                .Returns ( _scheduler.CreateColdObservable ( OnMultipleNext ( messages ) ) ) ;
    }

    [ TestMethod ]
    public void OnDeviceUpdated_ForExistingDeviceWithNewName_RaisesDeviceNameUpdated ( )
    {
        ConfigureNameUpdated ( ) ;

        using var sut = CreateSut ( ) ;

        IDevice updated = null! ;

        using var observer = sut.DeviceNameUpdated
                                .Subscribe ( x => updated = x ) ;

        sut.Start ( ) ;

        _scheduler.Start ( ) ;

        updated.Should ( )
               .Be ( _deviceNewName ) ;
    }

    [ TestMethod ]
    public void OnDeviceUpdated_ForExistingDevice_Notifies ( )
    {
        ConfigureSameDevice ( ) ;

        using var sut = CreateSut ( ) ;

        IDevice updated = null! ;

        using var observer = sut.DeviceUpdated
                                .Subscribe ( x => updated = x ) ;

        sut.Start ( ) ;

        _scheduler.Start ( ) ;

        updated.Should ( )
               .Be ( _device ) ;
    }

    [ TestMethod ]
    public void IsListening_ForInvoked_CallsWatcher ( )
    {
        _watcher.IsListening
                .Returns ( true ) ;

        using var sut = CreateSut ( ) ;
        sut.IsListening
           .Should ( )
           .BeTrue ( ) ;
    }

    [ TestMethod ]
    public void DiscoveredDevices_ForInvoked_CallsDevices ( )
    {
        var list = new List < IDevice > ( ).AsReadOnly ( ) ;

        _devices = Substitute.For < IDevices > ( ) ;
        _devices.DiscoveredDevices
                .Returns ( list ) ;

        using var sut = CreateSut ( ) ;
        sut.DiscoveredDevices
           .Should ( )
           .BeEquivalentTo ( list ) ;
    }

    [ TestMethod ]
    public void RemoveDevice_ForInvoked_CallsDevices ( )
    {
        _devices = Substitute.For < IDevices > ( ) ;

        using var sut = CreateSut ( ) ;

        sut.RemoveDevice ( _device ) ;

        _devices.Received ( )
                .RemoveDevice ( _device ) ;
    }

    private DeviceMonitor CreateSut ( )
    {
        var deviceMonitor = new DeviceMonitor ( _logger ,
                                                _scheduler ,
                                                _factory ,
                                                _devices ,
                                                _watcher ) ;

        return deviceMonitor ;
    }
}