using FluentAssertions ;
using Idasen.BluetoothLE.Characteristics.Characteristics.Customs ;
using Idasen.BluetoothLE.Characteristics.Interfaces.Characteristics.Customs ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery.Wrappers ;
using NSubstitute ;

namespace Idasen.BluetoothLE.Characteristics.Tests.Characteristics.Customs ;

[ TestClass ]
public class GattCharacteristicsProviderFactoryTests
{
    [ TestMethod ]
    public void Create_ForInvoked_Instance ( )
    {
        // provide a factory delegate that returns a simple provider
        IGattCharacteristicProvider Factory ( IGattCharacteristicsResultWrapper _ ) => 
            Substitute.For < IGattCharacteristicProvider > ( ) ;

        var sut     = new GattCharacteristicsProviderFactory ( Factory ) ;
        var wrapper = Substitute.For< IGattCharacteristicsResultWrapper > ( ) ;

        sut.Create ( wrapper )
           .Should ( )
           .NotBeNull ( ) ;
    }
}
