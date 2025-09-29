using Autofac.Extras.DynamicProxy ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery.Wrappers ;

namespace Idasen.BluetoothLE.Core.ServicesDiscovery ;

/// <inheritdoc />
[ Intercept ( typeof ( LogAspect ) ) ]
public class GattServices
    : IGattServices
{
    private readonly Dictionary < IGattDeviceServiceWrapper , IGattCharacteristicsResultWrapper > _dictionary =
        new( ) ;

    private readonly object _padlock = new( ) ;

    /// <summary>
    ///     Gets the number of items in the dictionary.
    /// </summary>
    public int Count
    {
        get
        {
            lock ( _padlock )
            {
                return _dictionary.Count ;
            }
        }
    }

    /// <summary>
    ///     Gets or sets the characteristics result for the specified service.
    ///     Setting a new value disposes the old one to avoid leaks.
    /// </summary>
    public IGattCharacteristicsResultWrapper this [ IGattDeviceServiceWrapper service ]
    {
        get
        {
            lock ( _padlock )
            {
                return _dictionary [ service ] ;
            }
        }
        set
        {
            Guard.ArgumentNotNull ( value ,
                                    nameof ( value ) ) ;

            lock ( _padlock )
            {
                if ( _dictionary.TryGetValue ( service ,
                                               out var existing ) )
                    // Dispose previously stored characteristics to avoid leaks
                    existing.Dispose ( ) ;

                _dictionary [ service ] = value ;
            }
        }
    }

    /// <summary>
    ///     Clears the dictionary and disposes entries.
    /// </summary>
    public void Clear ( )
    {
        DisposeEntries ( ) ;

        lock ( _padlock )
        {
            _dictionary.Clear ( ) ;
        }
    }

    /// <summary>
    ///     Disposes stored entries (service and characteristics).
    /// </summary>
    public void Dispose ( )
    {
        DisposeEntries ( ) ;

        GC.SuppressFinalize ( this ) ;
    }

    /// <summary>
    ///     Gets a read-only view of the dictionary.
    /// </summary>
    public IReadOnlyDictionary < IGattDeviceServiceWrapper , IGattCharacteristicsResultWrapper >
        ReadOnlyDictionary
    {
        get
        {
            lock ( _padlock )
            {
                return _dictionary ;
            }
        }
    }

    private void DisposeEntries ( )
    {
        lock ( _padlock )
        {
            foreach ( var kvp in _dictionary )
            {
                // Dispose value (characteristics) first, then the service
                kvp.Value.Dispose ( ) ;
                kvp.Key.Dispose ( ) ;
            }
        }
    }
}