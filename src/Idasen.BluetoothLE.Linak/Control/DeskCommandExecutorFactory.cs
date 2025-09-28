namespace Idasen.BluetoothLE.Linak.Control ;

using Aop.Aspects ;
using Autofac.Extras.DynamicProxy ;
using Characteristics.Interfaces.Characteristics ;
using Interfaces ;

[ Intercept ( typeof ( LogAspect ) ) ]
public class DeskCommandExecutorFactory
    : IDeskCommandExecutorFactory
{
    private readonly DeskCommandExecutor.Factory _factory ;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DeskCommandExecutorFactory" /> class.
    /// </summary>
    /// <param name="factory">The factory delegate.</param>
    public DeskCommandExecutorFactory ( DeskCommandExecutor.Factory factory )
    {
        ArgumentNullException.ThrowIfNull ( factory ) ;

        _factory = factory ;
    }

    /// <inheritdoc />
    public IDeskCommandExecutor Create ( IControl control )
    {
        ArgumentNullException.ThrowIfNull ( control ) ;

        return _factory ( control ) ;
    }
}
