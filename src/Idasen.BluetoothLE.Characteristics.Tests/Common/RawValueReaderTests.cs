using System.Collections ;
using Windows.Devices.Bluetooth.GenericAttributeProfile ;
using Windows.Storage.Streams ;
using FluentAssertions ;
using Idasen.BluetoothLE.Characteristics.Common ;
using Idasen.BluetoothLE.Characteristics.Interfaces.Common ;
using Idasen.BluetoothLE.Common.Tests ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery.Wrappers ;
using NSubstitute ;
using Serilog ;
using Serilog.Core ;

namespace Idasen.BluetoothLE.Characteristics.Tests.Common ;

[ TestClass ]
public class RawValueReaderTests
{
    private ILogger       _logger = null! ;
    private IBufferReader _reader = null! ;

    [ TestInitialize ]
    public void Setup ( )
    {
        _logger = Logger.None ;
        _reader = Substitute.For < IBufferReader > ( ) ;
    }

    private RawValueReader CreateSut ( )
    {
        return new RawValueReader ( _logger ,
                                    _reader ) ;
    }
    [ TestMethod ]
    public async Task TryReadValueAsync_ForCharacteristicIsNull_Throws ( )
    {
        var sut = CreateSut ( ) ;

        var action = async ( ) => { await sut.TryReadValueAsync ( null! ) ; } ;

        await action.Should ( )
                    .ThrowAsync < ArgumentNullException > ( )
                    .WithParameter ( "characteristic" ) ;
    }

    [ TestMethod ]
    public async Task TryReadValueAsync_ForSupportsNotifyTrue_False ( )
    {
        var sut            = CreateSut ( ) ;
        var result         = Substitute.For < IGattReadResultWrapper > ( ) ;
        var characteristic = Substitute.For < IGattCharacteristicWrapper > ( ) ;

        WithTryReadValueResult ( _reader ,
                                 Array.Empty < byte > ( ) ) ;

        result.Status
              .Returns ( GattCommunicationStatus.Success ) ;

        characteristic.WithCharacteristicProperties ( GattCharacteristicProperties.Notify )
                      .WithReadValueAsyncResult ( result ) ;

        var (success , _) = await sut.TryReadValueAsync ( characteristic ) ;

        success.Should ( )
               .BeFalse ( ) ;
    }

    [ TestMethod ]
    public async Task TryReadValueAsync_ForSupportsNotifyTrue_Empty ( )
    {
        var sut            = CreateSut ( ) ;
        var result         = Substitute.For < IGattReadResultWrapper > ( ) ;
        var characteristic = Substitute.For < IGattCharacteristicWrapper > ( ) ;

        WithTryReadValueResult ( _reader ,
                                 Array.Empty < byte > ( ) ) ;

        result.Status
              .Returns ( GattCommunicationStatus.Success ) ;

        characteristic.WithCharacteristicProperties ( GattCharacteristicProperties.Notify )
                      .WithReadValueAsyncResult ( result ) ;

        var (_ , bytes) = await sut.TryReadValueAsync ( characteristic ) ;

        bytes.Should ( )
             .BeEmpty ( ) ;
    }

    [ TestMethod ]
    public async Task TryReadValueAsync_ForNotSupportingRead_False ( )
    {
        var sut            = CreateSut ( ) ;
        var result         = Substitute.For < IGattReadResultWrapper > ( ) ;
        var characteristic = Substitute.For < IGattCharacteristicWrapper > ( ) ;

        WithTryReadValueResult ( _reader ,
                                 Array.Empty < byte > ( ) ) ;

        result.Status
              .Returns ( GattCommunicationStatus.Success ) ;

        characteristic.WithCharacteristicProperties ( GattCharacteristicProperties.None )
                      .WithReadValueAsyncResult ( result ) ;

        var (success , _) = await sut.TryReadValueAsync ( characteristic ) ;

        success.Should ( )
               .BeFalse ( ) ;
    }

    [ TestMethod ]
    public async Task TryReadValueAsync_ForNotSupportingRead_Empty ( )
    {
        var sut            = CreateSut ( ) ;
        var result         = Substitute.For < IGattReadResultWrapper > ( ) ;
        var characteristic = Substitute.For < IGattCharacteristicWrapper > ( ) ;

        WithTryReadValueResult ( _reader ,
                                 Array.Empty < byte > ( ) ) ;

        result.Status
              .Returns ( GattCommunicationStatus.Success ) ;

        characteristic.WithCharacteristicProperties ( GattCharacteristicProperties.None )
                      .WithReadValueAsyncResult ( result ) ;

        var (_ , bytes) = await sut.TryReadValueAsync ( characteristic ) ;

        bytes.Should ( )
             .BeEmpty ( ) ;
    }

