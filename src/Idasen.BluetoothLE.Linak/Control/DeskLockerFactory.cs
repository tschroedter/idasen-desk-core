using Autofac.Extras.DynamicProxy ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Linak.Interfaces ;

namespace Idasen.BluetoothLE.Linak.Control ;

/// <inheritdoc />
[Intercept ( typeof ( LogAspect ) ) ]
public class DeskLockerFactory
    : IDeskLockerFactory
{
    private readonly DeskLocker.Factory _factory ;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DeskLockerFactory" /> class.
    /// </summary>
    public DeskLockerFactory ( DeskLocker.Factory factory )
    {
        ArgumentNullException.ThrowIfNull ( factory ) ;

        _factory = factory ;
    }

    /// <inheritdoc />
    public IDeskLocker Create ( IDeskMover deskMover ,
                                IDeskCommandExecutor executor ,
                                IDeskHeightAndSpeed heightAndSpeed )
    {
        ArgumentNullException.ThrowIfNull ( deskMover ) ;
        ArgumentNullException.ThrowIfNull ( executor ) ;
        ArgumentNullException.ThrowIfNull ( heightAndSpeed ) ;

        return _factory ( deskMover ,
                          executor ,
                          heightAndSpeed ) ;
    }
}