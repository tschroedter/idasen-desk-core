namespace Idasen.BluetoothLE.Characteristics.Tests.Common ;

using BluetoothLE.Characteristics.Common ;
using FluentAssertions ;
using Selkie.AutoMocking ;

[ AutoDataTestClass ]
public class NotInitializeExceptionTests
{
    [ AutoDataTestMethod ]
    public void Constructor_ForInvoked_SetsMessage (
        string message )
    {
        var sut = new NotInitializeException ( message ) ;

        sut.Message
           .Should ( )
           .Be ( message ) ;
    }
}
