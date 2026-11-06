using FluentAssertions ;
using Idasen.BluetoothLE.Core.ServicesDiscovery.Wrappers ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery.Wrappers ;
using NSubstitute ;

namespace Idasen.BluetoothLE.Core.Tests.ServicesDiscovery.Wrapper ;

[ TestClass ]
public class GattCharacteristicWrapperFactoryTests
{
    [ TestMethod ]
    public void Create_ForCharacteristicNull_Throws ( )
    {
        var sut = new GattCharacteristicWrapperFactory ( _ => Substitute.For< IGattCharacteristicWrapper > ( ) ) ;

        Action action = ( ) => sut.Create ( null! ) ;

        action.Should ( )
              .Throw < ArgumentNullException > ( ) ;
    }
}
