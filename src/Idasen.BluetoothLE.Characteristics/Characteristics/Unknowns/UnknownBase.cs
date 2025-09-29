using Idasen.BluetoothLE.Characteristics.Interfaces.Characteristics ;

namespace Idasen.BluetoothLE.Characteristics.Characteristics.Unknowns ;

public class UnknownBase
    : ICharacteristicBase
{
    protected static readonly IEnumerable < byte > RawArrayEmpty = Enumerable.Empty < byte > ( )
                                                                             .ToArray ( ) ;

    private bool _disposed ;

    public T ? Initialize < T > ( )
        where T : class
    {
        return this as T ;
    }

    public Task Refresh ( )
    {
        return Task.FromResult ( false ) ;
    }

    public void Dispose ( )
    {
        Dispose ( true ) ;
    }

    protected virtual void Dispose ( bool disposing )
    {
        if ( _disposed )
            return ;

        _disposed = true ;
    }
}