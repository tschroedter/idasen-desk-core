using Autofac ;
using Autofac.Extras.DynamicProxy ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Characteristics.Interfaces.Characteristics ;
using Idasen.BluetoothLE.Core ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery ;

namespace Idasen.BluetoothLE.Characteristics.Characteristics ;

[ Intercept ( typeof ( LogAspect ) ) ]
public class CharacteristicBaseFactory
    : ICharacteristicBaseFactory
{
    private readonly ILifetimeScope _scope ;

    public CharacteristicBaseFactory ( ILifetimeScope scope )
    {
        Guard.ArgumentNotNull ( scope ,
                                nameof ( scope ) ) ;

        _scope = scope ;
    }

    public T Create<T> ( IDevice device ) where T : notnull
    {
        var instance = _scope.Resolve < T > ( new NamedParameter ( "device" ,
                                                                   device ) ) ;

        return instance ;
    }
}