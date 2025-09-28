#nullable disable

namespace Idasen.BluetoothLE.Core.Tests.ServicesDiscovery ;

using Core.ServicesDiscovery ;
using FluentAssertions ;
using Interfaces.ServicesDiscovery ;
using NSubstitute ;
using Selkie.AutoMocking ;

[ AutoDataTestClass ]
public class MatchMakerTests
{
    [ AutoDataTestMethod ]
    public void Create_ForKnownAddress_Instance (
        MatchMaker sut ,
        ulong address )
    {
        sut.PairToDeviceAsync ( address )
           .Should ( )
           .NotBeNull ( ) ;
    }

    [ AutoDataTestMethod ]
    public async Task Create_ForUnknownAddress_Throws (
        Lazy < MatchMaker > sut ,
        [ Freeze ] IDeviceFactory deviceFactory ,
        ulong address )
    {
        deviceFactory.FromBluetoothAddressAsync ( Arg.Any < ulong > ( ) )
                     .Returns ( ( IDevice ) null ) ;

        Func < Task > action = async ( ) =>
                               {
                                   await sut.Value
                                            .PairToDeviceAsync ( address )
                                            .ConfigureAwait ( false ) ;
                               } ;

        await action.Should ( )
                    .ThrowAsync < ArgumentNullException > ( ) ;
    }
}
