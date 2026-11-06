using FluentAssertions ;
using Idasen.BluetoothLE.Characteristics.Common ;

namespace Idasen.BluetoothLE.Characteristics.Tests.Common ;

[ TestClass ]
public class NotInitializeExceptionTests
{
    [ TestMethod ]
    public void Constructor_ForInvoked_SetsMessage ( )
    {
        var message = "msg" ;
        var sut = new NotInitializeException ( message ) ;

        sut.Message
           .Should ( )
           .Be ( message ) ;
    }
}
