using Autofac.Extras.DynamicProxy ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Core ;
using Idasen.BluetoothLE.Linak.Interfaces ;

namespace Idasen.BluetoothLE.Linak.Control ;

[ Intercept ( typeof ( LogAspect ) ) ]
/// <summary>
///     Factory that builds configured <see cref="IDeskMover"/> instances.
/// </summary>
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
        Guard.ArgumentNotNull ( factory ,
                                nameof ( factory ) ) ;

        _factory = factory ;
    }

    /// <inheritdoc />
    public IDeskMover Create ( IDeskCommandExecutor executor ,
                               IDeskHeightAndSpeed heightAndSpeed )
    {
        Guard.ArgumentNotNull ( executor ,
                                nameof ( executor ) ) ;
        Guard.ArgumentNotNull ( heightAndSpeed ,
                                nameof ( heightAndSpeed ) ) ;

        return _factory ( executor ,
                          heightAndSpeed ) ;
    }
}