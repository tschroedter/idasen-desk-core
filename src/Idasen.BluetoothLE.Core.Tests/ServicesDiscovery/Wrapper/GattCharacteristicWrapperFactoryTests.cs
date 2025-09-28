namespace Idasen.BluetoothLE.Core.Tests.ServicesDiscovery.Wrapper ;

using Core.ServicesDiscovery.Wrappers ;
using FluentAssertions ;
using Selkie.AutoMocking ;

[ AutoDataTestClass ]
public class GattCharacteristicWrapperFactoryTests
{
    [ AutoDataTestMethod ]
    public void Create_ForCharacteristicNull_Throws (
        GattCharacteristicWrapperFactory sut )
    {
        Action action = ( ) => sut.Create ( null! ) ;

        action.Should ( )
              .Throw < ArgumentNullException > ( ) ;
    }
}
