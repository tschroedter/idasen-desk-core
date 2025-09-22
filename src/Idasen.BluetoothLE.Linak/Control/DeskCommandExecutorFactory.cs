using Autofac.Extras.DynamicProxy ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Characteristics.Interfaces.Characteristics ;
using Idasen.BluetoothLE.Linak.Interfaces ;

namespace Idasen.BluetoothLE.Linak.Control ;


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