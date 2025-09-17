using Autofac.Extras.DynamicProxy ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Characteristics.Interfaces.Characteristics ;
using Idasen.BluetoothLE.Core ;
using Idasen.BluetoothLE.Linak.Interfaces ;

namespace Idasen.BluetoothLE.Linak.Control ;

[ Intercept ( typeof ( LogAspect ) ) ]
/// <summary>
///     Factory for creating <see cref="IDeskCommandExecutor"/> instances.
/// </summary>
public class DeskCommandExecutorFactory
    : IDeskCommandExecutorFactory
{
    private readonly DeskCommandExecutor.Factory _factory ;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DeskCommandExecutorFactory"/> class.
    /// </summary>
    /// <param name="factory">The factory delegate.</param>
    public DeskCommandExecutorFactory ( DeskCommandExecutor.Factory factory )
    {
        Guard.ArgumentNotNull ( factory ,
                                nameof ( factory ) ) ;

        _factory = factory ;
    }

    /// <inheritdoc />
    /// <summary>
    ///     Creates a new instance of <see cref="IDeskCommandExecutor"/>.
    /// </summary>
    /// <param name="control">The control.</param>
    /// <returns>A new instance of <see cref="IDeskCommandExecutor"/>.</returns>
    public IDeskCommandExecutor Create ( IControl control )
    {
        Guard.ArgumentNotNull ( control ,
                                nameof ( control ) ) ;

        return _factory ( control ) ;
    }
}