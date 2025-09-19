using Autofac.Extras.DynamicProxy ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Core ;
using Idasen.BluetoothLE.Linak.Interfaces ;

namespace Idasen.BluetoothLE.Linak.Control ;

[ Intercept ( typeof ( LogAspect ) ) ]
/// <summary>
///     Factory that creates <see cref="IInitialHeightProvider"/> instances used to retrieve the initial desk height.
/// </summary>
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
        Guard.ArgumentNotNull ( factory ,
                                nameof ( factory ) ) ;

        _factory = factory ;
    }

    /// <inheritdoc />
    public IInitialHeightProvider Create (
        IDeskCommandExecutor executor ,
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