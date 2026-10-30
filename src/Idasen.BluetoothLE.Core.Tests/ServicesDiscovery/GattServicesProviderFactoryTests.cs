using FluentAssertions ;
using Idasen.BluetoothLE.Common.Tests ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery.Wrappers ;
using Idasen.BluetoothLE.Core.ServicesDiscovery ;
using NSubstitute ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery ;

namespace Idasen.BluetoothLE.Core.Tests.ServicesDiscovery ;

[ TestClass ]
public class GattServicesProviderFactoryTests
{
    [ TestMethod ]
    public void Create_ForWrapperNull_Throws ( )
    {
        var sut = new GattServicesProviderFactory ( _ => Substitute.For< IGattServicesProvider > ( ) ) ;

        Action action = ( ) => sut.Create ( null! ) ;

        action.Should ( )
              .Throw < ArgumentNullException > ( )
              .WithParameter ( "wrapper" ) ;
    }

    [ TestMethod ]
    public void Create_ForWrapper_Instance ( )
    {
        var sut = new GattServicesProviderFactory ( _ => Substitute.For< IGattServicesProvider > ( ) ) ;
        var wrapper = Substitute.For< IBluetoothLeDeviceWrapper > ( ) ;

        sut.Create ( wrapper )
           .Should ( )
           .NotBeNull ( ) ;
    }
}
