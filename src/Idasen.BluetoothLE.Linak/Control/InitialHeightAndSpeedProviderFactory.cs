using Autofac.Extras.DynamicProxy ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Linak.Interfaces ;

namespace Idasen.BluetoothLE.Linak.Control ;

/// <inheritdoc />
[Intercept ( typeof ( LogAspect ) ) ]
public class InitialHeightAndSpeedProviderFactory
    : IInitialHeightAndSpeedProviderFactory
{
    private readonly InitialHeightProvider.Factory _factory ;

    /// <summary>
    ///     Initializes a new instance of the <see cref="InitialHeightAndSpeedProviderFactory" /> class.
    /// </summary>
    /// <param name="factory">The IoC factory delegate for <see cref="InitialHeightProvider" />.</param>
    public InitialHeightAndSpeedProviderFactory ( InitialHeightProvider.Factory factory )
    {
        ArgumentNullException.ThrowIfNull ( factory ) ;

        _factory = factory ;
    }

    /// <inheritdoc />
    public IInitialHeightProvider Create (
        IDeskCommandExecutor executor ,
        IDeskHeightAndSpeed heightAndSpeed )
    {
        ArgumentNullException.ThrowIfNull ( executor ) ;
        ArgumentNullException.ThrowIfNull ( heightAndSpeed ) ;

        return _factory ( executor ,
                          heightAndSpeed ) ;
    }
}