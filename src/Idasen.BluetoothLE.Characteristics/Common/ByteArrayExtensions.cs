using Idasen.BluetoothLE.Core ;

namespace Idasen.BluetoothLE.Characteristics.Common ;

public static class ByteArrayExtensions
{
    public static string ToHex ( this IEnumerable < byte > array )
    {
        Guard.ArgumentNotNull ( array ,
                                nameof ( array ) ) ;

        return BitConverter.ToString ( array.ToArray ( ) ) ;
    }
}
