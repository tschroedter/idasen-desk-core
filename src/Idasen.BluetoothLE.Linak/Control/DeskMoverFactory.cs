namespace Idasen.BluetoothLE.Linak.Control ;

using Aop.Aspects ;
using Autofac.Extras.DynamicProxy ;
using Interfaces ;

/// <inheritdoc />
[ Intercept ( typeof ( LogAspect ) ) ]
public class DeskMoverFactory
    : IDeskMoverFactory
{
    private readonly DeskMover.Factory _factory ;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DeskMoverFactory" /> class.
    /// </summary>
    /// <param name="factory">The IoC factory delegate for <see cref="DeskMover" />.</param>
    public DeskMoverFactory ( DeskMover.Factory factory )
    {
        ArgumentNullException.ThrowIfNull ( factory ) ;

        _factory = factory ;
    }

    /// <inheritdoc />
    public IDeskMover Create ( IDeskCommandExecutor executor ,
                               IDeskHeightAndSpeed heightAndSpeed )
    {
        ArgumentNullException.ThrowIfNull ( executor ) ;
        ArgumentNullException.ThrowIfNull ( heightAndSpeed ) ;

        return _factory ( executor ,
                          heightAndSpeed ) ;
    }
}
