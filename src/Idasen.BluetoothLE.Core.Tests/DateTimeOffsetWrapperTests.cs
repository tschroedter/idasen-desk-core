using System.Globalization ;
using FluentAssertions ;

namespace Idasen.BluetoothLE.Core.Tests ;

[ TestClass ]
public class DateTimeOffsetWrapperTests
{
    [ TestMethod ]
    public void Now_ReturnsNewWrapper ( )
    {
        var now = new DateTimeOffsetWrapper ( DateTimeOffset.UtcNow ) ;

        var next = now.Now ;

        next.Should ( ).NotBeNull ( ) ;
        next.Ticks.Should ( ).BeGreaterThan ( 0 ) ;
    }

    [ TestMethod ]
    public void ToString_OFormat_Invariant_ReturnsIso ( )
    {
        var dt = new DateTimeOffset (
                                     2025 ,
                                     01 ,
                                     02 ,
                                     03 ,
                                     04 ,
                                     05 ,
                                     TimeSpan.Zero ) ;
        var wrapper = new DateTimeOffsetWrapper ( dt ) ;

        var text = wrapper.ToString (
                                     "O" ,
                                     CultureInfo.InvariantCulture ) ;

        text.Should ( ).Be ( "2025-01-02T03:04:05.0000000+00:00" ) ;
    }
}
