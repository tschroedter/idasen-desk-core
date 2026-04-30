using System.Diagnostics.CodeAnalysis ;
using Windows.Devices.Bluetooth.GenericAttributeProfile ;
using Windows.Storage.Streams ;
using Autofac.Extras.DynamicProxy ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery.Wrappers ;
using Serilog ;

namespace Idasen.BluetoothLE.Core.ServicesDiscovery.Wrappers ;

/// <inheritdoc />
[ ExcludeFromCodeCoverage ]
[ Intercept ( typeof ( LogAspect ) ) ]
public class GattCharacteristicWrapper
    : IGattCharacteristicWrapper
{
    public delegate IGattCharacteristicWrapper Factory ( GattCharacteristic characteristic ) ;

    private readonly GattCharacteristic                         _characteristic ;
    private readonly ILogger                                    _logger ;
    private readonly IGattCharacteristicValueChangedObservables _observables ;
    private readonly IGattReadResultWrapperFactory              _readResultFactory ;
    private readonly IGattWriteResultWrapperFactory             _writeResultFactory ;

    private bool _disposed ;

    public GattCharacteristicWrapper ( ILogger                                    logger ,
                                       GattCharacteristic                         characteristic ,
                                       IGattCharacteristicValueChangedObservables observables ,
                                       IGattWriteResultWrapperFactory             writeResultFactory ,
                                       IGattReadResultWrapperFactory              readResultFactory )
    {
        Guard.ArgumentNotNull ( logger ,
                                nameof ( logger ) ) ;
        Guard.ArgumentNotNull ( characteristic ,
                                nameof ( characteristic ) ) ;
        Guard.ArgumentNotNull ( observables ,
                                nameof ( observables ) ) ;
        Guard.ArgumentNotNull ( writeResultFactory ,
                                nameof ( writeResultFactory ) ) ;
        Guard.ArgumentNotNull ( readResultFactory ,
                                nameof ( readResultFactory ) ) ;

        _logger             = logger ;
        _characteristic     = characteristic ;
        _observables        = observables ;
        _writeResultFactory = writeResultFactory ;
        _readResultFactory  = readResultFactory ;
    }

    /// <inheritdoc />
    public async Task < IGattCharacteristicWrapper > Initialize ( )
    {
        _logger.Debug ( "Initializing GattCharacteristic with UUID {Uuid}" ,
                        _characteristic.Uuid ) ;

        await _observables.Initialise ( _characteristic ) ;

        return this ;
    }

    /// <inheritdoc />
    public IObservable < GattCharacteristicValueChangedDetails > ValueChanged => _observables.ValueChanged ;

    /// <inheritdoc />
    public Guid Uuid => _characteristic.Uuid ;

    /// <inheritdoc />
    public GattCharacteristicProperties CharacteristicProperties => _characteristic.CharacteristicProperties ;

    public IReadOnlyList < GattPresentationFormat > PresentationFormats => _characteristic.PresentationFormats ;
    public Guid ServiceUuid => _characteristic.Service?.Uuid ?? Guid.Empty ; // maybe inject IGattDeviceServiceWrapper
    public string UserDescription => _characteristic.UserDescription ;
    public GattProtectionLevel ProtectionLevel => _characteristic.ProtectionLevel ;
    public ushort AttributeHandle => _characteristic.AttributeHandle ;

    /// <inheritdoc />
    public async Task < IGattWriteResultWrapper > WriteValueWithResultAsync ( IBuffer buffer , int timeoutMs = 5000 )
    {
        var writeTask = _characteristic.WriteValueWithResultAsync ( buffer ).AsTask ( ) ;
        var completedTask = await Task.WhenAny ( writeTask ,
                                                  Task.Delay ( timeoutMs ) ) ;

        if ( completedTask == writeTask )
        {
            var result = await writeTask ;
            return _writeResultFactory.Create ( result ) ;
        }

        _logger.Warning ( "WriteValueWithResultAsync timed out after {TimeoutMs}ms for characteristic {Uuid}" ,
                          timeoutMs ,
                          _characteristic.Uuid ) ;

        // Return a timeout result
        return new GattWriteResultWrapperNotSupported ( status : GattCommunicationStatus.Unreachable ) ;
    }

    /// <inheritdoc />
    public async Task < GattCommunicationStatus > WriteValueAsync ( IBuffer buffer , int timeoutMs = 5000 )
    {
        var writeTask = _characteristic.WriteValueAsync ( buffer ).AsTask ( ) ;
        var completedTask = await Task.WhenAny ( writeTask ,
                                                  Task.Delay ( timeoutMs ) ) ;

        if ( completedTask == writeTask )
        {
            return await writeTask ;
        }

        _logger.Warning ( "WriteValueAsync timed out after {TimeoutMs}ms for characteristic {Uuid}" ,
                          timeoutMs ,
                          _characteristic.Uuid ) ;

        return GattCommunicationStatus.Unreachable ;
    }

    /// <inheritdoc />
    public async Task < IGattReadResultWrapper > ReadValueAsync ( )
    {
        var result = await _characteristic.ReadValueAsync ( ).AsTask ( ) ;

        var wrapper = _readResultFactory.Create ( result ) ;

        return wrapper ;
    }

    public void Dispose ( )
    {
        Dispose ( true ) ;

        GC.SuppressFinalize ( this ) ;
    }

    protected virtual void Dispose ( bool disposing )
    {
        if ( _disposed )
            return ;

        if ( disposing ) _observables.Dispose ( ) ;

        _disposed = true ;
    }
}
