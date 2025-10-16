using Windows.Storage.Streams ;
using FluentAssertions ;
using Idasen.BluetoothLE.Characteristics.Characteristics ;
using Idasen.BluetoothLE.Common.Tests ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery.Wrappers ;
using JetBrains.Annotations ;
using NSubstitute ;
using Selkie.AutoMocking ;

namespace Idasen.BluetoothLE.Characteristics.Tests.Characteristics ;

[ TestClass ]
public class CharacteristicBaseTest
    : CharacteristicBaseTests < TestCharacteristicBase >
{
    private static readonly Guid GattCharacteristicUuid = Guid.Parse ( "22222222-2222-2222-2222-222222222222" ) ;
    private static readonly Guid GattServiceUuid        = Guid.Parse ( "11111111-1111-1111-1111-111111111111" ) ;

    [ UsedImplicitly ]
    protected static Guid GetGattCharacteristicUuid ( )
    {
        return GattCharacteristicUuid ;
    }

    [ UsedImplicitly ]
    protected static Guid GetGattServiceUuid ( )
    {
        return GattServiceUuid ;
    }

    [ TestMethod ]
    public void GattServiceUuid_ForInvoked_Uuid ( )
    {
        using var sut = CreateSut ( ) ;

        sut.GattServiceUuid
           .Should ( )
           .Be ( GattServiceUuid ) ;
    }

    [ AutoDataTestMethod ]
    public void Initialize_ForUnknownGattServiceUuid_Throws ( TestCharacteristicBase sut ,
                                                              [ Freeze ] IDevice     device )
    {
        IReadOnlyDictionary < IGattDeviceServiceWrapper , IGattCharacteristicsResultWrapper > gattServices =
            new Dictionary < IGattDeviceServiceWrapper , IGattCharacteristicsResultWrapper > ( ) ;

        device.GattServices
              .Returns ( gattServices ) ;

        Action action = ( ) => sut.Initialize < TestCharacteristicBase > ( ) ;

        action.Should ( )
              .Throw < InvalidOperationException > ( ) ;
    }

    [ TestMethod ]
    public void Initialize_ForKnownGattServiceUuid_AddsKeyToDescriptionToUuid ( )
    {
        using var sut = CreateSut ( ) ;

        ServiceWrapper.Uuid
                      .Returns ( sut.GattServiceUuid ) ;

        sut.Initialize < TestCharacteristicBase > ( )
           .DescriptionToUuid [ TestCharacteristicBase.RawValueKey ]
           .Should ( )
           .Be ( GattCharacteristicUuid ) ;
    }

    [ TestMethod ]
    public void RawDpg_ForNotRefreshedAndInvoked_EmptyBytes ( )
    {
        using var sut = CreateSut ( ) ;

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
        using var sut = CreateSut ( ) ;

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
        using var sut = CreateSut ( ) ;

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
        using var sut = CreateSut ( ) ;

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
        using var sut = CreateSut ( ) ;

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
        using var sut = CreateSut ( ) ;

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
        using var sut = CreateSut ( ) ;

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

        using var sut = CreateSut ( ) ;

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

        using var sut = CreateSut ( ) ;

        ServiceWrapper.Uuid
                      .Returns ( sut.GattServiceUuid ) ;

        await sut.Initialize < TestCharacteristicBase > ( )
                 .Refresh ( ) ;

        await sut.TryWriteRawValue ( RawValue1 ) ;

        const string messageTemplate = "Unknown characteristic with key '{Key}'" ;

        Logger.Received ( )
              .Error ( messageTemplate ,
                       Arg.Any < string > ( ) ) ;
    }

    [ TestMethod ]
    [ Ignore ( "Existing Characteristic will always return a value" ) ]
    public async Task TryWriteRawValue_ForKnownCharacteristicsIsNull_ReturnsFalse ( )
    {
        Wrappers.Clear ( ) ;

        Wrappers.Add ( TestCharacteristicBase.RawValueKey ,
                       null! ) ;

        using var sut = CreateSut ( ) ;

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

        using var sut = CreateSut ( ) ;

        ServiceWrapper.Uuid
                      .Returns ( sut.GattServiceUuid ) ;

        await sut.Initialize < TestCharacteristicBase > ( )
                 .Refresh ( ) ;

        await sut.TryWriteRawValue ( RawValue1 ) ;

        Logger.ReceivedWithAnyArgs ( )
              .Error ( "Ignored test error {Key}" ,
                       Arg.Any < string > ( ) ) ;
    }

    [ TestMethod ]
    public async Task ToString_ForInvoke_Instance ( )
    {
        using var sut = CreateSut ( ) ;

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
                                            RawValueHandler ,
                                            ToStringConverter ,
                                            DescriptionToUuid ) ;
    }

    protected override void PopulateWrappers ( )
    {
        Wrappers.Add ( TestCharacteristicBase.RawValueKey ,
                       CharacteristicWrapper1 ) ;
    }
}
