using Autofac.Extras.DynamicProxy ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Characteristics.Interfaces.Characteristics ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery ;
using Idasen.BluetoothLE.Linak.Interfaces ;
using Serilog ;

namespace Idasen.BluetoothLE.Linak ;

/// <inheritdoc />
[ Intercept ( typeof ( LogAspect ) ) ]
public class DeskCharacteristicsCreator
    : IDeskCharacteristicsCreator
{
    private readonly ICharacteristicBaseFactory _baseFactory ;
    private readonly ILogger                    _logger ;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DeskCharacteristicsCreator" /> class.
    /// </summary>
    public DeskCharacteristicsCreator (
        ILogger                    logger ,
        ICharacteristicBaseFactory baseFactory )
    {
        ArgumentNullException.ThrowIfNull ( logger ) ;
        ArgumentNullException.ThrowIfNull ( baseFactory ) ;

        _logger      = logger ;
        _baseFactory = baseFactory ;
    }

    /// <inheritdoc />
    public void Create (
        IDeskCharacteristics characteristics ,
        IDevice              device )
    {
        ArgumentNullException.ThrowIfNull ( device ) ;
        ArgumentNullException.ThrowIfNull ( characteristics ) ;

        _logger.Debug (
                       "[{DeviceId}] Creating desk characteristics {Characteristics}" ,
                       device.Id ,
                       characteristics ) ;

        characteristics.WithCharacteristics (
                                             DeskCharacteristicKey.GenericAccess ,
                                             _baseFactory.Create < IGenericAccess > ( device ) )
                       .WithCharacteristics (
                                             DeskCharacteristicKey.GenericAttribute ,
                                             _baseFactory.Create < IGenericAttribute > ( device ) )
                       .WithCharacteristics (
                                             DeskCharacteristicKey.ReferenceInput ,
                                             _baseFactory.Create < IReferenceInput > ( device ) )
                       .WithCharacteristics (
                                             DeskCharacteristicKey.ReferenceOutput ,
                                             _baseFactory.Create < IReferenceOutput > ( device ) )
                       .WithCharacteristics (
                                             DeskCharacteristicKey.Dpg ,
                                             _baseFactory.Create < IDpg > ( device ) )
                       .WithCharacteristics (
                                             DeskCharacteristicKey.Control ,
                                             _baseFactory.Create < IControl > ( device ) ) ;
    }
}
