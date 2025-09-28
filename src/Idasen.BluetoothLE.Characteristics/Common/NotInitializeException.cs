namespace Idasen.BluetoothLE.Characteristics.Common ;

using Core ;

public class NotInitializeException
    : Exception
{
    public NotInitializeException ( string message )
        : base ( message )
    {
        Guard.ArgumentNotNull ( message ,
                                nameof ( message ) ) ;
    }
}