    [ TestMethod ]
    public async Task TryReadValueAsync_ForGattCommunicationStatusIsSuccess_True ( )
    {
        var sut            = CreateSut ( ) ;
        var result         = Substitute.For < IGattReadResultWrapper > ( ) ;
        var characteristic = Substitute.For < IGattCharacteristicWrapper > ( ) ;

        WithTryReadValueResult ( _reader ,
                                 Array.Empty < byte > ( ) ) ;

        result.Status
              .Returns ( GattCommunicationStatus.Success ) ;

        characteristic.WithCharacteristicProperties ( GattCharacteristicProperties.Read )
                      .WithReadValueAsyncResult ( result ) ;

        var (success , _) = await sut.TryReadValueAsync ( characteristic ) ;

        success.Should ( )
               .BeTrue ( ) ;
    }

    [ TestMethod ]
    public async Task TryReadValueAsync_ForGattCommunicationStatusIsSuccess_Bytes ( )
    {
        var sut            = CreateSut ( ) ;
        var result         = Substitute.For < IGattReadResultWrapper > ( ) ;
        var characteristic = Substitute.For < IGattCharacteristicWrapper > ( ) ;
        var bytes          = new byte [ ] { 1 , 2 , 3 , 4 , 5 } ;

        WithTryReadValueResult ( _reader ,
                                 bytes ) ;

        result.Status
              .Returns ( GattCommunicationStatus.Success ) ;

        characteristic.WithCharacteristicProperties ( GattCharacteristicProperties.Read )
                      .WithReadValueAsyncResult ( result ) ;

        var (_ , value) = await sut.TryReadValueAsync ( characteristic ) ;

        value.Should ( )
             .BeEquivalentTo ( bytes ) ;
    }

    [ TestMethod ]
    public async Task TryReadValueAsync_ForGattCommunicationStatusIsNotSuccess_False ( )
    {
        var sut            = CreateSut ( ) ;
        var result         = Substitute.For < IGattReadResultWrapper > ( ) ;
        var characteristic = Substitute.For < IGattCharacteristicWrapper > ( ) ;

        WithTryReadValueResult ( _reader ,
                                 Array.Empty < byte > ( ) ) ;

        result.Status
              .Returns ( GattCommunicationStatus.Unreachable ) ;

        characteristic.WithCharacteristicProperties ( GattCharacteristicProperties.Read )
                      .WithReadValueAsyncResult ( result ) ;

        var (success , _) = await sut.TryReadValueAsync ( characteristic ) ;

        success.Should ( )
               .BeFalse ( ) ;
    }

    [ TestMethod ]
    public async Task TryReadValueAsync_ForGattCommunicationStatusIsNotSuccess_Empty ( )
    {
        var sut            = CreateSut ( ) ;
        var result         = Substitute.For < IGattReadResultWrapper > ( ) ;
        var characteristic = Substitute.For < IGattCharacteristicWrapper > ( ) ;

        WithTryReadValueResult ( _reader ,
                                 Array.Empty < byte > ( ) ) ;

        result.Status
              .Returns ( GattCommunicationStatus.Success ) ;

        characteristic.WithCharacteristicProperties ( GattCharacteristicProperties.Read )
                      .WithReadValueAsyncResult ( result ) ;

        var (_ , value) = await sut.TryReadValueAsync ( characteristic ) ;

        value.Should ( )
             .BeEmpty ( ) ;
    }

    [ TestMethod ]
    public async Task TryReadValueAsync_ForGattCommunicationStatus_SetsProtocolError ( )
    {
        var sut            = CreateSut ( ) ;
        var result         = Substitute.For < IGattReadResultWrapper > ( ) ;
        var characteristic = Substitute.For < IGattCharacteristicWrapper > ( ) ;
        var protocolError  = ( byte ) 42 ;

        WithTryReadValueResult ( _reader ,
                                 Array.Empty < byte > ( ) ) ;

        result.Status
              .Returns ( GattCommunicationStatus.Success ) ;

        result.ProtocolError
              .Returns ( protocolError ) ;

        characteristic.WithCharacteristicProperties ( GattCharacteristicProperties.Read )
                      .WithReadValueAsyncResult ( result ) ;

        _ = await sut.TryReadValueAsync ( characteristic ) ;

        sut.ProtocolError
           .Should ( )
           .Be ( protocolError ) ;
    }

    private static void WithTryReadValueResult ( IBufferReader reader ,
                                                 IEnumerable   bytes )
    {
        reader.TryReadValue ( Arg.Any < IBuffer > ( ) ,
                              out _ )
              .Returns ( x =>
                         {
                             x [ 1 ] = bytes ;

                             return true ;
                         } ) ;
    }
}
