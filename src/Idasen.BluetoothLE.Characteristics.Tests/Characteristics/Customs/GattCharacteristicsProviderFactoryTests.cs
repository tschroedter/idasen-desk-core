using FluentAssertions ;
using Idasen.BluetoothLE.Characteristics.Characteristics.Customs ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery.Wrappers ;
using NSubstitute ;
using Microsoft.VisualStudio.TestTools.UnitTesting ;

namespace Idasen.BluetoothLE.Characteristics.Tests.Characteristics.Customs ;

[ TestClass ]
public class GattCharacteristicsProviderFactoryTests
{
    [ TestMethod ]
    public void Create_ForInvoked_Instance ( )
    {
        // provide a factory delegate that returns a simple provider
        GattCharacteristicProvider.Factory factory = _ => Substitute.For< Idasen.BluetoothLE.Characteristics.Interfaces.Characteristics.Customs.IGattCharacteristicProvider > ( ) ;

        var sut = new GattCharacteristicsProviderFactory ( factory ) ;
        var wrapper = Substitute.For< IGattCharacteristicsResultWrapper > ( ) ;

        sut.Create ( wrapper )
           .Should ( )
           .NotBeNull ( ) ;
    }
}
