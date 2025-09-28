namespace Idasen.BluetoothLE.Linak ;

using System.Diagnostics.CodeAnalysis ;
using System.Reactive ;
using System.Reactive.Linq ;
using JetBrains.Annotations ;

[ ExcludeFromCodeCoverage ]
public static class RxExtensions
{
    /// <summary>
    ///     Subscribes to an observable and executes an asynchronous action for each element.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">The source observable sequence.</param>
    /// <param name="asyncAction">An asynchronous action to run for each element.</param>
    /// <param name="handler">Optional error handler for the subscription.</param>
    [ UsedImplicitly ]
    public static IDisposable SubscribeAsync<T> ( this IObservable < T > source ,
                                                  Func < Task > asyncAction ,
                                                  Action < Exception >? handler = null )
    {
        ArgumentNullException.ThrowIfNull ( source ) ;
        ArgumentNullException.ThrowIfNull ( asyncAction ) ;

        IObservable < Unit > query = source.SelectMany ( _ => Observable.FromAsync ( async ( ) =>
                                                                                     {
                                                                                         await asyncAction ( ).ConfigureAwait ( false ) ;
                                                                                         return Unit.Default ;
                                                                                     } ) ) ;

        return handler == null
                   ? query.Subscribe ( _ => { } ,
                                       _ => { } )
                   : query.Subscribe ( _ => { } ,
                                       handler ) ;
    }

    /// <summary>
    ///     Subscribes to an observable and executes an asynchronous action for each element.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">The source observable sequence.</param>
    /// <param name="asyncAction">An asynchronous action to run for each element.</param>
    /// <param name="handler">Optional error handler for the subscription.</param>
    [ UsedImplicitly ]
    public static IDisposable SubscribeAsync<T> ( this IObservable < T > source ,
                                                  Func < T , Task > asyncAction ,
                                                  Action < Exception >? handler = null )
    {
        ArgumentNullException.ThrowIfNull ( source ) ;
        ArgumentNullException.ThrowIfNull ( asyncAction ) ;

        IObservable < Unit > query = source.SelectMany ( t => Observable.FromAsync ( async ( ) =>
                                                                                     {
                                                                                         await asyncAction ( t ).ConfigureAwait ( false ) ;
                                                                                         return Unit.Default ;
                                                                                     } ) ) ;

        return handler == null
                   ? query.Subscribe ( _ => { } ,
                                       _ => { } )
                   : query.Subscribe ( _ => { } ,
                                       handler ) ;
    }
}
