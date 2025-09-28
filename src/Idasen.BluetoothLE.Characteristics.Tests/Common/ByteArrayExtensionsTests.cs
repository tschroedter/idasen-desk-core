namespace Idasen.BluetoothLE.Characteristics.Tests.Common ;

using BluetoothLE.Characteristics.Common ;
using BluetoothLE.Common.Tests ;
using FluentAssertions ;

[ TestClass ]
public class ByteArrayExtensionsTests
{
    [ TestMethod ]
    public void ToHex_ForBytesNull_Throws ( )
    {
        byte [ ] bytes = null! ;

        // ReSharper disable once AssignNullToNotNullAttribute
        Action action = ( ) => { bytes.ToHex ( ) ; } ;

        action.Should ( )
              .Throw < ArgumentNullException > ( )
              .WithParameter ( "array" ) ;
    }

    [ TestMethod ]
    public void ToHex_ForBytes_Instance ( )
    {
        var bytes = new byte [ ] { 1 , 2 , 3 } ;

        bytes.ToHex ( )
             .Should ( )
             .Be ( "01-02-03" ) ;
    }
}
