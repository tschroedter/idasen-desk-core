using FluentAssertions ;
using Idasen.BluetoothLE.Common.Tests ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery.Wrappers ;
using Idasen.BluetoothLE.Core.ServicesDiscovery ;
using NSubstitute ;

namespace Idasen.BluetoothLE.Core.Tests.ServicesDiscovery ;

[ TestClass ]
public class GattServicesTests
{
    [ TestMethod ]
    public void Indexer_ForServiceAndResultNull_Throws ( )
    {
        using var sut = new GattServices ( ) ;

        var service = Substitute.For< IGattDeviceServiceWrapper > ( ) ;

        // ReSharper disable once AccessToDisposedClosure
        Action action = ( ) => sut [ service ] = null! ;

        action.Should ( )
              .Throw < ArgumentException > ( )
              .WithParameter ( "value" ) ;
    }

    [ TestMethod ]
    public void Indexer_ForServiceAndResult_SetsKeyAndValue ( )
    {
        using var sut = new GattServices ( ) ;

        var service = Substitute.For< IGattDeviceServiceWrapper > ( ) ;
        var result = Substitute.For< IGattCharacteristicsResultWrapper > ( ) ;

        sut [ service ] = result ;

        sut [ service ]
           .Should ( )
           .Be ( result ) ;
    }

    [ TestMethod ]
    public void Indexer_ForServiceAndResult_UpdatesCount ( )
    {
        using var sut = new GattServices ( ) ;

        var service = Substitute.For< IGattDeviceServiceWrapper > ( ) ;
        var result = Substitute.For< IGattCharacteristicsResultWrapper > ( ) ;

        sut [ service ] = result ;

        sut.Count
           .Should ( )
           .Be ( 1 ) ;
    }

    [ TestMethod ]
    public void Indexer_ForExistingService_DisposesOldValueOnly ( )
    {
        using var sut = new GattServices ( ) ;

        var service = Substitute.For< IGattDeviceServiceWrapper > ( ) ;
        var first = Substitute.For< IGattCharacteristicsResultWrapper > ( ) ;
        var second = Substitute.For< IGattCharacteristicsResultWrapper > ( ) ;

        sut [ service ] = first ;
        sut [ service ] = second ;

        sut [ service ]
           .Should ( )
           .Be ( second ) ;

        first.Received ( 1 )
             .Dispose ( ) ;
        second.DidNotReceive ( )
              .Dispose ( ) ;
        service.DidNotReceive ( )
               .Dispose ( ) ;
    }

    [ TestMethod ]
    public void Clear_ForInvoked_DisposesService1 ( )
    {
        using var sut = new GattServices ( ) ;

        var service1 = Substitute.For< IGattDeviceServiceWrapper > ( ) ;
        var result1 = Substitute.For< IGattCharacteristicsResultWrapper > ( ) ;
        var service2 = Substitute.For< IGattDeviceServiceWrapper > ( ) ;
        var result2 = Substitute.For< IGattCharacteristicsResultWrapper > ( ) ;

        sut [ service1 ] = result1 ;
        sut [ service2 ] = result2 ;

        sut.Clear ( ) ;

        service1.Received ( )
                .Dispose ( ) ;
    }

    [ TestMethod ]
    public void Clear_ForInvoked_DisposesService2 ( )
    {
        using var sut = new GattServices ( ) ;

        var service1 = Substitute.For< IGattDeviceServiceWrapper > ( ) ;
        var result1 = Substitute.For< IGattCharacteristicsResultWrapper > ( ) ;
        var service2 = Substitute.For< IGattDeviceServiceWrapper > ( ) ;
        var result2 = Substitute.For< IGattCharacteristicsResultWrapper > ( ) ;

        sut [ service1 ] = result1 ;
        sut [ service2 ] = result2 ;

        sut.Clear ( ) ;

        service2.Received ( )
                .Dispose ( ) ;
    }

    [ TestMethod ]
    public void Clear_ForInvoked_DisposesValues ( )
    {
        using var sut = new GattServices ( ) ;

        var service1 = Substitute.For< IGattDeviceServiceWrapper > ( ) ;
        var result1 = Substitute.For< IGattCharacteristicsResultWrapper > ( ) ;
        var service2 = Substitute.For< IGattDeviceServiceWrapper > ( ) ;
        var result2 = Substitute.For< IGattCharacteristicsResultWrapper > ( ) ;

        sut [ service1 ] = result1 ;
        sut [ service2 ] = result2 ;

        sut.Clear ( ) ;

        result1.Received ( )
               .Dispose ( ) ;
        result2.Received ( )
               .Dispose ( ) ;
    }

    [ TestMethod ]
    public void Clear_ForInvoked_SetsCountToZero ( )
    {
        using var sut = new GattServices ( ) ;

        var service = Substitute.For< IGattDeviceServiceWrapper > ( ) ;
        var result = Substitute.For< IGattCharacteristicsResultWrapper > ( ) ;

        sut [ service ] = result ;

        sut.Clear ( ) ;

        sut.Count
           .Should ( )
           .Be ( 0 ) ;
    }

    [ TestMethod ]
    public void Dispose_ForInvoked_DisposesService1 ( )
    {
        var sut = new GattServices ( ) ;
        var service1 = Substitute.For< IGattDeviceServiceWrapper > ( ) ;
        var result1 = Substitute.For< IGattCharacteristicsResultWrapper > ( ) ;
        var service2 = Substitute.For< IGattDeviceServiceWrapper > ( ) ;
        var result2 = Substitute.For< IGattCharacteristicsResultWrapper > ( ) ;

        sut [ service1 ] = result1 ;
        sut [ service2 ] = result2 ;

        sut.Dispose ( ) ;

        service1.Received ( )
                .Dispose ( ) ;
    }

    [ TestMethod ]
    public void Dispose_ForInvoked_DisposesService2 ( )
    {
        var sut = new GattServices ( ) ;
        var service1 = Substitute.For< IGattDeviceServiceWrapper > ( ) ;
        var result1 = Substitute.For< IGattCharacteristicsResultWrapper > ( ) ;
        var service2 = Substitute.For< IGattDeviceServiceWrapper > ( ) ;
        var result2 = Substitute.For< IGattCharacteristicsResultWrapper > ( ) ;

        sut [ service1 ] = result1 ;
        sut [ service2 ] = result2 ;

        sut.Dispose ( ) ;

        service2.Received ( )
                .Dispose ( ) ;
    }

    [ TestMethod ]
    public void Dispose_ForInvoked_DisposesValues ( )
    {
        var sut = new GattServices ( ) ;
        var service1 = Substitute.For< IGattDeviceServiceWrapper > ( ) ;
        var result1 = Substitute.For< IGattCharacteristicsResultWrapper > ( ) ;
        var service2 = Substitute.For< IGattDeviceServiceWrapper > ( ) ;
        var result2 = Substitute.For< IGattCharacteristicsResultWrapper > ( ) ;

        sut [ service1 ] = result1 ;
        sut [ service2 ] = result2 ;

        sut.Dispose ( ) ;

        result1.Received ( )
               .Dispose ( ) ;
        result2.Received ( )
               .Dispose ( ) ;
    }

    [ TestMethod ]
    public void ReadOnlyDictionary_ForInvoked_ContainsService1 ( )
    {
        using var sut = new GattServices ( ) ;

        var service = Substitute.For< IGattDeviceServiceWrapper > ( ) ;
        var result = Substitute.For< IGattCharacteristicsResultWrapper > ( ) ;

        sut [ service ] = result ;

        sut.ReadOnlyDictionary [ service ]
           .Should ( )
           .Be ( result ) ;
    }
}
