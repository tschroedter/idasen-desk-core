#nullable disable

using FluentAssertions ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery ;
using Idasen.BluetoothLE.Core.ServicesDiscovery ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery.Wrappers ;
using NSubstitute ;

namespace Idasen.BluetoothLE.Core.Tests.ServicesDiscovery ;

[ TestClass ]
public class MatchMakerTests
{
    [ TestMethod ]
    public void Create_ForKnownAddress_Instance ( )
    {
        var logger = Substitute.For< Serilog.ILogger > ( ) ;
        var collection = Substitute.For< IOfficialGattServicesCollection > ( ) ;
        var deviceFactory = Substitute.For< IDeviceFactory > ( ) ;
        var device = Substitute.For< IDevice > ( ) ;
        deviceFactory.FromBluetoothAddressAsync ( Arg.Any< ulong > ( ) )
                     .Returns ( device ) ;

        var sut = new MatchMaker ( logger , collection , deviceFactory ) ;

        sut.PairToDeviceAsync ( 123UL )
           .Should ( )
           .NotBeNull ( ) ;
    }

    [ TestMethod ]
    public async Task Create_ForUnknownAddress_Throws ( )
    {
        var logger = Substitute.For< Serilog.ILogger > ( ) ;
        var collection = Substitute.For< IOfficialGattServicesCollection > ( ) ;
        var deviceFactory = Substitute.For< IDeviceFactory > ( ) ;
        deviceFactory.FromBluetoothAddressAsync ( Arg.Any< ulong > ( ) )
                     .Returns ( ( IDevice )null ) ;

        var sut = new MatchMaker ( logger , collection , deviceFactory ) ;

        var action = async ( ) =>
                     {
                         await sut.PairToDeviceAsync ( 123UL )
                                  .ConfigureAwait ( false ) ;
                     } ;

        await action.Should ( )
                    .ThrowAsync < ArgumentNullException > ( ) ;
    }
}
