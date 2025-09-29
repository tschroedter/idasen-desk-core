using System.Reactive.Concurrency ;
using System.Reactive.Subjects ;
using Idasen.BluetoothLE.Characteristics.Interfaces.Characteristics ;
using Idasen.BluetoothLE.Characteristics.Interfaces.Characteristics.Customs ;
using Idasen.BluetoothLE.Characteristics.Interfaces.Common ;
using Idasen.BluetoothLE.Core ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery ;
using Serilog ;

namespace Idasen.BluetoothLE.Characteristics.Characteristics ;

public class GenericAccess
    : CharacteristicBase ,
      IGenericAccess
{
    internal const string CharacteristicDeviceName = "Device Name" ;
    internal const string CharacteristicAppearance = "Appearance" ;
    internal const string CharacteristicParameters = "Peripheral Preferred Connection Parameters" ;
    internal const string CharacteristicResolution = "Central Address Resolution" ;

    private readonly IAllGattCharacteristicsProvider _allGattCharacteristicsProvider ;

    public GenericAccess ( ILogger                                    logger ,
                           IScheduler                                 scheduler ,
                           IDevice                                    device ,
                           IGattCharacteristicsProviderFactory        providerFactory ,
                           IRawValueReader                            rawValueReader ,
                           IRawValueWriter                            rawValueWriter ,
                           ICharacteristicBaseToStringConverter       toStringConverter ,
                           IDescriptionToUuid                         descriptionToUuid ,
                           Func < ISubject < IEnumerable < byte > > > subjectFactory ,
                           IAllGattCharacteristicsProvider            allGattCharacteristicsProvider )
        : base ( logger ,
                 scheduler ,
                 device ,
                 providerFactory ,
                 rawValueReader ,
                 rawValueWriter ,
                 toStringConverter ,
                 descriptionToUuid )
    {
        Guard.ArgumentNotNull ( subjectFactory ,
                                nameof ( subjectFactory ) ) ;
        Guard.ArgumentNotNull ( allGattCharacteristicsProvider ,
                                nameof ( allGattCharacteristicsProvider ) ) ;

        _allGattCharacteristicsProvider = allGattCharacteristicsProvider ;

        DeviceNameChanged = subjectFactory ( ) ;
        ResolutionChanged = subjectFactory ( ) ;
        ParametersChanged = subjectFactory ( ) ;
        AppearanceChanged = subjectFactory ( ) ;
    }

    /// <inheritdoc />
    public ISubject < IEnumerable < byte > > AppearanceChanged { get ; }

    /// <inheritdoc />
    public ISubject < IEnumerable < byte > > ParametersChanged { get ; }

    /// <inheritdoc />
    public ISubject < IEnumerable < byte > > ResolutionChanged { get ; }

    /// <inheritdoc />
    public ISubject < IEnumerable < byte > > DeviceNameChanged { get ; }

    /// <inheritdoc cref="IGenericAccess.GattServiceUuid" />
    public override Guid GattServiceUuid { get ; } = Guid.Parse ( "00001800-0000-1000-8000-00805F9B34FB" ) ;

    /// <inheritdoc />
    public IEnumerable < byte > RawResolution => GetValueOrEmpty ( CharacteristicResolution ) ;

    /// <inheritdoc />
    public IEnumerable < byte > RawParameters => GetValueOrEmpty ( CharacteristicParameters ) ;

    /// <inheritdoc />
    public IEnumerable < byte > RawAppearance => GetValueOrEmpty ( CharacteristicAppearance ) ;

    /// <inheritdoc />
    public IEnumerable < byte > RawDeviceName => GetValueOrEmpty ( CharacteristicDeviceName ) ;

    public override async Task Refresh ( )
    {
        await base.Refresh ( ) ;

        AppearanceChanged.OnNext ( RawAppearance ) ;
        ParametersChanged.OnNext ( RawParameters ) ;
        ResolutionChanged.OnNext ( RawResolution ) ;
        DeviceNameChanged.OnNext ( RawDeviceName ) ;
    }

    protected override T WithMapping < T > ( )
        where T : class
    {
        if ( _allGattCharacteristicsProvider.TryGetUuid ( CharacteristicDeviceName ,
                                                          out var uuid ) )
            DescriptionToUuid [ CharacteristicDeviceName ] = uuid ;

        if ( _allGattCharacteristicsProvider.TryGetUuid ( CharacteristicAppearance ,
                                                          out uuid ) )
            DescriptionToUuid [ CharacteristicAppearance ] = uuid ;

        if ( _allGattCharacteristicsProvider.TryGetUuid ( CharacteristicParameters ,
                                                          out uuid ) )
            DescriptionToUuid [ CharacteristicParameters ] = uuid ;

        if ( _allGattCharacteristicsProvider.TryGetUuid ( CharacteristicResolution ,
                                                          out uuid ) )
            DescriptionToUuid [ CharacteristicResolution ] = uuid ;

        return this as T ?? throw new InvalidCastException ( $"Can't cast {GetType ( )} to {typeof ( T )}" ) ;
    }
}