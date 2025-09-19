using Autofac.Extras.DynamicProxy ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Core ;
using Idasen.BluetoothLE.Linak.Interfaces ;

namespace Idasen.BluetoothLE.Linak.Control ;

[ Intercept ( typeof ( LogAspect ) ) ]
/// <summary>
///     Factory for creating <see cref="IDeskMovementMonitor"/> instances.
/// </summary>
public class DeskMovementMonitorFactory
    : IDeskMovementMonitorFactory
{
    private readonly DeskMovementMonitor.Factory _factory ;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DeskMovementMonitorFactory" /> class.
    /// </summary>
    /// <param name="factory">The IoC factory delegate for <see cref="DeskMovementMonitor" />.</param>
    public DeskMovementMonitorFactory ( DeskMovementMonitor.Factory factory )
    {
        Guard.ArgumentNotNull ( factory ,
                                nameof ( factory ) ) ;

        _factory = factory ;
    }

    /// <inheritdoc />
    public IDeskMovementMonitor Create ( IDeskHeightAndSpeed heightAndSpeed )
    {
        Guard.ArgumentNotNull ( heightAndSpeed ,
                                nameof ( heightAndSpeed ) ) ;

        return _factory ( heightAndSpeed ) ;
    }
}