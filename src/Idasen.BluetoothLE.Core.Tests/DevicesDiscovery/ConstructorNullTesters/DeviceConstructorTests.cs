using FluentAssertions ;
using Idasen.BluetoothLE.Common.Tests ;
using Idasen.BluetoothLE.Core.DevicesDiscovery ;
using Idasen.BluetoothLE.Core.Interfaces ;
using NSubstitute ;

namespace Idasen.BluetoothLE.Core.Tests.DevicesDiscovery.ConstructorNullTesters ;

[ TestClass ]
public class DeviceConstructorTests
    : BaseConstructorNullTester < Device >
{
    public override int NumberOfConstructorsPassed => 2 ;
    public override int NumberOfConstructorsFailed => 0 ;

    [ TestMethod ]
    public override void Constructor_ForAnyParameterNullThrows_AllPassing ( )
    {
        Action nullDevice = ( ) => _ = new Device ( null! ) ;
        nullDevice.Should ( )
                  .Throw < ArgumentNullException > ( )
                  .WithParameter ( "device" ) ;

        Action nullBroadcastTime = ( ) => _ = new Device ( null! ,
                                                           0ul ,
                                                           null ,
                                                           0 ) ;
        nullBroadcastTime.Should ( )
                         .Throw < ArgumentNullException > ( )
                         .WithParameter ( "broadcastTime" ) ;

        var device = new Device ( Substitute.For < IDateTimeOffset > ( ) ,
                                  0ul ,
                                  null ,
                                  0 ) ;
        device.Name.Should ( )
                    .BeNull ( ) ;
    }
}
