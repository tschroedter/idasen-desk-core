using Windows.Devices.Bluetooth.GenericAttributeProfile ;
using Windows.Storage.Streams ;
using FluentAssertions ;
using Idasen.BluetoothLE.Characteristics.Common ;
using Idasen.BluetoothLE.Common.Tests ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery.Wrappers ;
using Idasen.BluetoothLE.Core.ServicesDiscovery.Wrappers ;
using NSubstitute ;

namespace Idasen.BluetoothLE.Characteristics.Tests.Common ;

[ TestClass ]
public class RawValueWriterTests
{
    [ TestMethod ]
    public async Task TryWriteValueAsync_ForCharacteristicIsNull_Throws ( )
    {
        var sut = new RawValueWriter ( ) ;
        var buffer = Substitute.For< IBuffer > ( ) ;

        var action = async ( ) =>
                     {
                         await sut.TryWriteValueAsync ( null! ,
                                                        buffer ) ;
                     } ;

        await action.Should ( )
                    .ThrowAsync < ArgumentNullException > ( )
                    .WithParameter ( "characteristic" ) ;
    }

    private static async Task< bool > CallTryWriteValueAsync ( GattCharacteristicProperties properties ,
                                                              GattCommunicationStatus    status )
    {
        var sut = new RawValueWriter ( ) ;
        var characteristic = Substitute.For< IGattCharacteristicWrapper > ( ) ;
        var buffer = Substitute.For< IBuffer > ( ) ;

        characteristic.WithCharacteristicProperties ( properties )
                      .WithWriteValueAsyncResult ( status ) ;

        return await sut.TryWriteValueAsync ( characteristic ,
                                              buffer ) ;
    }

    private static async Task< bool > CallTryWritableAuxiliariesValueAsync ( GattCharacteristicProperties properties ,
                                                                              GattCommunicationStatus    status )
    {
        var sut = new RawValueWriter ( ) ;
        var characteristic = Substitute.For< IGattCharacteristicWrapper > ( ) ;
        var buffer = Substitute.For< IBuffer > ( ) ;

        characteristic.WithCharacteristicProperties ( properties )
                      .WithWriteValueAsyncResult ( status ) ;

        return await sut.TryWritableAuxiliariesValueAsync ( characteristic ,
                                                            buffer ) ;
    }

    [ TestMethod ]
    public async Task TryWriteValueAsync_ForBufferIsNull_Throws ( )
    {
        var sut = new RawValueWriter ( ) ;
        var characteristic = Substitute.For< IGattCharacteristicWrapper > ( ) ;

        var action = async ( ) =>
                     {
                         await sut.TryWriteValueAsync ( characteristic ,
                                                        null! ) ;
                     } ;

        await action.Should ( )
                    .ThrowAsync < ArgumentNullException > ( )
                    .WithParameter ( "buffer" ) ;
    }

    [ TestMethod ]
    public async Task TryWriteValueAsync_ForGattCommunicationStatusIsSuccess_True ( )
    {
        var actual = await CallTryWriteValueAsync ( GattCharacteristicProperties.Write ,
                                                    GattCommunicationStatus.Success ) ;

        actual.Should ( )
              .BeTrue ( ) ;
    }

    [ TestMethod ]
    public async Task TryWriteValueAsync_ForGattCommunicationStatusIsUnreachable_False ( )
    {
        var actual = await CallTryWriteValueAsync ( GattCharacteristicProperties.Write ,
                                                    GattCommunicationStatus.Unreachable ) ;

        actual.Should ( )
              .BeFalse ( ) ;
    }

    [ TestMethod ]
    public async Task TryWriteValueAsync_ForGattCommunicationStatusIsProtocolError_False ( )
    {
        var actual = await CallTryWriteValueAsync ( GattCharacteristicProperties.Write ,
                                                    GattCommunicationStatus.ProtocolError ) ;

        actual.Should ( )
              .BeFalse ( ) ;
    }

    [ TestMethod ]
    public async Task TryWriteValueAsync_ForGattCommunicationStatusIsAccessDenied_False ( )
    {
        var actual = await CallTryWriteValueAsync ( GattCharacteristicProperties.Write ,
                                                    GattCommunicationStatus.AccessDenied ) ;

        actual.Should ( )
              .BeFalse ( ) ;
    }

    [ TestMethod ]
    public async Task TryWriteValueAsync_ForCharacteristicDoesNotSupportWrite_False ( )
    {
        var actual = await CallTryWriteValueAsync ( GattCharacteristicProperties.None ,
                                                    GattCommunicationStatus.AccessDenied ) ;

        actual.Should ( )
              .BeFalse ( ) ;
    }

