namespace Idasen.BluetoothLE.Characteristics.Characteristics ;

using Aop.Aspects ;
using Autofac ;
using Autofac.Extras.DynamicProxy ;
using Core ;
using Core.Interfaces.ServicesDiscovery ;
using Interfaces.Characteristics ;

/// <summary>
///     Factory for creating characteristic instances using Autofac, injecting the required
///     <see cref="IDevice" /> into the constructor via a named parameter.
/// </summary>
[ Intercept ( typeof ( LogAspect ) ) ]
public sealed class CharacteristicBaseFactory
    : ICharacteristicBaseFactory
{
    private readonly ILifetimeScope _scope ;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CharacteristicBaseFactory" /> class.
    /// </summary>
    /// <param name="scope">The Autofac lifetime scope used to resolve instances.</param>
    public CharacteristicBaseFactory ( ILifetimeScope scope )
    {
        Guard.ArgumentNotNull ( scope ,
                                nameof ( scope ) ) ;

        _scope = scope ;
    }

    /// <summary>
    ///     Creates an instance of <typeparamref name="T" /> providing the specified <see cref="IDevice" />.
    /// </summary>
    /// <typeparam name="T">The service type to resolve.</typeparam>
    /// <param name="device">The device to pass to the resolved constructor.</param>
    /// <returns>An instance of <typeparamref name="T" />.</returns>
    public T Create<T> ( IDevice device ) where T : notnull
    {
        Guard.ArgumentNotNull ( device ,
                                nameof ( device ) ) ;

        T instance = _scope.Resolve < T > ( new NamedParameter ( "device" ,
                                                                 device ) ) ;

        return instance ;
    }
}
