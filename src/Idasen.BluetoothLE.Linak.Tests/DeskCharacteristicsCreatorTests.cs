using Idasen.BluetoothLE.Characteristics.Interfaces.Characteristics ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery ;
using Idasen.BluetoothLE.Linak.Interfaces ;
using NSubstitute ;
using Serilog ;
using Serilog.Core ;

namespace Idasen.BluetoothLE.Linak.Tests ;

[ TestClass ]
public class DeskCharacteristicsCreatorTests
{
    private ILogger                    _logger      = null! ;
    private ICharacteristicBaseFactory _baseFactory = null! ;

    [ TestInitialize ]
    public void Setup ( )
    {
        _logger      = Logger.None ;
        _baseFactory = Substitute.For < ICharacteristicBaseFactory > ( ) ;
    }

    private DeskCharacteristicsCreator CreateSut ( )
    {
        return new DeskCharacteristicsCreator ( _logger ,
                                                _baseFactory ) ;
    }
    [ TestMethod ]
    public void Create_ForInvokedWithCharacteristics_AddsGenericAccess ( )
    {
        var sut             = CreateSut ( ) ;
        var characteristics = Substitute.For < IDeskCharacteristics > ( ) ;
        var device          = Substitute.For < IDevice > ( ) ;
        var characteristic  = Substitute.For < IGenericAccess > ( ) ;

        _baseFactory.Create < IGenericAccess > ( device )
                    .Returns ( characteristic ) ;

        characteristics.WithCharacteristics ( Arg.Any < DeskCharacteristicKey > ( ) ,
                                              Arg.Any < ICharacteristicBase > ( ) )
                       .Returns ( characteristics ) ;

        sut.Create ( characteristics ,
                     device ) ;

        characteristics.Received ( )
                       .WithCharacteristics ( DeskCharacteristicKey.GenericAccess ,
                                              characteristic ) ;
    }

    [ TestMethod ]
    public void Create_ForInvokedWithCharacteristics_AddsGenericAttribute ( )
    {
        var sut             = CreateSut ( ) ;
        var characteristics = Substitute.For < IDeskCharacteristics > ( ) ;
        var device          = Substitute.For < IDevice > ( ) ;
        var characteristic  = Substitute.For < IGenericAttributeService > ( ) ;

        _baseFactory.Create < IGenericAttributeService > ( device )
                    .Returns ( characteristic ) ;

        characteristics.WithCharacteristics ( Arg.Any < DeskCharacteristicKey > ( ) ,
                                              Arg.Any < ICharacteristicBase > ( ) )
                       .Returns ( characteristics ) ;

        sut.Create ( characteristics ,
                     device ) ;

        characteristics.Received ( )
                       .WithCharacteristics ( DeskCharacteristicKey.GenericAttribute ,
                                              characteristic ) ;
    }

    [ TestMethod ]
    public void Create_ForInvokedWithCharacteristics_AddsReferenceInput ( )
    {
        var sut             = CreateSut ( ) ;
        var characteristics = Substitute.For < IDeskCharacteristics > ( ) ;
        var device          = Substitute.For < IDevice > ( ) ;
        var characteristic  = Substitute.For < IReferenceInput > ( ) ;

        _baseFactory.Create < IReferenceInput > ( device )
                    .Returns ( characteristic ) ;

        characteristics.WithCharacteristics ( Arg.Any < DeskCharacteristicKey > ( ) ,
                                              Arg.Any < ICharacteristicBase > ( ) )
                       .Returns ( characteristics ) ;

        sut.Create ( characteristics ,
                     device ) ;

        characteristics.Received ( )
                       .WithCharacteristics ( DeskCharacteristicKey.ReferenceInput ,
                                              characteristic ) ;
    }

    [ TestMethod ]
    public void Create_ForInvokedWithCharacteristics_AddsReferenceOutput ( )
    {
        var sut             = CreateSut ( ) ;
        var characteristics = Substitute.For < IDeskCharacteristics > ( ) ;
        var device          = Substitute.For < IDevice > ( ) ;
        var characteristic  = Substitute.For < IReferenceOutput > ( ) ;

        _baseFactory.Create < IReferenceOutput > ( device )
                    .Returns ( characteristic ) ;

        characteristics.WithCharacteristics ( Arg.Any < DeskCharacteristicKey > ( ) ,
                                              Arg.Any < ICharacteristicBase > ( ) )
                       .Returns ( characteristics ) ;

        sut.Create ( characteristics ,
                     device ) ;

        characteristics.Received ( )
                       .WithCharacteristics ( DeskCharacteristicKey.ReferenceOutput ,
                                              characteristic ) ;
    }

    [ TestMethod ]
    public void Create_ForInvokedWithCharacteristics_AddsDpg ( )
    {
        var sut             = CreateSut ( ) ;
        var characteristics = Substitute.For < IDeskCharacteristics > ( ) ;
        var device          = Substitute.For < IDevice > ( ) ;
        var characteristic  = Substitute.For < IDpg > ( ) ;

        _baseFactory.Create < IDpg > ( device )
                    .Returns ( characteristic ) ;

        characteristics.WithCharacteristics ( Arg.Any < DeskCharacteristicKey > ( ) ,
                                              Arg.Any < ICharacteristicBase > ( ) )
                       .Returns ( characteristics ) ;

        sut.Create ( characteristics ,
                     device ) ;

        characteristics.Received ( )
                       .WithCharacteristics ( DeskCharacteristicKey.Dpg ,
                                              characteristic ) ;
    }

    [ TestMethod ]
    public void Create_ForInvokedWithCharacteristics_AddsControl ( )
    {
        var sut             = CreateSut ( ) ;
        var characteristics = Substitute.For < IDeskCharacteristics > ( ) ;
        var device          = Substitute.For < IDevice > ( ) ;
        var characteristic  = Substitute.For < IControl > ( ) ;

        _baseFactory.Create < IControl > ( device )
                    .Returns ( characteristic ) ;

        characteristics.WithCharacteristics ( Arg.Any < DeskCharacteristicKey > ( ) ,
                                              Arg.Any < ICharacteristicBase > ( ) )
                       .Returns ( characteristics ) ;

        sut.Create ( characteristics ,
                     device ) ;

        characteristics.Received ( )
                       .WithCharacteristics ( DeskCharacteristicKey.Control ,
                                              characteristic ) ;
    }
}
