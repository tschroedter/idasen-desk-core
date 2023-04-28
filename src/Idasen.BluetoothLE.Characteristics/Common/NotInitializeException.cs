using System ;
using Idasen.BluetoothLE.Core ;

namespace Idasen.BluetoothLE.Characteristics.Common
{
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
}