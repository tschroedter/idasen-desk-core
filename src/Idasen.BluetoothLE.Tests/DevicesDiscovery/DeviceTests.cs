namespace Idasen.BluetoothLE.Tests.DevicesDiscovery ;

using Core ;
using Core.DevicesDiscovery ;
using Core.Interfaces ;
using FluentAssertions ;

[ TestClass ]
public class DeviceTests
{
    private const ulong Address = 1234 ;
    private const string Name = "Name" ;
    private const short RawSignalStrengthInDBm = - 50 ;

    private IDateTimeOffset _broadcastTime = null! ;
    private DeviceComparer _comparer = null! ;

    [ TestInitialize ]
    public void Initialize ( )
    {
        var dateTimeOffset = DateTimeOffset.Parse ( "2/10/2007 1:02:03 PM -7:30" ) ;
        _broadcastTime = new DateTimeOffsetWrapper ( dateTimeOffset ) ;

        _comparer = new DeviceComparer ( ) ;
    }

    [ TestMethod ]
    public void Constructor_ForDeviceIsNull_Throws ( )
    {
        // ReSharper disable once ObjectCreationAsStatement
        Action action = ( ) => { new Device ( null! ) ; } ;

        action.Should ( )
              .Throw < ArgumentNullException > ( )
              .WithParameter ( "device" ) ;
    }

    [ TestMethod ]
    public void Constructor_ForBroadcastTimeNull_Throws ( )
    {
        _broadcastTime = null! ;

        // ReSharper disable once ObjectCreationAsStatement
        Action action = ( ) => { CreateSut ( ) ; } ;

        action.Should ( )
              .Throw < ArgumentNullException > ( )
              .WithParameter ( "broadcastTime" ) ;
    }

    [ TestMethod ]
    public void Constructor_ForInvoked_SetsBroadcastTime ( )
    {
        Device sut = CreateSut ( ) ;

        sut.BroadcastTime
           .Should ( )
           .Be ( _broadcastTime ) ;
    }

    [ TestMethod ]
    public void Constructor_ForInvoked_SetsAddress ( )
    {
        Device sut = CreateSut ( ) ;

        sut.Address
           .Should ( )
           .Be ( Address ) ;
    }

    [ TestMethod ]
    public void Constructor_ForInvoked_SetsName ( )
    {
        Device sut = CreateSut ( ) ;

        sut.Name
           .Should ( )
           .Be ( Name ) ;
    }

    [ TestMethod ]
    public void Constructor_ForInvoked_SetsRawSignalStrengthInDBm ( )
    {
        Device sut = CreateSut ( ) ;

        sut.RawSignalStrengthInDBm
           .Should ( )
           .Be ( RawSignalStrengthInDBm ) ;
    }

    [ TestMethod ]
    public void ToString_ForInvoked_ReturnsInstance ( )
    {
        Device sut = CreateSut ( ) ;

        sut.ToString ( )
           .Should ( )
           .NotBeNullOrEmpty ( ) ;
    }

    [ TestMethod ]
    public void Constructor_ForIDevice_ReturnsInstance ( )
    {
        Device device = CreateSut ( ) ;

        var sut = new Device ( device ) ;

        _comparer.Equals ( sut ,
                           device )
                 .Should ( )
                 .BeTrue ( ) ;
    }

    private Device CreateSut ( )
    {
        return new Device ( _broadcastTime ,
                            Address ,
                            Name ,
                            RawSignalStrengthInDBm ) ;
    }
}
