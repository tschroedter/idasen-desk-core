using Autofac.Extras.DynamicProxy ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Characteristics.Interfaces.Characteristics ;
using Idasen.BluetoothLE.Core ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery ;
using Idasen.BluetoothLE.Linak.Interfaces ;
using Serilog ;

namespace Idasen.BluetoothLE.Linak ;

/// <inheritdoc />
[ Intercept ( typeof ( LogAspect ) ) ]
/// <summary>
///     Creates and attaches all known LINAK desk characteristics to a given container.
/// </summary>
public class DeskCharacteristicsCreator
    : IDeskCharacteristicsCreator
{
    private readonly ICharacteristicBaseFactory _baseFactory ;
    private readonly ILogger _logger ;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DeskCharacteristicsCreator"/> class.
    /// </summary>
    public DeskCharacteristicsCreator (
        ILogger logger ,
        ICharacteristicBaseFactory baseFactory )
    {
        Guard.ArgumentNotNull ( logger ,
                                nameof ( logger ) ) ;
        Guard.ArgumentNotNull ( baseFactory ,
                                nameof ( baseFactory ) ) ;

        _logger = logger ;
        _baseFactory = baseFactory ;
    }

    /// <inheritdoc />
    public void Create (
        IDeskCharacteristics characteristics ,
        IDevice device )
    {
        Guard.ArgumentNotNull ( device ,
                                nameof ( device ) ) ;
        Guard.ArgumentNotNull ( characteristics ,
                                nameof ( characteristics ) ) ;

        _logger.Debug ( $"[{device.Id}] Creating desk characteristics {characteristics}" ) ;

        characteristics.WithCharacteristics ( DeskCharacteristicKey.GenericAccess ,
                                              _baseFactory.Create < IGenericAccess > ( device ) )
                       .WithCharacteristics ( DeskCharacteristicKey.GenericAttribute ,
                                              _baseFactory.Create < IGenericAttribute > ( device ) )
                       .WithCharacteristics ( DeskCharacteristicKey.ReferenceInput ,
                                              _baseFactory.Create < IReferenceInput > ( device ) )
                       .WithCharacteristics ( DeskCharacteristicKey.ReferenceOutput ,
                                              _baseFactory.Create < IReferenceOutput > ( device ) )
                       .WithCharacteristics ( DeskCharacteristicKey.Dpg ,
                                              _baseFactory.Create < IDpg > ( device ) )
                       .WithCharacteristics ( DeskCharacteristicKey.Control ,
                                              _baseFactory.Create < IControl > ( device ) ) ;
    }
}