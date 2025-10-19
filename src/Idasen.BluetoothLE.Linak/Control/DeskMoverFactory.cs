using Autofac.Extras.DynamicProxy ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Linak.Interfaces ;
using Serilog ;

namespace Idasen.BluetoothLE.Linak.Control ;

/// <inheritdoc />
[ Intercept ( typeof ( LogAspect ) ) ]
public class DeskMoverFactory
    : IDeskMoverFactory
{
    private readonly ILogger                               _logger ;
    private readonly DeskMover.Factory                     _factory ;
    private readonly IInitialHeightAndSpeedProviderFactory _providerFactory ;
    private readonly IDeskMovementMonitorFactory           _monitorFactory ;
    private readonly IStoppingHeightCalculator             _calculator ;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DeskMoverFactory" /> class.
    /// </summary>
    public DeskMoverFactory ( ILogger                               logger ,
                              DeskMover.Factory                     factory ,
                              IInitialHeightAndSpeedProviderFactory providerFactory ,
                              IDeskMovementMonitorFactory           monitorFactory ,
                              IStoppingHeightCalculator             calculator )
    {
        ArgumentNullException.ThrowIfNull ( logger ) ;
        ArgumentNullException.ThrowIfNull ( factory ) ;
        ArgumentNullException.ThrowIfNull ( providerFactory ) ;
        ArgumentNullException.ThrowIfNull ( monitorFactory ) ;
        ArgumentNullException.ThrowIfNull ( calculator ) ;

        _logger          = logger ;
        _factory         = factory ;
        _providerFactory = providerFactory ;
        _monitorFactory  = monitorFactory ;
        _calculator      = calculator ;
    }

    /// <inheritdoc />
    public IDeskMover Create ( IDeskCommandExecutor executor ,
                               IDeskHeightAndSpeed  heightAndSpeed )
    {
        ArgumentNullException.ThrowIfNull ( executor ) ;
        ArgumentNullException.ThrowIfNull ( heightAndSpeed ) ;

        var locationHandlers = new DeskLocationHandlers ( heightAndSpeed ,
                                                          _providerFactory ) ;

        var movementHandlers = new DeskMovementHandlers ( _logger ,
                                                          heightAndSpeed ,
                                                          _monitorFactory ,
                                                          executor ,
                                                          _calculator ) ;

        return _factory ( locationHandlers ,
                          movementHandlers ) ;
    }
}
