using System.Reactive.Concurrency ;
using Windows.Devices.Bluetooth.GenericAttributeProfile ;
using FluentAssertions ;
using Idasen.BluetoothLE.Characteristics.Characteristics ;
using Idasen.BluetoothLE.Characteristics.Interfaces.Characteristics ;
using Idasen.BluetoothLE.Characteristics.Interfaces.Characteristics.Customs ;
using Idasen.BluetoothLE.Characteristics.Interfaces.Common ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery.Wrappers ;
using NSubstitute ;
using Serilog ;

namespace Idasen.BluetoothLE.Characteristics.Tests.Characteristics ;

[ TestClass ]
public abstract class CharacteristicBaseTests < T >
    where T : CharacteristicBase
{
    public const string ToStringResult = "Some Text" ;

    protected readonly Dictionary < IGattDeviceServiceWrapper , IGattCharacteristicsResultWrapper > WrappersReadOnly
        =
        new( ) ;

    protected IGattCharacteristicProvider                          CharacteristicProvider  = null! ;
    protected IGattCharacteristicWrapper                           CharacteristicWrapper1  = null! ;
    protected IGattCharacteristicWrapper                           CharacteristicWrapper10 = null! ;
    protected IGattCharacteristicWrapper                           CharacteristicWrapper2  = null! ;
    protected IGattCharacteristicWrapper                           CharacteristicWrapper3  = null! ;
    protected IGattCharacteristicWrapper                           CharacteristicWrapper4  = null! ;
    protected IGattCharacteristicWrapper                           CharacteristicWrapper5  = null! ;
    protected IGattCharacteristicWrapper                           CharacteristicWrapper6  = null! ;
    protected IGattCharacteristicWrapper                           CharacteristicWrapper7  = null! ;
    protected IGattCharacteristicWrapper                           CharacteristicWrapper8  = null! ;
    protected IGattCharacteristicWrapper                           CharacteristicWrapper9  = null! ;
    protected IDescriptionToUuid                                   DescriptionToUuid       = null! ;
    protected IDevice                                              Device                  = null! ;
    protected ILogger                                              Logger                  = null! ;
    protected Dictionary < string , GattCharacteristicProperties > Properties              = null! ;
    protected IGattCharacteristicsProviderFactory                  ProviderFactory         = null! ;

    protected byte [ ]                                           RawValue1         = null! ;
    protected byte [ ]                                           RawValue10        = null! ;
    protected byte [ ]                                           RawValue2         = null! ;
    protected byte [ ]                                           RawValue3         = null! ;
    protected byte [ ]                                           RawValue4         = null! ;
    protected byte [ ]                                           RawValue5         = null! ;
    protected byte [ ]                                           RawValue6         = null! ;
    protected byte [ ]                                           RawValue7         = null! ;
    protected byte [ ]                                           RawValue8         = null! ;
    protected byte [ ]                                           RawValue9         = null! ;
    protected IRawValueReader                                    RawValueReader    = null! ;
    protected IRawValueWriter                                    RawValueWriter    = null! ;
    protected IGattCharacteristicsResultWrapper                  ResultWrapper     = null! ;
    protected IScheduler                                         Scheduler         = null! ;
    protected IGattDeviceServiceWrapper                          ServiceWrapper    = null! ;
    protected ICharacteristicBaseToStringConverter               ToStringConverter = null! ;
    protected Dictionary < string , IGattCharacteristicWrapper > Wrappers          = null! ;

    [ TestInitialize ]
    public virtual void Initialize ( )
    {
        Logger                 = Substitute.For < ILogger > ( ) ;
        Device                 = Substitute.For < IDevice > ( ) ;
        Scheduler              = Substitute.For < IScheduler > ( ) ;
        ProviderFactory        = Substitute.For < IGattCharacteristicsProviderFactory > ( ) ;
        CharacteristicProvider = Substitute.For < IGattCharacteristicProvider > ( ) ;
        RawValueReader         = Substitute.For < IRawValueReader > ( ) ;
        RawValueWriter         = Substitute.For < IRawValueWriter > ( ) ;
        ToStringConverter      = Substitute.For < ICharacteristicBaseToStringConverter > ( ) ;
        DescriptionToUuid      = new DescriptionToUuid ( ) ;

        ServiceWrapper = Substitute.For < IGattDeviceServiceWrapper > ( ) ;
        ResultWrapper  = Substitute.For < IGattCharacteristicsResultWrapper > ( ) ;

        WrappersReadOnly.Add ( ServiceWrapper ,
                               ResultWrapper ) ;

        IReadOnlyDictionary < IGattDeviceServiceWrapper , IGattCharacteristicsResultWrapper > gattServices =
            WrappersReadOnly ;

        Device.GattServices
              .Returns ( gattServices ) ;

        ProviderFactory.Create ( ResultWrapper )
                       .Returns ( CharacteristicProvider ) ;

        CharacteristicWrapper1  = Substitute.For < IGattCharacteristicWrapper > ( ) ;
        CharacteristicWrapper2  = Substitute.For < IGattCharacteristicWrapper > ( ) ;
        CharacteristicWrapper3  = Substitute.For < IGattCharacteristicWrapper > ( ) ;
        CharacteristicWrapper4  = Substitute.For < IGattCharacteristicWrapper > ( ) ;
        CharacteristicWrapper5  = Substitute.For < IGattCharacteristicWrapper > ( ) ;
        CharacteristicWrapper6  = Substitute.For < IGattCharacteristicWrapper > ( ) ;
        CharacteristicWrapper7  = Substitute.For < IGattCharacteristicWrapper > ( ) ;
        CharacteristicWrapper8  = Substitute.For < IGattCharacteristicWrapper > ( ) ;
        CharacteristicWrapper9  = Substitute.For < IGattCharacteristicWrapper > ( ) ;
        CharacteristicWrapper10 = Substitute.For < IGattCharacteristicWrapper > ( ) ;

        Wrappers = new Dictionary < string , IGattCharacteristicWrapper > ( ) ;

        CharacteristicProvider.Characteristics
                              .Returns ( Wrappers ) ;

        Properties = new Dictionary < string , GattCharacteristicProperties > ( ) ;

        CharacteristicProvider.Properties
                              .Returns ( Properties ) ;

        RawValue1  = [1 , 2 , 3] ;
        RawValue2  = [4 , 5 , 6] ;
        RawValue3  = [7 , 8 , 9] ;
        RawValue4  = [10 , 11 , 12] ;
        RawValue4  = [13 , 14 , 15] ;
        RawValue5  = [16 , 17 , 18] ;
        RawValue6  = [19 , 20 , 21] ;
        RawValue7  = [22 , 23 , 24] ;
        RawValue8  = [25 , 25 , 27] ;
        RawValue9  = [28 , 29 , 30] ;
        RawValue10 = [31 , 32 , 33] ;

        RawValueReader.TryReadValueAsync ( CharacteristicWrapper1 )
                      .Returns ( ( true , RawValue1 ) ) ;

        RawValueReader.TryReadValueAsync ( CharacteristicWrapper2 )
                      .Returns ( ( true , RawValue2 ) ) ;

        RawValueReader.TryReadValueAsync ( CharacteristicWrapper3 )
                      .Returns ( ( true , RawValue3 ) ) ;

        RawValueReader.TryReadValueAsync ( CharacteristicWrapper4 )
                      .Returns ( ( true , RawValue4 ) ) ;

        RawValueReader.TryReadValueAsync ( CharacteristicWrapper5 )
                      .Returns ( ( true , RawValue5 ) ) ;

        RawValueReader.TryReadValueAsync ( CharacteristicWrapper6 )
                      .Returns ( ( true , RawValue6 ) ) ;

        RawValueReader.TryReadValueAsync ( CharacteristicWrapper7 )
                      .Returns ( ( true , RawValue7 ) ) ;

        RawValueReader.TryReadValueAsync ( CharacteristicWrapper8 )
                      .Returns ( ( true , RawValue8 ) ) ;

        RawValueReader.TryReadValueAsync ( CharacteristicWrapper9 )
                      .Returns ( ( true , RawValue9 ) ) ;

        RawValueReader.TryReadValueAsync ( CharacteristicWrapper10 )
                      .Returns ( ( true , RawValue10 ) ) ;

        ToStringConverter.ToString ( Arg.Any < CharacteristicBase > ( ) )
                         .Returns ( ToStringResult ) ;

        PopulateWrappers ( ) ;

        AfterInitialize ( ) ;
    }

    protected virtual void AfterInitialize ( )
    {
    }

    [ TestMethod ]
    public void Initialize_ForKnownGattServiceUuid_SetsCharacteristics ( )
    {
        var sut = CreateSut ( ) ;

        ServiceWrapper.Uuid
                      .Returns ( sut.GattServiceUuid ) ;

        sut.Initialize < T > ( )
           .Characteristics
           .Should ( )
           .Be ( CharacteristicProvider ) ;
    }

    protected abstract T    CreateSut ( ) ;
    protected abstract void PopulateWrappers ( ) ;
}