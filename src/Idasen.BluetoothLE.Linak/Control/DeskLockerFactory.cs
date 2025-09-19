using Autofac.Extras.DynamicProxy ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Core ;
using Idasen.BluetoothLE.Linak.Interfaces ;

namespace Idasen.BluetoothLE.Linak.Control ;

[ Intercept ( typeof ( LogAspect ) ) ]
/// <summary>
///     Factory that creates <see cref="IDeskLocker"/> instances.
/// </summary>
public class DeskLockerFactory
    : IDeskLockerFactory
{
    private readonly DeskLocker.Factory _factory ;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DeskLockerFactory" /> class.
    /// </summary>
    public DeskLockerFactory ( DeskLocker.Factory factory )
    {
        Guard.ArgumentNotNull ( factory ,
                                nameof ( factory ) ) ;

        _factory = factory ;
    }

    /// <inheritdoc />
    public IDeskLocker Create ( IDeskMover deskMover ,
                                IDeskCommandExecutor executor ,
                                IDeskHeightAndSpeed heightAndSpeed )
    {
        Guard.ArgumentNotNull ( deskMover ,
                                nameof ( deskMover ) ) ;
        Guard.ArgumentNotNull ( executor ,
                                nameof ( executor ) ) ;
        Guard.ArgumentNotNull ( heightAndSpeed ,
                                nameof ( heightAndSpeed ) ) ;

        return _factory ( deskMover ,
                          executor ,
                          heightAndSpeed ) ;
    }
}