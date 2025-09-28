namespace Idasen.BluetoothLE.Characteristics.Tests.Characteristics ;

using Windows.Storage.Streams ;
using BluetoothLE.Characteristics.Characteristics ;
using BluetoothLE.Common.Tests ;
using Core.Interfaces.ServicesDiscovery ;
using Core.Interfaces.ServicesDiscovery.Wrappers ;
using FluentAssertions ;
using NSubstitute ;
using Selkie.AutoMocking ;

[ TestClass ]
public class CharacteristicBaseTest
    : CharacteristicBaseTests < TestCharacteristicBase >
{
    protected readonly Guid GattCharacteristicUuid = Guid.Parse ( "22222222-2222-2222-2222-222222222222" ) ;
    protected readonly Guid GattServiceUuid = Guid.Parse ( "11111111-1111-1111-1111-111111111111" ) ;

    [ TestMethod ]
    public void GattServiceUuid_ForInvoked_Uuid ( )
    {
        CreateSut ( ).GattServiceUuid
                     .Should ( )
                     .Be ( GattServiceUuid ) ;
    }

    [ AutoDataTestMethod ]
    public void Initialize_ForUnknownGattServiceUuid_Throws (
        TestCharacteristicBase sut ,
        [ Freeze ] IDevice device )
    {
        IReadOnlyDictionary < IGattDeviceServiceWrapper , IGattCharacteristicsResultWrapper > gattServices =
            new Dictionary < IGattDeviceServiceWrapper , IGattCharacteristicsResultWrapper > ( ) ;

        device.GattServices
              .Returns ( gattServices ) ;

        Action action = ( ) => sut.Initialize < TestCharacteristicBase > ( ) ;

        action.Should ( )
              .Throw < ArgumentException > ( )
              .WithParameter ( "GattServiceUuid" ) ;
    }

    [ TestMethod ]
    public void Initialize_ForKnownGattServiceUuid_AddsKeyToDescriptionToUuid ( )
    {
        TestCharacteristicBase sut = CreateSut ( ) ;

        ServiceWrapper.Uuid
                      .Returns ( sut.GattServiceUuid ) ;

        sut.Initialize < TestCharacteristicBase > ( )
           .DescriptionToUuid[TestCharacteristicBase.RawValueKey]
           .Should ( )
           .Be ( GattCharacteristicUuid ) ;
    }

    [ TestMethod ]
    public void RawDpg_ForNotRefreshedAndInvoked_EmptyBytes ( )
    {
        TestCharacteristicBase sut = CreateSut ( ) ;

        ServiceWrapper.Uuid
                      .Returns ( sut.GattServiceUuid ) ;

        sut.Initialize < TestCharacteristicBase > ( ) ;

        sut.RawValue
           .Should ( )
           .BeEquivalentTo ( CharacteristicBase.RawArrayEmpty ) ;
    }

    [ TestMethod ]
    public async Task RawDpg_ForRefreshedAndInvoked_Bytes ( )
    {
        TestCharacteristicBase sut = CreateSut ( ) ;

        ServiceWrapper.Uuid
                      .Returns ( sut.GattServiceUuid ) ;

        await sut.Initialize < TestCharacteristicBase > ( )
                 .Refresh ( ) ;

        sut.RawValue
           .Should ( )
           .BeEquivalentTo ( RawValue1 ) ;
    }

    [ TestMethod ]
    public async Task Refresh_ForSuccessfulRead_UpdatesRawValuesAsync ( )
    {
        TestCharacteristicBase sut = CreateSut ( ) ;

        ServiceWrapper.Uuid
                      .Returns ( sut.GattServiceUuid ) ;

        RawValueReader.TryReadValueAsync ( CharacteristicWrapper1 )
                      .Returns ( ( true , RawValue1 ) ) ;

        await sut.Initialize < TestCharacteristicBase > ( )
                 .Refresh ( ) ;

        sut.RawValue
           .Should ( )
           .BeEquivalentTo ( RawValue1 ) ;
    }

    [ TestMethod ]
    public async Task Refresh_ForFailedRead_UpdatesRawValuesAsync ( )
    {
        TestCharacteristicBase sut = CreateSut ( ) ;

        ServiceWrapper.Uuid
                      .Returns ( sut.GattServiceUuid ) ;

        RawValueReader.TryReadValueAsync ( CharacteristicWrapper1 )
                      .Returns ( ( false , RawValue1 ) ) ;

        await sut.Initialize < TestCharacteristicBase > ( )
                 .Refresh ( ) ;

        sut.RawValue
           .Should ( )
           .BeEquivalentTo ( CharacteristicBase.RawArrayEmpty ) ;
    }

    [ TestMethod ]
    public async Task TryWriteRawValue_ForKnownCharacteristics_WritesRawValuesAsync ( )
    {
        TestCharacteristicBase sut = CreateSut ( ) ;

        ServiceWrapper.Uuid
                      .Returns ( sut.GattServiceUuid ) ;

        await sut.Initialize < TestCharacteristicBase > ( )
                 .Refresh ( ) ;

        await sut.TryWriteRawValue ( RawValue1 ) ;

        await RawValueWriter.Received ( )
                            .TryWriteValueAsync ( CharacteristicWrapper1 ,
                                                  Arg.Is < IBuffer > ( x => x.Length == RawValue1.Length ) ) ;
    }

    [ TestMethod ]
    public async Task TryWriteRawValue_ForKnownCharacteristics_ReturnsTrue ( )
    {
        TestCharacteristicBase sut = CreateSut ( ) ;

        ServiceWrapper.Uuid
                      .Returns ( sut.GattServiceUuid ) ;

        RawValueWriter.TryWriteValueAsync ( Arg.Any < IGattCharacteristicWrapper > ( ) ,
                                            Arg.Any < IBuffer > ( ) )
                      .Returns ( Task.FromResult ( true ) ) ;

        await sut.Initialize < TestCharacteristicBase > ( )
                 .Refresh ( ) ;

        sut.TryWriteRawValue ( RawValue1 )
           .Result
           .Should ( )
           .Be ( true ) ;
    }

    [ TestMethod ]
    public async Task TryWriteRawValue_ForKnownCharacteristicsAndFailedToWrite_ReturnsFalse ( )
    {
        TestCharacteristicBase sut = CreateSut ( ) ;

        ServiceWrapper.Uuid
                      .Returns ( sut.GattServiceUuid ) ;

        RawValueWriter.TryWriteValueAsync ( Arg.Any < IGattCharacteristicWrapper > ( ) ,
                                            Arg.Any < IBuffer > ( ) )
                      .Returns ( Task.FromResult ( false ) ) ;

        await sut.Initialize < TestCharacteristicBase > ( )
                 .Refresh ( ) ;

        sut.TryWriteRawValue ( RawValue1 )
           .Result
           .Should ( )
           .Be ( false ) ;
    }

    [ TestMethod ]
    public async Task TryWriteRawValue_ForUnknownCharacteristics_ReturnsFalse ( )
    {
        Wrappers.Clear ( ) ;

        TestCharacteristicBase sut = CreateSut ( ) ;

        ServiceWrapper.Uuid
                      .Returns ( sut.GattServiceUuid ) ;

        await sut.Initialize < TestCharacteristicBase > ( )
                 .Refresh ( ) ;

        sut.TryWriteRawValue ( RawValue1 )
           .Result
           .Should ( )
           .Be ( false ) ;
    }

    [ TestMethod ]
    public async Task TryWriteRawValue_ForUnknownCharacteristics_LogsError ( )
    {
        Wrappers.Clear ( ) ;

        TestCharacteristicBase sut = CreateSut ( ) ;

        ServiceWrapper.Uuid
                      .Returns ( sut.GattServiceUuid ) ;

        await sut.Initialize < TestCharacteristicBase > ( )
                 .Refresh ( ) ;

        await sut.TryWriteRawValue ( RawValue1 ) ;

        // Accept Serilog's generic Error<T>(string, T) overload
        Logger.Received ( )
              .Error ( Arg.Is < string > ( s => s.Contains ( "Unknown characteristic" ,
                                                             StringComparison.OrdinalIgnoreCase ) ) ,
                       Arg.Any < string > ( ) ) ;
    }

    [ TestMethod ]
    [ Ignore ( "Existing Characteristic will always return a value" ) ]
    public async Task TryWriteRawValue_ForKnownCharacteristicsIsNull_ReturnsFalse ( )
    {
        Wrappers.Clear ( ) ;

        Wrappers.Add ( TestCharacteristicBase.RawValueKey ,
                       null! ) ;

        TestCharacteristicBase sut = CreateSut ( ) ;

        ServiceWrapper.Uuid
                      .Returns ( sut.GattServiceUuid ) ;

        await sut.Initialize < TestCharacteristicBase > ( )
                 .Refresh ( ) ;

        sut.TryWriteRawValue ( RawValue1 )
           .Result
           .Should ( )
           .Be ( false ) ;
    }

    [ TestMethod ]
    [ Ignore ( "Existing Characteristic will always return a value" ) ]
    public async Task TryWriteRawValue_ForKnownCharacteristicsIsNull_LogsError ( )
    {
        Wrappers.Clear ( ) ;

        Wrappers.Add ( TestCharacteristicBase.RawValueKey ,
                       null! ) ;

        TestCharacteristicBase sut = CreateSut ( ) ;

        ServiceWrapper.Uuid
                      .Returns ( sut.GattServiceUuid ) ;

        await sut.Initialize < TestCharacteristicBase > ( )
                 .Refresh ( ) ;

        await sut.TryWriteRawValue ( RawValue1 ) ;

        Logger.ReceivedWithAnyArgs ( )
              .Error ( Arg.Any < string > ( ) ) ;
    }

    [ TestMethod ]
    public async Task ToString_ForInvoke_Instance ( )
    {
        TestCharacteristicBase sut = CreateSut ( ) ;

        ServiceWrapper.Uuid
                      .Returns ( sut.GattServiceUuid ) ;

        RawValueReader.TryReadValueAsync ( CharacteristicWrapper1 )
                      .Returns ( ( true , RawValue1 ) ) ;

        await sut.Initialize < TestCharacteristicBase > ( )
                 .Refresh ( ) ;

        sut.ToString ( )
           .Should ( )
           .Be ( ToStringResult ) ;
    }

    protected override TestCharacteristicBase CreateSut ( )
    {
        return new TestCharacteristicBase ( Logger ,
                                            Scheduler ,
                                            Device ,
                                            ProviderFactory ,
                                            RawValueReader ,
                                            RawValueWriter ,
                                            ToStringConverter ,
                                            DescriptionToUuid ) ;
    }

    protected override void PopulateWrappers ( )
    {
        Wrappers.Add ( TestCharacteristicBase.RawValueKey ,
                       CharacteristicWrapper1 ) ;
    }
}
