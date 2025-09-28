namespace Idasen.BluetoothLE.Core ;

using System.Reactive.Concurrency ;
using System.Reactive.Linq ;
using Aop.Aspects ;
using Autofac.Extras.DynamicProxy ;
using Interfaces.DevicesDiscovery ;

/// <summary>
///     Factory that creates observable timers using Rx.
/// </summary>
/// <inheritdoc />
[ Intercept ( typeof ( LogAspect ) ) ]
public sealed class ObservableTimerFactory
    : IObservableTimerFactory
{
    /// <inheritdoc />
    public IObservable < long > Create ( TimeSpan period ,
                                         IScheduler scheduler )
    {
        ArgumentNullException.ThrowIfNull ( scheduler ) ;

        if ( period < TimeSpan.Zero )
        {
            throw new ArgumentOutOfRangeException ( nameof ( period ) ,
                                                    period ,
                                                    "The period must be non-negative." ) ;
        }

        return Observable.Interval ( period ,
                                     scheduler ) ;
    }
}