    [ TestMethod ]
    public async Task TryWriteValueAsync_ForCharacteristicDoesNotSupportWrite_DoesNotCallTryWriteValueAsync ( )
    {
        await AssertDoesNotCallWriteAsync ( ( s , c , b ) => s.TryWriteValueAsync ( c ,
                                                                                  b ) ,
                                           GattCharacteristicProperties.None ,
                                           verifyWithResult : false ) ;
    }

    [ TestMethod ]
    public async Task TryWritableAuxiliariesValueAsync_ForCharacteristicIsNull_Throws ( )
    {
        var sut = new RawValueWriter ( ) ;
        var buffer = Substitute.For< IBuffer > ( ) ;

        var action = async ( ) =>
                     {
                         await sut.TryWritableAuxiliariesValueAsync ( null! ,
                                                                      buffer ) ;
                     } ;

        await action.Should ( )
                    .ThrowAsync < ArgumentNullException > ( )
                    .WithParameter ( "characteristic" ) ;
    }

    [ TestMethod ]
    public async Task TryWritableAuxiliariesValueAsync_ForBufferIsNull_Throws ( )
    {
        var sut = new RawValueWriter ( ) ;
        var characteristic = Substitute.For< IGattCharacteristicWrapper > ( ) ;

        var action = async ( ) =>
                     {
                         await sut.TryWritableAuxiliariesValueAsync ( characteristic ,
                                                                      null! ) ;
                     } ;

        await action.Should ( )
                    .ThrowAsync < ArgumentNullException > ( )
                    .WithParameter ( "buffer" ) ;
    }

    [ TestMethod ]
    public async Task TryWritableAuxiliariesValueAsync_ForGattCommunicationStatusIsSuccess_True ( )
    {
        var actual = await CallTryWritableAuxiliariesValueAsync ( GattCharacteristicProperties.WritableAuxiliaries ,
                                                                  GattCommunicationStatus.Success ) ;

        actual.Should ( )
              .BeTrue ( ) ;
    }

    [ TestMethod ]
    public async Task TryWritableAuxiliariesValueAsync_ForGattCommunicationStatusIsUnreachable_False ( )
    {
        var actual = await CallTryWritableAuxiliariesValueAsync ( GattCharacteristicProperties.WritableAuxiliaries ,
                                                                  GattCommunicationStatus.Unreachable ) ;

        actual.Should ( )
              .BeFalse ( ) ;
    }

    [ TestMethod ]
    public async Task TryWritableAuxiliariesValueAsync_ForGattCommunicationStatusIsProtocolError_False ( )
    {
        var actual = await CallTryWritableAuxiliariesValueAsync ( GattCharacteristicProperties.WritableAuxiliaries ,
                                                                  GattCommunicationStatus.ProtocolError ) ;

        actual.Should ( )
              .BeFalse ( ) ;
    }

    [ TestMethod ]
    public async Task TryWritableAuxiliariesValueAsync_ForGattCommunicationStatusIsAccessDenied_False ( )
    {
        var actual = await CallTryWritableAuxiliariesValueAsync ( GattCharacteristicProperties.WritableAuxiliaries ,
                                                                  GattCommunicationStatus.AccessDenied ) ;

        actual.Should ( )
              .BeFalse ( ) ;
    }

    [ TestMethod ]
    public async Task TryWritableAuxiliariesValueAsync_ForCharacteristicDoesNotSupportWrite_False ( )
    {
        var actual = await CallTryWritableAuxiliariesValueAsync ( GattCharacteristicProperties.None ,
                                                                  GattCommunicationStatus.AccessDenied ) ;

        actual.Should ( )
              .BeFalse ( ) ;
    }

    [ TestMethod ]
    public async Task TryWritableAuxiliariesValueAsync_ForCharacteristicDoesNotSupportWritableAuxiliaries_DoesNotCallTryWriteValueAsync ( )
    {
        var sut = new RawValueWriter ( ) ;
        var characteristic = Substitute.For< IGattCharacteristicWrapper > ( ) ;
        var buffer = Substitute.For< IBuffer > ( ) ;
        characteristic.WithCharacteristicProperties ( GattCharacteristicProperties.None ) ;

        await sut.TryWriteValueAsync ( characteristic ,
                                       buffer ) ;

        await characteristic.DidNotReceive ( )
                            .WriteValueAsync ( buffer ) ;
    }

