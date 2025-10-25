using System.Reactive.Subjects ;
using FluentAssertions ;
using Idasen.BluetoothLE.Common.Tests ;
using Idasen.BluetoothLE.Core.Interfaces.DevicesDiscovery ;
using Idasen.BluetoothLE.Linak.Interfaces ;
using JetBrains.Annotations ;
using Microsoft.Reactive.Testing ;
using NSubstitute ;
using Serilog ;

namespace Idasen.BluetoothLE.Linak.Tests ;

[ TestClass ]
public class DeskDetectorTests : IDisposable
{
    private const string              DeviceName    = nameof ( DeviceName ) ;
    private const uint                DeviceAddress = 123 ;
    private const uint                DeviceTimeout = 456 ;
    private       IDesk               _desk         = null! ;
    private       ISubject < IDesk >  _deskDetected = null! ;
    private       Subject < IDevice > _deskFound    = null! ;
    private       IDesk               _deskOther    = null! ;
    private       IDevice             _device       = null! ;
    private       Subject < IDevice > _discovered   = null! ;
    private       IDeskFactory        _factory      = null! ;

    private ILogger                  _logger  = null! ;
    private IDeviceMonitorWithExpiry _monitor = null! ;

    [ UsedImplicitly ] private Subject < IDevice > _nameChanged = null! ;

    private TestScheduler       _scheduler = null! ;
    private Subject < IDevice > _updated   = null! ;

    public void Dispose ( )
    {
        ( _deskDetected as IDisposable )?.Dispose ( ) ;
        _updated.OnCompleted ( ) ;
        _discovered.OnCompleted ( ) ;
        _nameChanged.OnCompleted ( ) ;
        _deskFound.OnCompleted ( ) ;

        _updated.Dispose ( ) ;
        _discovered.Dispose ( ) ;
        _nameChanged.Dispose ( ) ;
        _deskFound.Dispose ( ) ;
        GC.SuppressFinalize ( this ) ;
    }

    [ TestInitialize ]
    public void Initialize ( )
    {
        _logger       = Substitute.For < ILogger > ( ) ;
        _scheduler    = new TestScheduler ( ) ;
        _monitor      = Substitute.For < IDeviceMonitorWithExpiry > ( ) ;
        _factory      = Substitute.For < IDeskFactory > ( ) ;
        _deskDetected = new Subject < IDesk > ( ) ;

        _updated = new Subject < IDevice > ( ) ;
        _monitor.DeviceUpdated.Returns ( _updated ) ;

        _discovered = new Subject < IDevice > ( ) ;
        _monitor.DeviceDiscovered.Returns ( _discovered ) ;

        _nameChanged = new Subject < IDevice > ( ) ;
        _monitor.DeviceNameUpdated.Returns ( _discovered ) ;

        _deskFound = new Subject < IDevice > ( ) ;
        _monitor.DeviceUpdated.Returns ( _deskFound ) ;

        _device = Substitute.For < IDevice > ( ) ;
        _device.Name.Returns ( DeviceName ) ;
        _device.Address.Returns ( DeviceAddress ) ;

        _desk      = Substitute.For < IDesk > ( ) ;
        _deskOther = Substitute.For < IDesk > ( ) ;
        _factory.CreateAsync ( _device.Address )
                .Returns ( _desk ,
                           _deskOther ) ;
    }

    [ TestMethod ]
    public void Initialize_ForDeviceNameIsNull_Throws ( )
    {
        Action action = ( ) => CreateSut ( ).Initialize ( null! ,
                                                          DeviceAddress ,
                                                          DeviceTimeout ) ;

        action.Should ( )
              .Throw < ArgumentNullException > ( )
              .WithParameter ( "deviceName" ) ;
    }

    [ TestMethod ]
    public void Initialize_Invoked_SetsTimeout ( )
    {
        using var sut = CreateSut ( ) ;

        sut.Initialize ( DeviceName ,
                         DeviceAddress ,
                         DeviceTimeout ) ;

        _monitor.TimeOut
                .Should ( )
                .Be ( TimeSpan.FromSeconds ( DeviceTimeout ) ) ;
    }

    [ TestMethod ]
    public void Start_ConnectedToAnotherDesk_DisposesOldDesk ( )
    {
        using var sut = CreateSut ( ) ;

        sut.Initialize ( DeviceName ,
                         DeviceAddress ,
                         DeviceTimeout ) ;

        // connect to desk
        sut.StartListening ( ) ;

        _discovered.OnNext ( _device ) ;

        _scheduler.Start ( ) ;

        // connect to desk again, so that the old one is disposed
        sut.StartListening ( ) ;

        _desk.Received ( )
             .Dispose ( ) ;
    }

    [ TestMethod ]
    public void Dispose_Invoked_DisposesMonitor ( )
    {
        var sut = CreateSut ( ) ;

        sut.Dispose ( ) ;

        _monitor.Received ( )
                .Dispose ( ) ;
    }

    [ TestMethod ]
    public void Initialize_ForDeviceAddressIsZero_Throws ( )
    {
        Action action = ( ) => CreateSut ( ).Initialize ( DeviceName ,
                                                          0 ,
                                                          DeviceTimeout ) ;

        action.Should ( )
              .Throw < ArgumentException > ( )
              .WithMessage ( "Device address must be a valid non-zero value.*" ) ;
    }

    [ TestMethod ]
    public void Initialize_ForDeviceTimeoutIsZero_Throws ( )
    {
        Action action = ( ) => CreateSut ( ).Initialize ( DeviceName ,
                                                          DeviceAddress ,
                                                          0 ) ;

        action.Should ( )
              .Throw < ArgumentOutOfRangeException > ( )
              .WithMessage ( "Device timeout must be between 1 and 3600 seconds.*" ) ;
    }

    [ TestMethod ]
    public void Initialize_ForDeviceTimeoutExceedsLimit_Throws ( )
    {
        Action action = ( ) => CreateSut ( ).Initialize ( DeviceName ,
                                                          DeviceAddress ,
                                                          3601 ) ;

        action.Should ( )
              .Throw < ArgumentOutOfRangeException > ( )
              .WithMessage ( "Device timeout must be between 1 and 3600 seconds.*" ) ;
    }

    // todo figure out how to test disposing of IDisposables of Subjects
    // todo improve code coverage

    private DeskDetector CreateSut ( )
    {
        return new DeskDetector ( _logger ,
                                  _scheduler ,
                                  _monitor ,
                                  _factory ,
                                  _deskDetected ) ;
    }
}