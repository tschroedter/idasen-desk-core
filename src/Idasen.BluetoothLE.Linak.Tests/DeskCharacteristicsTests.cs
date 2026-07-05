using FluentAssertions ;
using FluentAssertions.Execution ;
using Idasen.BluetoothLE.Characteristics.Interfaces.Characteristics ;
using Idasen.BluetoothLE.Common.Tests ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery ;
using Idasen.BluetoothLE.Linak.Interfaces ;
using NSubstitute ;
using Serilog.Core ;

namespace Idasen.BluetoothLE.Linak.Tests ;

[ TestClass ]
public class DeskCharacteristicsTests
{
    [ TestMethod ]
    public async Task Refresh_ForInvoked_CallsCharacteristicRefresh ( )
    {
        var genericAccess           = Substitute.For < IGenericAccess > ( ) ;
        var genericAttributeService = Substitute.For < IGenericAttributeService > ( ) ;
        var sut                     = CreateSut ( ) ;

        sut.WithCharacteristics ( DeskCharacteristicKey.GenericAccess ,
                                  genericAccess ) ;
        sut.WithCharacteristics ( DeskCharacteristicKey.GenericAttribute ,
                                  genericAttributeService ) ;

        await sut.Refresh ( ) ;

        using var scope = new AssertionScope ( ) ;

        await genericAccess.Received ( )
                           .Refresh ( ) ;

        await genericAttributeService.Received ( )
                                     .Refresh ( ) ;
    }

    [ TestMethod ]
    public void Initialize_ForInvoked_CallsCreator ( )
    {
        var creator = Substitute.For < IDeskCharacteristicsCreator > ( ) ;
        var device  = Substitute.For < IDevice > ( ) ;
        var sut     = CreateSut ( creator ) ;

        sut.Initialize ( device ) ;

        creator.Received ( )
               .Create ( sut ,
                         device ) ;
    }

    [ TestMethod ]
    public void Initialize_ForDeviceIsNull_Throws ( )
    {
        var sut = CreateSut ( ) ;

        Action action = ( ) => sut.Initialize ( null! ) ;

        action.Should ( )
              .Throw < ArgumentNullException > ( )
              .WithParameter ( "device" ) ;
    }

    [ TestMethod ]
    public void WithCharacteristics_ForCharacteristicIsNull_Throws ( )
    {
        var sut = CreateSut ( ) ;

        Action action = ( ) => sut.WithCharacteristics ( DeskCharacteristicKey.GenericAccess ,
                                                         null! ) ;

        action.Should ( )
              .Throw < ArgumentNullException > ( )
              .WithParameter ( "characteristic" ) ;
    }

    [ TestMethod ]
    public void WithCharacteristics_ForCharacteristic_AddsCharacteristic ( )
    {
        var genericAccess = Substitute.For < IGenericAccess > ( ) ;
        var sut           = CreateSut ( ) ;

        sut.WithCharacteristics ( DeskCharacteristicKey.GenericAccess ,
                                  genericAccess ) ;

        sut.Characteristics
           .Should ( )
           .Contain ( x => x.Key   == DeskCharacteristicKey.GenericAccess &&
                           x.Value == genericAccess ) ;
    }

    [ TestMethod ]
    public void ToString_ForInvoked_Instance ( )
    {
        var genericAccess           = Substitute.For < IGenericAccess > ( ) ;
        var genericAttributeService = Substitute.For < IGenericAttributeService > ( ) ;
        var sut                     = CreateSut ( ) ;

        sut.WithCharacteristics ( DeskCharacteristicKey.GenericAccess ,
                                  genericAccess ) ;
        sut.WithCharacteristics ( DeskCharacteristicKey.GenericAttribute ,
                                  genericAttributeService ) ;

        using var scope = new AssertionScope ( ) ;

        sut.ToString ( )
           .Should ( )
           .Contain ( "GenericAccess" ) ;

        sut.ToString ( )
           .Should ( )
           .Contain ( "GenericAttributeService" ) ;
    }

    private static DeskCharacteristics CreateSut ( IDeskCharacteristicsCreator? creator = null )
    {
        var logger = Logger.None ;

        creator ??= Substitute.For < IDeskCharacteristicsCreator > ( ) ;

        return new DeskCharacteristics ( logger ,
                                         creator ) ;
    }
}