    [ TestMethod ]
    public async Task TryWriteWithoutResponseAsync_ForCharacteristicIsNull_Throws ( )
    {
        var sut = new RawValueWriter ( ) ;
        var buffer = Substitute.For< IBuffer > ( ) ;

        var action = async ( ) =>
                     {
                         await sut.TryWriteWithoutResponseAsync ( null! ,
                                                                  buffer ) ;
                     } ;

        await action.Should ( )
                    .ThrowAsync < ArgumentNullException > ( )
                    .WithParameter ( "characteristic" ) ;
    }

    [ TestMethod ]
    public async Task TryWriteWithoutResponseAsync_ForBufferIsNull_Throws ( )
    {
        var sut = new RawValueWriter ( ) ;
        var characteristic = Substitute.For< IGattCharacteristicWrapper > ( ) ;

        var action = async ( ) =>
                     {
                         await sut.TryWriteWithoutResponseAsync ( characteristic ,
                                                                  null! ) ;
                     } ;

        await action.Should ( )
                    .ThrowAsync < ArgumentNullException > ( )
                    .WithParameter ( "buffer" ) ;
    }

    [ TestMethod ]
    public async Task TryWriteWithoutResponseAsync_ReturnsProvidedResult_ForVariousStatuses ( )
    {
        var result = Substitute.For< IGattWriteResultWrapper > ( ) ;

        foreach ( var status in new[] { GattCommunicationStatus.Success ,
                                        GattCommunicationStatus.Unreachable ,
                                        GattCommunicationStatus.ProtocolError ,
                                        GattCommunicationStatus.AccessDenied } )
        {
            result.Status.Returns ( status ) ;

            var actual = await CallTryWriteWithoutResponseAsync ( GattCharacteristicProperties.WriteWithoutResponse ,
                                                                 result ) ;

            actual.Should ( )
                  .Be ( result ) ;
        }
    }

    [ TestMethod ]
    public async Task TryWriteWithoutResponseAsync_ForCharacteristicDoesNotWriteWithoutResponse_GattWriteResultNotSupported ( )
    {
        var sut = new RawValueWriter ( ) ;
        var characteristic = Substitute.For< IGattCharacteristicWrapper > ( ) ;
        var buffer = Substitute.For< IBuffer > ( ) ;

        characteristic.WithCharacteristicProperties ( GattCharacteristicProperties.None ) ;

        var actual = await sut.TryWriteWithoutResponseAsync ( characteristic ,
                                                              buffer ) ;
        actual.Should ( )
              .Be ( GattWriteResultWrapper.NotSupported ) ;
    }

    [ TestMethod ]
    public async Task TryWriteWithoutResponseAsync_ForCharacteristicDoesNotSupportWrite_DoesNotCallTryWriteValueAsync ( )
    {
        await AssertDoesNotCallWriteAsync ( ( s , c , b ) => s.TryWriteWithoutResponseAsync ( c ,
                                                                                          b ) ,
                                           GattCharacteristicProperties.None ,
                                           verifyWithResult : true ) ;
    }

    private static async Task AssertDoesNotCallWriteAsync (
        Func< RawValueWriter , IGattCharacteristicWrapper , IBuffer , Task > invoke ,
        GattCharacteristicProperties properties ,
        bool                        verifyWithResult )
    {
        var sut = new RawValueWriter ( ) ;
        var characteristic = Substitute.For< IGattCharacteristicWrapper > ( ) ;
        var buffer = Substitute.For< IBuffer > ( ) ;

        characteristic.WithCharacteristicProperties ( properties ) ;

        await invoke ( sut ,
                       characteristic ,
                       buffer ) ;

        if ( verifyWithResult )
            await characteristic.DidNotReceive ( )
                                .WriteValueWithResultAsync ( buffer ) ;
        else
            await characteristic.DidNotReceive ( )
                                .WriteValueAsync ( buffer ) ;
    }

    private static async Task< IGattWriteResultWrapper > CallTryWriteWithoutResponseAsync (
        GattCharacteristicProperties properties ,
        IGattWriteResultWrapper        result )
    {
        var sut = new RawValueWriter ( ) ;
        var characteristic = Substitute.For< IGattCharacteristicWrapper > ( ) ;
        var buffer = Substitute.For< IBuffer > ( ) ;

        characteristic.WithCharacteristicProperties ( properties )
                      .WithWriteValueWithResultAsync ( result ) ;

        return await sut.TryWriteWithoutResponseAsync ( characteristic ,
                                                       buffer ) ;
    }
}
