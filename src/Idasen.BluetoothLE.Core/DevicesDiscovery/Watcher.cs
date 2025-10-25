using System.Reactive.Subjects ;
using Autofac.Extras.DynamicProxy ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Core.Interfaces.DevicesDiscovery ;

namespace Idasen.BluetoothLE.Core.DevicesDiscovery ;

/// <inheritdoc cref="IWatcher" />
[ Intercept ( typeof ( LogAspect ) ) ]
public class Watcher
    : IWatcher
{
    private readonly ISubject < DateTime > _startedWatching ;
    private readonly IWrapper              _wrapper ;

    private bool _disposed ;

    public Watcher ( IWrapper              wrapper ,
                     ISubject < DateTime > started )
    {
        Guard.ArgumentNotNull ( wrapper ,
                                nameof ( wrapper ) ) ;
        Guard.ArgumentNotNull ( started ,
                                nameof ( started ) ) ;

        _wrapper         = wrapper ;
        _startedWatching = started ;
    }

    /// <inheritdoc />
    public bool IsListening => _wrapper.Status == Status.Started ;

    /// <inheritdoc />
    public IObservable < DateTime > Started => _startedWatching ;

    /// <inheritdoc />
    public IObservable < DateTime > Stopped => _wrapper.Stopped ;

    /// <inheritdoc />
    public IObservable < IDevice > Received => _wrapper.Received ;

    /// <inheritdoc />
    public void StartListening ( )
    {
        if ( IsListening )
            return ;

        _wrapper.StartListening ( ) ;

        _startedWatching.OnNext ( DateTime.Now ) ;
    }

    /// <inheritdoc />
    public void StopListening ( )
    {
        if ( ! IsListening )
            return ;

        _wrapper.StopListening ( ) ;
    }

    /// <inheritdoc />
    public void Dispose ( )
    {
        Dispose ( true ) ;

        GC.SuppressFinalize ( this ) ;
    }

    protected virtual void Dispose ( bool disposing )
    {
        if ( _disposed )
            return ;

        if ( disposing ) _wrapper.Dispose ( ) ;

        _disposed = true ;
    }
}