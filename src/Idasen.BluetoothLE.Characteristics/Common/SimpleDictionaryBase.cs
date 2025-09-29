using Idasen.BluetoothLE.Characteristics.Interfaces.Common ;
using Idasen.BluetoothLE.Core ;

namespace Idasen.BluetoothLE.Characteristics.Common ;

public class SimpleDictionaryBase < TKey , TValue >
    : ISimpleDictionary < TKey , TValue >
    where TKey : notnull
{
    private readonly Dictionary < TKey , TValue > _dictionary = new( ) ;

    private readonly object _padlock = new( ) ;

    /// <inheritdoc />
    public TValue this [ TKey key ]
    {
        get
        {
            lock ( _padlock )
            {
                return _dictionary [ key ] ;
            }
        }
        set
        {
            Guard.ArgumentNotNull ( value! ,
                                    nameof ( value ) ) ;

            lock ( _padlock )
            {
                _dictionary [ key ] = value ;
            }
        }
    }

    /// <inheritdoc />
    public void Clear ( )
    {
        lock ( _padlock )
        {
            _dictionary.Clear ( ) ;
        }
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public IEnumerable < string > Keys
    {
        get
        {
            lock ( _padlock )
            {
                // Avoid invalid casts; only include keys that are truly strings
                return _dictionary.Keys
                                  .OfType < string > ( )
                                  .ToArray ( ) ;
            }
        }
    }

    /// <inheritdoc />
    public IReadOnlyDictionary < TKey , TValue > ReadOnlyDictionary
    {
        get
        {
            lock ( _padlock )
            {
                // Create a shallow copy snapshot to avoid exposing internal state
                return new Dictionary < TKey , TValue > ( _dictionary ) ;
            }
        }
    }
}