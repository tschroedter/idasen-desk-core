using Autofac.Extras.DynamicProxy ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery.Wrappers ;

namespace Idasen.BluetoothLE.Core.ServicesDiscovery
{
    [ Intercept ( typeof ( LogAspect ) ) ]
    public class GattServicesDictionary
        : IGattServicesDictionary
    {
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
                    if ( _dictionary.TryGetValue ( service , out var existing ) )
                    {
                        // Dispose previously stored characteristics to avoid leaks
                        existing.Dispose ( ) ;
                    }

                    _dictionary [ service ] = value ;
                }
            }
        }

        public void Clear ( )
        {
            DisposeEntries ( ) ;

            lock ( _padlock )
            {
                _dictionary.Clear ( ) ;
            }
        }

        public void Dispose ( )
        {
            DisposeEntries ( ) ;
        }

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

        private readonly Dictionary < IGattDeviceServiceWrapper , IGattCharacteristicsResultWrapper > _dictionary =
            new( ) ;

        private readonly object _padlock = new( ) ;
    }
}