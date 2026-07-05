using FluentAssertions ;
using FluentAssertions.Execution ;
using Idasen.BluetoothLE.Common.Tests ;
using Idasen.BluetoothLE.Core.DevicesDiscovery ;
using Idasen.BluetoothLE.Core.Interfaces ;
using Idasen.BluetoothLE.Core.Interfaces.DevicesDiscovery ;
using NSubstitute ;
using Serilog ;
using Serilog.Core ;

namespace Idasen.BluetoothLE.Core.Tests.DevicesDiscovery ;

[ TestClass ]
public class DevicesTests
{
    private ILogger _logger = null! ;

    [ TestInitialize ]
    public void Setup ( )
    {
        _logger = Logger.None ;
    }

    private Devices CreateSut ( )
    {
        return new Devices ( _logger ) ;
    }

    private static IDevice CreateDevice ( ulong address = 123456ul ,
                                          string name   = "TestDevice" )
    {
        var device = Substitute.For < IDevice > ( ) ;
        device.Address
              .Returns ( address ) ;
        device.Name
              .Returns ( name ) ;
        device.RawSignalStrengthInDBm
              .Returns ( ( short ) - 50 ) ;
        device.BroadcastTime
              .Returns ( Substitute.For < IDateTimeOffset > ( ) ) ;
        return device ;
    }
    [ TestMethod ]
    public void Constructor_ForLoggerNull_Throws ( )
    {
        // ReSharper disable once ObjectCreationAsStatement
        var action = ( ) => { new Devices ( null! ) ; } ;

        action.Should ( )
              .Throw < ArgumentNullException > ( )
              .WithParameter ( "logger" ) ;
    }

    [ TestMethod ]
    public void DiscoveredDevices_ForInitialized_IsEmpty ( )
    {
        var sut = CreateSut ( ) ;

        sut.DiscoveredDevices
           .Should ( )
           .BeEmpty ( "Should be empty when created" ) ;
    }

    [ TestMethod ]
    public void AddOrUpdateDevice_ForDeviceIsNull_Throws ( )
    {
        var sut = CreateSut ( ) ;

        var action = ( ) => { sut.AddOrUpdateDevice ( null! ) ; } ;

        action.Should ( )
              .Throw < ArgumentNullException > ( )
              .WithParameter ( "device" ) ;
    }

    [ TestMethod ]
    public void RemoveDevice_ForDeviceIsNull_Throws ( )
    {
        var sut = CreateSut ( ) ;

        var action = ( ) => { sut.RemoveDevice ( null! ) ; } ;

        action.Should ( )
              .Throw < ArgumentNullException > ( )
              .WithParameter ( "device" ) ;
    }

    [ TestMethod ]
    public void ContainsDevice_ForDeviceIsNull_Throws ( )
    {
        var sut = CreateSut ( ) ;

        var action = ( ) => { sut.ContainsDevice ( null! ) ; } ;

        action.Should ( )
              .Throw < ArgumentNullException > ( )
              .WithParameter ( "device" ) ;
    }

    [ TestMethod ]
    public void AddOrUpdateDevice_ForNewDeviceAdded_IncreasesCount ( )
    {
        var sut    = CreateSut ( ) ;
        var device = CreateDevice ( ) ;

        sut.AddOrUpdateDevice ( device ) ;

        sut.DiscoveredDevices
           .Count
           .Should ( )
           .Be ( 1 ) ;
    }

    [ TestMethod ]
    public void Remove_ForExistingDevice_RemovesDevice ( )
    {
        var sut      = CreateSut ( ) ;
        var device   = CreateDevice ( ) ;
        var comparer = new DeviceComparer ( ) ;

        sut.AddOrUpdateDevice ( device ) ;

        sut.RemoveDevice ( device ) ;

        sut.DiscoveredDevices
           .Should ( )
           .NotContain ( x => comparer.Equals ( x ,
                                                device ) ) ;
    }

    [ TestMethod ]
    public void Remove_ForExistingDevice_DoesNotRemovesOtherDevice ( )
    {
        var sut      = CreateSut ( ) ;
        var device1  = CreateDevice ( 123ul , "Device1" ) ;
        var device2  = CreateDevice ( 456ul , "Device2" ) ;
        var comparer = new DeviceComparer ( ) ;

        sut.AddOrUpdateDevice ( device1 ) ;
        sut.AddOrUpdateDevice ( device2 ) ;

        sut.RemoveDevice ( device1 ) ;

        using var scope = new AssertionScope ( ) ;

        sut.DiscoveredDevices
           .Should ( )
           .NotContain ( x => comparer.Equals ( x ,
                                                device1 ) ) ;

        sut.DiscoveredDevices
           .Should ( )
           .ContainSingle ( x => comparer.Equals ( x ,
                                                   device2 ) ) ;
    }

    [ TestMethod ]
    public void Remove_ForExistingDevice_DecreasesCount ( )
    {
        var sut    = CreateSut ( ) ;
        var device = CreateDevice ( ) ;

        sut.AddOrUpdateDevice ( device ) ;

        sut.RemoveDevice ( device ) ;

        sut.DiscoveredDevices
           .Count
           .Should ( )
           .Be ( 0 ) ;
    }

    [ TestMethod ]
    public void AddOrUpdateDevice_ForNewDeviceAdded_DeviceAdded ( )
    {
        var sut      = CreateSut ( ) ;
        var device   = CreateDevice ( ) ;
        var comparer = new DeviceComparer ( ) ;

        sut.AddOrUpdateDevice ( device ) ;

        sut.DiscoveredDevices
           .Should ( )
           .ContainSingle ( x => comparer.Equals ( x ,
                                                   device ) ) ;
    }

