using System ;
using System.Reactive.Concurrency ;
using System.Reactive.Linq ;
using Autofac.Extras.DynamicProxy ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Core.Interfaces.DevicesDiscovery ;

namespace Idasen.BluetoothLE.Core ;

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