using System.Reactive.Concurrency ;
using System.Reactive.Subjects ;
using Idasen.BluetoothLE.Core.DevicesDiscovery ;
using Idasen.BluetoothLE.Core.Interfaces ;
using Idasen.BluetoothLE.Core.Interfaces.DevicesDiscovery ;
using Microsoft.Reactive.Testing ;
using NSubstitute ;
using Serilog ;

namespace Idasen.BluetoothLE.Core.Tests.DevicesDiscovery ;

[ TestClass ]
public class DeviceMonitorWithExpiryTests
    : IDisposable
{
    private readonly Subject < long >        _subject        = new( ) ;
    private          IDateTimeOffset         _dateTimeOffset = null! ;
    private          ISubject < IDevice >    _deviceExpired  = null! ;
    private          IDeviceMonitor          _deviceMonitor  = null! ;
    private          IObservableTimerFactory _factory        = null! ;
    private          ILogger                 _logger         = null! ;
    private          IScheduler              _scheduler      = null! ;

    public void Dispose ( )
    {
        _deviceMonitor.Dispose ( ) ;
        _subject.Dispose ( ) ;

        GC.SuppressFinalize ( this ) ;
    }

    [ TestInitialize ]
    public void Setup ( )
    {
        _logger         = Substitute.For < ILogger > ( ) ;
        _dateTimeOffset = Substitute.For < IDateTimeOffset > ( ) ;
        _deviceMonitor  = Substitute.For < IDeviceMonitor > ( ) ;
        _deviceExpired  = new Subject < IDevice > ( ) ;
        _factory        = Substitute.For < IObservableTimerFactory > ( ) ;
        _scheduler      = new TestScheduler ( ) ;
    }

    [ TestMethod ]
    public void Constructor_ThrowsOnNullArguments ( )
    {
        Assert.ThrowsExactly < ArgumentNullException > ( ( ) => CreateSutForNullTest ( null! ,
                                                                                       _dateTimeOffset ,
                                                                                       _deviceMonitor ,
                                                                                       _deviceExpired ,
                                                                                       _factory ,
                                                                                       _scheduler ) ) ;
        Assert.ThrowsExactly < ArgumentNullException > ( ( ) => CreateSutForNullTest ( _logger ,
                                                                                       null! ,
                                                                                       _deviceMonitor ,
                                                                                       _deviceExpired ,
                                                                                       _factory ,
                                                                                       _scheduler ) ) ;
        Assert.ThrowsExactly < ArgumentNullException > ( ( ) => CreateSutForNullTest ( _logger ,
                                                                                       _dateTimeOffset ,
                                                                                       null! ,
                                                                                       _deviceExpired ,
                                                                                       _factory ,
                                                                                       _scheduler ) ) ;
        Assert.ThrowsExactly < ArgumentNullException > ( ( ) => CreateSutForNullTest ( _logger ,
                                                                                       _dateTimeOffset ,
                                                                                       _deviceMonitor ,
                                                                                       null! ,
                                                                                       _factory ,
                                                                                       _scheduler ) ) ;
        Assert.ThrowsExactly < ArgumentNullException > ( ( ) => CreateSutForNullTest ( _logger ,
                                                                                       _dateTimeOffset ,
                                                                                       _deviceMonitor ,
                                                                                       _deviceExpired ,
                                                                                       null! ,
                                                                                       _scheduler ) ) ;
        Assert.ThrowsExactly < ArgumentNullException > ( ( ) => CreateSutForNullTest ( _logger ,
                                                                                       _dateTimeOffset ,
                                                                                       _deviceMonitor ,
                                                                                       _deviceExpired ,
                                                                                       _factory ,
                                                                                       null! ) ) ;
    }

    private static DeviceMonitorWithExpiry CreateSutForNullTest ( ILogger                 logger ,
                                                                  IDateTimeOffset         dateTimeOffset ,
                                                                  IDeviceMonitor          deviceMonitor ,
                                                                  ISubject < IDevice >    deviceExpired ,
                                                                  IObservableTimerFactory factory ,
                                                                  IScheduler              scheduler )
    {
        return new DeviceMonitorWithExpiry ( logger ,
                                             dateTimeOffset ,
                                             deviceMonitor ,
                                             deviceExpired ,
                                             factory ,
                                             scheduler ) ;
    }

    [ TestMethod ]
    public void TimeOut_SetNegative_Throws ( )
    {
        Assert.ThrowsExactly < ArgumentException > ( ( ) =>
                                                     {
                                                         using var sut = CreateSut ( ) ;
                                                         sut.TimeOut = TimeSpan.FromSeconds ( - 1 ) ;
                                                     } ) ;
    }

    [ TestMethod ]
    public void TimeOut_SetValue_UpdatesPropertyAndLogs ( )
    {
        using var sut        = CreateSut ( ) ;
        var       newTimeout = TimeSpan.FromSeconds ( 10 ) ;

        sut.TimeOut = newTimeout ;

        Assert.AreEqual ( newTimeout ,
                          sut.TimeOut ) ;

        _logger.Received ( ).Information ( "TimeOut = {Timeout}" ,
                                           newTimeout ) ;
    }

    [ TestMethod ]
    public void Dispose_CallsDisposeOnDeviceMonitor ( )
    {
        var sut = CreateSut ( ) ;
        sut.Dispose ( ) ;
        _deviceMonitor.Received ( ).Dispose ( ) ;
    }

    [ TestMethod ]
    public void StartListening_DelegatesToDeviceMonitor ( )
    {
        using var sut = CreateSut ( ) ;
        sut.StartListening ( ) ;
        _deviceMonitor.Received ( ).StartListening ( ) ;
    }

    [ TestMethod ]
    public void StopListening_DelegatesToDeviceMonitor ( )
    {
        using var sut = CreateSut ( ) ;
        sut.StopListening ( ) ;
        _deviceMonitor.Received ( ).StopListening ( ) ;
    }

    [ TestMethod ]
    public void RemoveDevice_DelegatesToDeviceMonitor ( )
    {
        using var sut    = CreateSut ( ) ;
        var       device = Substitute.For < IDevice > ( ) ;
        sut.RemoveDevice ( device ) ;
        _deviceMonitor.Received ( ).RemoveDevice ( device ) ;
    }

    private DeviceMonitorWithExpiry CreateSut ( )
    {
        _factory.Create ( Arg.Any < TimeSpan > ( ) ,
                          Arg.Any < IScheduler > ( ) )
                .Returns ( _subject ) ;
        return new DeviceMonitorWithExpiry ( _logger ,
                                             _dateTimeOffset ,
                                             _deviceMonitor ,
                                             _deviceExpired ,
                                             _factory ,
                                             _scheduler ) ;
    }
}
