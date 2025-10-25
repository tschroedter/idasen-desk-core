using System.Reactive.Concurrency ;
using System.Runtime.CompilerServices ;
using System.Runtime.InteropServices.WindowsRuntime ;
using Autofac.Extras.DynamicProxy ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Characteristics.Common ;
using Idasen.BluetoothLE.Characteristics.Interfaces.Characteristics ;
using Idasen.BluetoothLE.Characteristics.Interfaces.Characteristics.Customs ;
using Idasen.BluetoothLE.Characteristics.Interfaces.Common ;
using Idasen.BluetoothLE.Core ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery ;
using Serilog ;

[ assembly : InternalsVisibleTo ( "Idasen.BluetoothLE.Characteristics.Tests" ) ]

namespace Idasen.BluetoothLE.Characteristics.Characteristics ;

/// <summary>
///     Abstract base for Bluetooth LE characteristic classes. Centralizes service discovery, characteristic mapping, raw
///     value read/write, caching, and logging. Designed for DI and AOP with LogAspect.
/// </summary>
[ Intercept ( typeof ( LogAspect ) ) ]
public abstract class CharacteristicBase
    : ICharacteristicBase
{
    internal static readonly IEnumerable < byte > RawArrayEmpty = Enumerable.Empty < byte > ( )
                                                                            .ToArray ( ) ;

    private readonly ICharacteristicBaseToStringConverter _toStringConverter ;

    internal readonly Dictionary < string , IEnumerable < byte > >
        RawValues = new( ) ;

    private bool _disposed ;

    internal IGattCharacteristicProvider ? Characteristics ;

    protected CharacteristicBase ( ILogger                              logger ,
                                   IScheduler                           scheduler ,
                                   IDevice                              device ,
                                   IGattCharacteristicsProviderFactory  providerFactory ,
                                   IRawValueHandler                     valueHandler ,
                                   ICharacteristicBaseToStringConverter toStringConverter ,
                                   IDescriptionToUuid                   descriptionToUuid )
    {
        Guard.ArgumentNotNull ( logger ,
                                nameof ( logger ) ) ;
        Guard.ArgumentNotNull ( scheduler ,
                                nameof ( scheduler ) ) ;
        Guard.ArgumentNotNull ( device ,
                                nameof ( device ) ) ;
        Guard.ArgumentNotNull ( providerFactory ,
                                nameof ( providerFactory ) ) ;
        Guard.ArgumentNotNull ( valueHandler ,
                                nameof ( valueHandler ) ) ;
        Guard.ArgumentNotNull ( toStringConverter ,
                                nameof ( toStringConverter ) ) ;
        Guard.ArgumentNotNull ( descriptionToUuid ,
                                nameof ( descriptionToUuid ) ) ;

        Device             = device ;
        Logger             = logger ;
        Scheduler          = scheduler ;
        ProviderFactory    = providerFactory ;
        RawValueReader     = valueHandler.RawValueReader ;
        RawValueWriter     = valueHandler.RawValueWriter ;
        _toStringConverter = toStringConverter ;
        DescriptionToUuid  = descriptionToUuid ;
    }

    protected IDevice                             Device          { get ; }
    protected ILogger                             Logger          { get ; }
    protected IGattCharacteristicsProviderFactory ProviderFactory { get ; }
    protected IRawValueReader                     RawValueReader  { get ; }
    protected IRawValueWriter                     RawValueWriter  { get ; }
    protected IScheduler                          Scheduler       { get ; private set ; }

    public abstract Guid GattServiceUuid { get ; }

    internal IDescriptionToUuid DescriptionToUuid { get ; }

    /// <summary>
    ///     Initializes this characteristic by locating the GATT service, creating the characteristic provider,
    ///     and applying the mapping for type <typeparamref name="T" />. Returns this instance cast to
    ///     <typeparamref name="T" />.
    /// </summary>
    /// <typeparam name="T">The characteristic-specific type to cast and return.</typeparam>
    /// <returns>This instance cast to <typeparamref name="T" />.</returns>
    /// <exception cref="ArgumentException">Thrown if the GATT service with <see cref="GattServiceUuid" /> cannot be found.</exception>
    /// <exception cref="InvalidCastException">Thrown if this instance cannot be cast to <typeparamref name="T" />.</exception>
    public virtual T Initialize < T > ( )
        where T : class
    {
        Guard.ArgumentNotNull ( GattServiceUuid ,
                                nameof ( GattServiceUuid ) ) ;

        var (service , characteristicsResultWrapper) = Device.GattServices
                                                             .FirstOrDefault ( x => x.Key.Uuid ==
                                                                                    GattServiceUuid ) ;

        if ( service == null )
        {
            foreach ( var service1 in Device.GattServices )
            {
                Logger.Debug ( "Service: DeviceId = {DeviceId}, Uuid = {Uuid}" ,
                               service1.Key.DeviceId ,
                               service1.Key.Uuid ) ;

                foreach ( var characteristic in service1.Value.Characteristics )
                    Logger.Debug ( "Characteristic: {ServiceUuid} {Uuid} {UserDescription}" ,
                                   characteristic.ServiceUuid ,
                                   characteristic.Uuid ,
                                   characteristic.UserDescription ) ;
            }

            throw new InvalidOperationException ( "Failed, can't find GattDeviceService for " +
                                                  $"UUID {GattServiceUuid}" ) ;
        }

        Logger.Debug ( "Found GattDeviceService with UUID {Uuid}" ,
                       GattServiceUuid ) ;

        Characteristics = ProviderFactory.Create ( characteristicsResultWrapper ) ;

        WithMapping < T > ( ) ;

        return this as T ?? throw new InvalidCastException ( $"Can't cast {GetType ( )} to {typeof ( T )}" ) ;
    }

    /// <summary>
    ///     Refreshes all known characteristics by reading their raw values and updating the local cache.
    /// </summary>
    public virtual async Task Refresh ( )
    {
        if ( Characteristics == null )
        {
            Logger.Error ( "{Property} is null" ,
                           nameof ( Characteristics ) ) ;

            return ;
        }

        Characteristics.Refresh ( DescriptionToUuid.ReadOnlyDictionary ) ;

        var keys = Characteristics.Characteristics.Keys.ToArray ( ) ;

        foreach ( var key in keys )
        {
            if ( ! Characteristics.Characteristics.TryGetValue ( key ,
                                                                 out var characteristic ) )
            {
                Logger.Warning ( "Failed to get value for key {Key}" ,
                                 key ) ;

                continue ;
            }

            Logger.Debug ( "Reading raw value for {Key} and characteristic {Uuid}" ,
                           key ,
                           characteristic.Uuid ) ;

            (bool success , byte [ ] value) result =
                await RawValueReader.TryReadValueAsync ( characteristic ) ;

            RawValues [ key ] = result.success
                                    ? result.value
                                    : RawArrayEmpty ;
        }
    }

    /// <summary>
    ///     Disposes this instance. Multiple calls are safe.
    /// </summary>
    public void Dispose ( )
    {
        Dispose ( true ) ;

        GC.SuppressFinalize ( this ) ;
    }

    protected abstract T WithMapping < T > ( )
        where T : class ;

    protected async Task < bool > TryWriteValueAsync ( string               key ,
                                                       IEnumerable < byte > bytes )
    {
        try
        {
            return await DoTryWriteValueAsync ( key ,
                                                bytes ) ;
        }
        catch ( Exception e )
        {
            const string message = "Failed to write value async!" ;

            if ( e.IsBluetoothDisabledException ( ) )
                e.LogBluetoothStatusException ( Logger ,
                                                message ) ;
            else
                Logger.Error ( e ,
                               message ) ;

            return false ;
        }
    }

    private async Task < bool > DoTryWriteValueAsync ( string               key ,
                                                       IEnumerable < byte > bytes )
    {
        if ( Characteristics == null )
        {
            Logger.Error ( "{Property} is null" ,
                           nameof ( Characteristics ) ) ;

            return false ;
        }

        if ( ! Characteristics.Characteristics.TryGetValue ( key ,
                                                             out var characteristic ) )
        {
            // Keep single-argument overload for unit test expectations
            Logger.Error ( "Unknown characteristic with key '{Key}'" ,
                           key ) ;

            return false ;
        }

        return await RawValueWriter.TryWriteValueAsync ( characteristic ,
                                                         bytes.ToArray ( )
                                                              .AsBuffer ( ) ) ;
    }

    protected IEnumerable < byte > GetValueOrEmpty ( string key )
    {
        return RawValues.GetValueOrDefault ( key ,
                                             RawArrayEmpty ) ;
    }

    /// <summary>
    ///     Returns a logging-friendly string for this characteristic.
    /// </summary>
    /// <returns>A string representation for diagnostics/logging.</returns>
    public override string ToString ( )
    {
        return _toStringConverter.ToString ( this ) ;
    }

    protected virtual void Dispose ( bool disposing )
    {
        if ( _disposed )
            return ;

        _disposed = true ;
    }
}