using System.Text ;
using Autofac.Extras.DynamicProxy ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Characteristics.Interfaces.Characteristics ;
using Idasen.BluetoothLE.Core ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery ;
using Idasen.BluetoothLE.Linak.Interfaces ;
using Serilog ;

namespace Idasen.BluetoothLE.Linak ;

/// <summary>
///     Aggregates discovered characteristics for a LINAK desk and provides typed accessors.
/// </summary>
[ Intercept ( typeof ( LogAspect ) ) ]
public class DeskCharacteristics
    : IDeskCharacteristics
{
    private readonly Dictionary < DeskCharacteristicKey , ICharacteristicBase > _available = new ( ) ;

    private readonly IDeskCharacteristicsCreator _creator ;
    private readonly ILogger _logger ;

    public DeskCharacteristics (
        ILogger logger ,
        IDeskCharacteristicsCreator creator )
    {
        Guard.ArgumentNotNull ( creator ,
                                nameof ( creator ) ) ;
        Guard.ArgumentNotNull ( logger ,
                                nameof ( logger ) ) ;

        _logger = logger ;
        _creator = creator ;
    }

    /// <summary>
    ///     Gets the available characteristics map.
    /// </summary>
    public IReadOnlyDictionary < DeskCharacteristicKey , ICharacteristicBase > Characteristics => _available ;

    /// <inheritdoc />
    public async Task Refresh ( )
    {
        foreach (var characteristicBase in _available.Values)
        {
            await characteristicBase.Refresh ( ) ;
        }
    }

    /// <inheritdoc />
    public IDeskCharacteristics Initialize ( IDevice device )
    {
        Guard.ArgumentNotNull ( device ,
                                nameof ( device ) ) ;

        _creator.Create ( this ,
                          device ) ;

        return this ;
    }

    /// <inheritdoc />
    public IGenericAccess GenericAccess =>
        _available.As < IGenericAccess > ( DeskCharacteristicKey.GenericAccess ) ;

    /// <inheritdoc />
    public IGenericAttribute GenericAttribute =>
        _available.As < IGenericAttribute > ( DeskCharacteristicKey.GenericAttribute ) ;

    /// <inheritdoc />
    public IReferenceInput ReferenceInput =>
        _available.As < IReferenceInput > ( DeskCharacteristicKey.ReferenceInput ) ;

    /// <inheritdoc />
    public IReferenceOutput ReferenceOutput =>
        _available.As < IReferenceOutput > ( DeskCharacteristicKey.ReferenceOutput ) ;

    /// <inheritdoc />
    public IDpg Dpg => _available.As < IDpg > ( DeskCharacteristicKey.Dpg ) ;

    /// <inheritdoc />
    public IControl Control => _available.As < IControl > ( DeskCharacteristicKey.Control ) ;

    /// <inheritdoc />
    public IDeskCharacteristics WithCharacteristics (
        DeskCharacteristicKey key ,
        ICharacteristicBase characteristic )
    {
        Guard.ArgumentNotNull ( characteristic ,
                                nameof ( characteristic ) ) ;

        characteristic.Initialize < ICharacteristicBase > ( ) ;

        if ( _available.TryGetValue ( key ,
                                      out var oldCharacteristic ) )
        {
            oldCharacteristic.Dispose ( ) ;
        }

        _available[key] = characteristic ;

        _logger.Debug ( $"Added characteristic {characteristic} for key {key}" ) ;

        return this ;
    }

    /// <summary>
    ///     Returns a multi-line representation of the current characteristics and their values.
    /// </summary>
    public override string ToString ( )
    {
        var builder = new StringBuilder ( ) ;

        builder.AppendLine ( GenericAccess.ToString ( ) ) ;
        builder.AppendLine ( GenericAttribute.ToString ( ) ) ;
        builder.AppendLine ( ReferenceInput.ToString ( ) ) ;
        builder.AppendLine ( ReferenceOutput.ToString ( ) ) ;
        builder.AppendLine ( Dpg.ToString ( ) ) ;
        builder.AppendLine ( Control.ToString ( ) ) ;

        return builder.ToString ( ) ;
    }
}