namespace Idasen.BluetoothLE.Core.ServicesDiscovery.Wrappers ;

using System.Diagnostics.CodeAnalysis ;
using Windows.Devices.Bluetooth.GenericAttributeProfile ;
using Windows.Storage.Streams ;
using Aop.Aspects ;
using Autofac.Extras.DynamicProxy ;
using Interfaces.ServicesDiscovery.Wrappers ;
using Serilog ;

/// <inheritdoc />
[ ExcludeFromCodeCoverage ]
[ Intercept ( typeof ( LogAspect ) ) ]
public class GattCharacteristicWrapper
    : IGattCharacteristicWrapper
{
    public delegate IGattCharacteristicWrapper Factory ( GattCharacteristic characteristic ) ;

    private readonly GattCharacteristic _characteristic ;
    private readonly ILogger _logger ;
    private readonly IGattCharacteristicValueChangedObservables _observables ;
    private readonly IGattReadResultWrapperFactory _readResultFactory ;
    private readonly IGattWriteResultWrapperFactory _writeResultFactory ;

    public GattCharacteristicWrapper (
        ILogger logger ,
        GattCharacteristic characteristic ,
        IGattCharacteristicValueChangedObservables observables ,
        IGattWriteResultWrapperFactory writeResultFactory ,
        IGattReadResultWrapperFactory readResultFactory )
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

        _logger = logger ;
        _characteristic = characteristic ;
        _observables = observables ;
        _writeResultFactory = writeResultFactory ;
        _readResultFactory = readResultFactory ;
    }

    /// <inheritdoc />
    public async Task < IGattCharacteristicWrapper > Initialize ( )
    {
        _logger.Information ( "Initializing GattCharacteristic with UUID {Uuid}" ,
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
    public async Task < IGattWriteResultWrapper > WriteValueWithResultAsync ( IBuffer buffer )
    {
        GattWriteResult? result = await _characteristic.WriteValueWithResultAsync ( buffer ) ;

        return _writeResultFactory.Create ( result ) ;
    }

    /// <inheritdoc />
    public async Task < GattCommunicationStatus > WriteValueAsync ( IBuffer buffer ) => await _characteristic.WriteValueAsync ( buffer ) ;

    /// <inheritdoc />
    public async Task < IGattReadResultWrapper > ReadValueAsync ( )
    {
        GattReadResult? result = await _characteristic.ReadValueAsync ( ).AsTask ( ) ;

        IGattReadResultWrapper wrapper = _readResultFactory.Create ( result ) ;

        return wrapper ;
    }

    public void Dispose ( )
    {
        _observables.Dispose ( ) ;
        GC.SuppressFinalize ( this ) ;
    }
}
