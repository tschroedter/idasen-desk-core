namespace Idasen.BluetoothLE.Characteristics.Tests.Characteristics.Customs ;

using BluetoothLE.Characteristics.Characteristics.Customs ;
using Core.Interfaces.ServicesDiscovery.Wrappers ;
using FluentAssertions ;
using Selkie.AutoMocking ;

[ AutoDataTestClass ]
public class GattCharacteristicsProviderFactoryTests
{
    [ AutoDataTestMethod ]
    public void Create_ForInvoked_Instance (
        GattCharacteristicsProviderFactory sut ,
        IGattCharacteristicsResultWrapper wrapper )
    {
        sut.Create ( wrapper )
           .Should ( )
           .NotBeNull ( ) ;
    }
}
