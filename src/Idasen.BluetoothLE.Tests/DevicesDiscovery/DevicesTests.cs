using FluentAssertions ;
using FluentAssertions.Execution ;
using Idasen.BluetoothLE.Core.DevicesDiscovery ;
using Idasen.BluetoothLE.Core.Interfaces.DevicesDiscovery ;
using NSubstitute ;
using Serilog.Core ;

namespace Idasen.BluetoothLE.Tests.DevicesDiscovery ;

[ TestClass ]
public class DevicesTests
{
    private static Devices CreateSut ( )
    {
        return new Devices ( Logger.None ) ;
    }

    [ TestMethod ]
    public void Constructor_ForLoggerNull_Throws ( )
    {
        var action = ( ) => new Devices ( null! ) ;

        action.Should ( )
              .Throw < ArgumentNullException > ( )
              .WithParameterName ( "logger" ) ;
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
        var sut    = CreateSut ( ) ;
        var action = ( ) => { sut.AddOrUpdateDevice ( null! ) ; } ;

        action.Should ( )
              .Throw < ArgumentNullException > ( )
              .WithParameterName ( "device" ) ;
    }

    [ TestMethod ]
    public void RemoveDevice_ForDeviceIsNull_Throws ( )
    {
        var sut    = CreateSut ( ) ;
        var action = ( ) => { sut.RemoveDevice ( null! ) ; } ;

        action.Should ( )
              .Throw < ArgumentNullException > ( )
              .WithParameterName ( "device" ) ;
    }

    [ TestMethod ]
    public void ContainsDevice_ForDeviceIsNull_Throws ( )
    {
        var sut    = CreateSut ( ) ;
        var action = ( ) => { sut.ContainsDevice ( null! ) ; } ;

        action.Should ( )
              .Throw < ArgumentNullException > ( )
              .WithParameterName ( "device" ) ;
    }

    [ TestMethod ]
    public void AddOrUpdateDevice_ForNewDeviceAdded_IncreasesCount ( )
    {
        var sut    = CreateSut ( ) ;
        var device = Substitute.For < IDevice > ( ) ;

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
        var device   = Substitute.For < IDevice > ( ) ;
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
        var device1  = Substitute.For < IDevice > ( ) ;
        var device2  = Substitute.For < IDevice > ( ) ;
        var comparer = new DeviceComparer ( ) ;

        device1.Address.Returns ( 1u ) ;
        device2.Address.Returns ( 2u ) ;

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
        var device = Substitute.For < IDevice > ( ) ;

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
        var device   = Substitute.For < IDevice > ( ) ;
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
        var device1 = Substitute.For < IDevice > ( ) ;
        var device2 = Substitute.For < IDevice > ( ) ;

        device1.Address.Returns ( 1u ) ;
        device2.Address.Returns ( 2u ) ;

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
        var device1  = Substitute.For < IDevice > ( ) ;
        var device2  = Substitute.For < IDevice > ( ) ;
        var comparer = new DeviceComparer ( ) ;

        device1.Address.Returns ( 1u ) ;
        device2.Address.Returns ( 2u ) ;

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
        var device = Substitute.For < IDevice > ( ) ;

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
        var sut      = CreateSut ( ) ;
        var device1  = Substitute.For < IDevice > ( ) ;
        var device2  = Substitute.For < IDevice > ( ) ;
        var comparer = new DeviceComparer ( ) ;

        device2.Address
               .Returns ( device1.Address ) ;

        sut.AddOrUpdateDevice ( device1 ) ;
        sut.AddOrUpdateDevice ( device2 ) ;

        sut.DiscoveredDevices
           .Should ( )
           .ContainSingle ( x => comparer.Equals ( x ,
                                                   device2 ) ) ;
    }

    [ TestMethod ]
    public void AddOrUpdateDevice_ForDeviceWithEmptyName_UpdatesDeviceName ( )
    {
        var sut      = CreateSut ( ) ;
        var device1  = Substitute.For < IDevice > ( ) ;
        var device2  = Substitute.For < IDevice > ( ) ;
        var comparer = new DeviceComparer ( ) ;

        device1.Name
               .Returns ( string.Empty ) ;

        var address = device1.Address ;

        device2.Address
               .Returns ( address ) ;

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
        var device = Substitute.For < IDevice > ( ) ;

        sut.AddOrUpdateDevice ( device ) ;

        sut.ContainsDevice ( device )
           .Should ( )
           .BeTrue ( ) ;
    }

    [ TestMethod ]
    public void ContainsDevice_ForNotExistingDevice_ReturnsFalse ( )
    {
        var sut    = CreateSut ( ) ;
        var device = Substitute.For < IDevice > ( ) ;

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
        var device = Substitute.For < IDevice > ( ) ;

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
        var device   = Substitute.For < IDevice > ( ) ;
        var comparer = new DeviceComparer ( ) ;

        sut.AddOrUpdateDevice ( device ) ;

        sut.TryGetDevice ( device.Address ,
                           out var actual ) ;

        comparer.Equals ( actual ,
                          device )
                .Should ( )
                .BeTrue ( ) ;
    }
}