    [ TestMethod ]
    public void AddOrUpdateDevice_ForTwoNewDevicesAdded_IncreasesCount ( )
    {
        var sut     = CreateSut ( ) ;
        var device1 = CreateDevice ( 123ul , "Device1" ) ;
        var device2 = CreateDevice ( 456ul , "Device2" ) ;

        sut.AddOrUpdateDevice ( device1 ) ;
        sut.AddOrUpdateDevice ( device2 ) ;

        sut.DiscoveredDevices
           .Count
           .Should ( )
           .Be ( 2 ) ;
    }

    [ TestMethod ]
    public void AddOrUpdateDevice_ForTwoNewDevicesAdded_DevicesAdded ( )
    {
        var sut      = CreateSut ( ) ;
        var device1  = CreateDevice ( 123ul , "Device1" ) ;
        var device2  = CreateDevice ( 456ul , "Device2" ) ;
        var comparer = new DeviceComparer ( ) ;

        sut.AddOrUpdateDevice ( device1 ) ;
        sut.AddOrUpdateDevice ( device2 ) ;

        using var scope = new AssertionScope ( ) ;

        sut.DiscoveredDevices
           .Should ( )
           .ContainSingle ( x => comparer.Equals ( x ,
                                                   device1 ) ) ;

        sut.DiscoveredDevices
           .Should ( )
           .ContainSingle ( x => comparer.Equals ( x ,
                                                   device2 ) ) ;
    }

    [ TestMethod ]
    public void AddOrUpdateDevice_ForSameDeviceAddedTwice_CountStaysTheSame ( )
    {
        var sut    = CreateSut ( ) ;
        var device = CreateDevice ( ) ;

        sut.AddOrUpdateDevice ( device ) ;
        sut.AddOrUpdateDevice ( device ) ;

        sut.DiscoveredDevices
           .Count
           .Should ( )
           .Be ( 1 ) ;
    }

    [ TestMethod ]
    public void AddOrUpdateDevice_ForSameDeviceAddedTwice_UpdatesDevice ( )
    {
        var sut     = CreateSut ( ) ;
        var device1 = CreateDevice ( 123ul , "Device1" ) ;
        var device2 = CreateDevice ( 123ul , "Device2" ) ;

        // Configure device2 with different signal strength to verify update
        device2.RawSignalStrengthInDBm
               .Returns ( ( short ) - 75 ) ;

        sut.AddOrUpdateDevice ( device1 ) ;
        sut.AddOrUpdateDevice ( device2 ) ;

        sut.TryGetDevice ( 123ul ,
                           out var storedDevice ) ;

        // The signal strength should be updated to device2's value
        storedDevice!.RawSignalStrengthInDBm
                    .Should ( )
                    .Be ( - 75 ) ;
    }

    [ TestMethod ]
    public void AddOrUpdateDevice_ForDeviceWithEmptyName_UpdatesDeviceName ( )
    {
        var sut      = CreateSut ( ) ;
        var device1  = CreateDevice ( 123ul , string.Empty ) ;
        var device2  = CreateDevice ( 123ul , "UpdatedDevice" ) ;
        var comparer = new DeviceComparer ( ) ;

        sut.AddOrUpdateDevice ( device1 ) ;
        sut.AddOrUpdateDevice ( device2 ) ;

        sut.DiscoveredDevices
           .Should ( )
           .ContainSingle ( x => comparer.Equals ( x ,
                                                   device2 ) ) ;
    }

    [ TestMethod ]
    public void ContainsDevice_ForExistingDevice_ReturnsTrue ( )
    {
        var sut    = CreateSut ( ) ;
        var device = CreateDevice ( ) ;

        sut.AddOrUpdateDevice ( device ) ;

        sut.ContainsDevice ( device )
           .Should ( )
           .BeTrue ( ) ;
    }

    [ TestMethod ]
    public void ContainsDevice_ForNotExistingDevice_ReturnsFalse ( )
    {
        var sut    = CreateSut ( ) ;
        var device = CreateDevice ( ) ;

        sut.ContainsDevice ( device )
           .Should ( )
           .BeFalse ( ) ;
    }

    [ TestMethod ]
    public void TryGetDevice_ForNotExistingDevice_ReturnsFalse ( )
    {
        var sut = CreateSut ( ) ;

        sut.TryGetDevice ( 0ul ,
                           out _ )
           .Should ( )
           .BeFalse ( ) ;
    }

    [ TestMethod ]
    public void TryGetDevice_ForExistingDevice_ReturnsTrue ( )
    {
        var sut    = CreateSut ( ) ;
        var device = CreateDevice ( ) ;

        sut.AddOrUpdateDevice ( device ) ;

        sut.TryGetDevice ( device.Address ,
                           out _ )
           .Should ( )
           .BeTrue ( ) ;
    }

    [ TestMethod ]
    public void TryGetDevice_ForExistingDevice_ReturnsDevice ( )
    {
        var sut      = CreateSut ( ) ;
        var device   = CreateDevice ( ) ;
        var comparer = new DeviceComparer ( ) ;

        sut.AddOrUpdateDevice ( device ) ;

        sut.TryGetDevice ( device.Address ,
                           out var actual ) ;

        comparer.Equals ( actual ,
                          device )
                .Should ( )
                .BeTrue ( ) ;
    }

    [ TestMethod ]
    public void Clear_ForInvoked_ClearsDiscoveredDevices ( )
    {
        var sut    = CreateSut ( ) ;
        var device = CreateDevice ( ) ;

        sut.AddOrUpdateDevice ( device ) ;

        sut.Clear ( ) ;

        sut.DiscoveredDevices
           .Should ( )
           .BeEmpty ( ) ;
    }
}
