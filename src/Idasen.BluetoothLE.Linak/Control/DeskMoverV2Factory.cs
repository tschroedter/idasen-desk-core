using Autofac.Extras.DynamicProxy ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Linak.Interfaces ;

namespace Idasen.BluetoothLE.Linak.Control;

/// <inheritdoc />
[Intercept(typeof(LogAspect))]
public class DeskMoverV2Factory
    : IDeskMoverV2Factory
{
    private readonly DeskMoverV2.Factory _factory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DeskMoverV2Factory" /> class.
    /// </summary>
    /// <param name="factory">The IoC factory delegate for <see cref="DeskMoverV2" />.</param>
    public DeskMoverV2Factory(DeskMoverV2.Factory factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        _factory = factory;
    }

    /// <inheritdoc />
    public IDeskMover Create(IDeskCommandExecutor executor,
                             IDeskHeightAndSpeed  heightAndSpeed)
    {
        ArgumentNullException.ThrowIfNull(executor);
        ArgumentNullException.ThrowIfNull(heightAndSpeed);

        return _factory(executor,
                        heightAndSpeed);
    }
}
