namespace Idasen.BluetoothLE.Core.Tests.ServicesDiscovery ;

using Common.Tests ;
using Core.ServicesDiscovery ;
using FluentAssertions ;
using Interfaces.ServicesDiscovery.Wrappers ;
using Selkie.AutoMocking ;

[ AutoDataTestClass ]
public class GattServicesProviderFactoryTests
{
    [ AutoDataTestMethod ]
    public void Create_ForWrapperNull_Throws (
        GattServicesProviderFactory sut )
    {
        Action action = ( ) => sut.Create ( null! ) ;

        action.Should ( )
              .Throw < ArgumentNullException > ( )
              .WithParameter ( "wrapper" ) ;
    }

    [ AutoDataTestMethod ]
    public void Create_ForWrapper_Instance (
        GattServicesProviderFactory sut ,
        IBluetoothLeDeviceWrapper wrapper )
    {
        sut.Create ( wrapper )
           .Should ( )
           .NotBeNull ( ) ;
    }
}
