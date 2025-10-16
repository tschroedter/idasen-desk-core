using System.Reactive.Concurrency ;
using Windows.Devices.Bluetooth.GenericAttributeProfile ;
using FluentAssertions ;
using Idasen.BluetoothLE.Characteristics.Characteristics ;
using Idasen.BluetoothLE.Characteristics.Common ;
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

    protected Dictionary < IGattDeviceServiceWrapper , IGattCharacteristicsResultWrapper > WrappersReadOnly { get ; } =
        new( ) ;

    protected IGattCharacteristicProvider CharacteristicProvider { get ; private set ; } = null! ;
    protected IGattCharacteristicWrapper CharacteristicWrapper1 { get ; private set ; } = null! ;
    protected IGattCharacteristicWrapper CharacteristicWrapper10 { get ; private set ; } = null! ;
    protected IGattCharacteristicWrapper CharacteristicWrapper2 { get ; private set ; } = null! ;
    protected IGattCharacteristicWrapper CharacteristicWrapper3 { get ; private set ; } = null! ;
    protected IGattCharacteristicWrapper CharacteristicWrapper4 { get ; private set ; } = null! ;
    protected IGattCharacteristicWrapper CharacteristicWrapper5 { get ; private set ; } = null! ;
    protected IGattCharacteristicWrapper CharacteristicWrapper6 { get ; private set ; } = null! ;
    protected IGattCharacteristicWrapper CharacteristicWrapper7 { get ; private set ; } = null! ;
    protected IGattCharacteristicWrapper CharacteristicWrapper8 { get ; private set ; } = null! ;
    protected IGattCharacteristicWrapper CharacteristicWrapper9 { get ; private set ; } = null! ;
    protected IDescriptionToUuid DescriptionToUuid { get ; private set ; } = null! ;
    protected IDevice Device { get ; private set ; } = null! ;
    protected ILogger Logger { get ; private set ; } = null! ;
    protected Dictionary < string , GattCharacteristicProperties > Properties { get ; private set ; } = null! ;
    protected IGattCharacteristicsProviderFactory ProviderFactory { get ; private set ; } = null! ;

    protected byte [ ]                                           RawValue1         { get ; private set ; } = null! ;
    protected byte [ ]                                           RawValue10        { get ; private set ; } = null! ;
    protected byte [ ]                                           RawValue2         { get ; private set ; } = null! ;
    protected byte [ ]                                           RawValue3         { get ; private set ; } = null! ;
    protected byte [ ]                                           RawValue4         { get ; private set ; } = null! ;
    protected byte [ ]                                           RawValue5         { get ; private set ; } = null! ;
    protected byte [ ]                                           RawValue6         { get ; private set ; } = null! ;
    protected byte [ ]                                           RawValue7         { get ; private set ; } = null! ;
    protected byte [ ]                                           RawValue8         { get ; private set ; } = null! ;
    protected byte [ ]                                           RawValue9         { get ; private set ; } = null! ;
    protected IRawValueReader                                    RawValueReader    { get ; private set ; } = null! ;
    protected IRawValueWriter                                    RawValueWriter    { get ; private set ; } = null! ;
    protected IGattCharacteristicsResultWrapper                  ResultWrapper     { get ; private set ; } = null! ;
    protected IScheduler                                         Scheduler         { get ; private set ; } = null! ;
    protected IGattDeviceServiceWrapper                          ServiceWrapper    { get ; private set ; } = null! ;
    protected ICharacteristicBaseToStringConverter               ToStringConverter { get ; private set ; } = null! ;
    protected Dictionary < string , IGattCharacteristicWrapper > Wrappers          { get ; private set ; } = null! ;
    protected IRawValueHandler                                   RawValueHandler   { get ; private set ; } = null! ;

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
        RawValueHandler        = new RawValueHandler ( RawValueReader ,
                                                       RawValueWriter ) ;

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
