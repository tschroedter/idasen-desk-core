using Autofac.Extras.DynamicProxy ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Core ;
using Idasen.BluetoothLE.Linak.Interfaces ;

namespace Idasen.BluetoothLE.Linak.Control ;

[ Intercept ( typeof ( LogAspect ) ) ]
public class InitialHeightAndSpeedProviderFactory
    : IInitialHeightAndSpeedProviderFactory
{
    private readonly InitialHeightProvider.Factory _factory ;

    public InitialHeightAndSpeedProviderFactory ( InitialHeightProvider.Factory factory )
    {
        Guard.ArgumentNotNull ( factory ,
                                nameof ( factory ) ) ;

        _factory = factory ;
    }

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