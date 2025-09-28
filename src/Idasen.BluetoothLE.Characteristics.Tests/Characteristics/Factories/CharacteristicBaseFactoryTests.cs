namespace Idasen.BluetoothLE.Characteristics.Tests.Characteristics.Factories ;

using BluetoothLE.Characteristics.Characteristics ;
using Core.Interfaces.ServicesDiscovery ;
using FluentAssertions ;
using Selkie.AutoMocking ;

[ AutoDataTestClass ]
public class CharacteristicBaseFactoryTests
{
    [ AutoDataTestMethod ]
    public void Create_ForInvoked_Instance (
        CharacteristicBaseFactory sut ,
        IDevice device )
    {
        // not easy to  test because of dependency on ILifetimeScope
        sut.Create < object > ( device )
           .Should ( )
           .NotBeNull ( ) ;
    }
}
