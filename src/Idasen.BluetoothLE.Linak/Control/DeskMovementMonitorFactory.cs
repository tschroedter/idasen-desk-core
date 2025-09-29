using Autofac.Extras.DynamicProxy ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Linak.Interfaces ;

namespace Idasen.BluetoothLE.Linak.Control ;

/// <inheritdoc />
[ Intercept ( typeof ( LogAspect ) ) ]
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
        ArgumentNullException.ThrowIfNull ( factory ) ;

        _factory = factory ;
    }

    /// <inheritdoc />
    public IDeskMovementMonitor Create ( IDeskHeightAndSpeed heightAndSpeed )
    {
        ArgumentNullException.ThrowIfNull ( heightAndSpeed ) ;

        return _factory ( heightAndSpeed ) ;
    }
}