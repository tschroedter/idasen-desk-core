using FluentAssertions ;
using Idasen.BluetoothLE.Characteristics.Characteristics.Unknowns ;

namespace Idasen.BluetoothLE.Characteristics.Tests.Characteristics.Unknowns ;

[ TestClass ]
public class ControlTests
{
    [ TestMethod ]
    public void RawControl2_ForInvoked_Empty ( )
    {
        using var sut = CreateSut ( ) ;

        sut.RawControl2
           .Should ( )
           .BeEmpty ( ) ;
    }

    [ TestMethod ]
    public void RawControl3_ForInvoked_Empty ( )
    {
        using var sut = CreateSut ( ) ;

        sut.RawControl3
           .Should ( )
           .BeEmpty ( ) ;
    }

    [ TestMethod ]
    public async Task TryWriteRawControl2_ForInvoked_ReturnsFalse ( )
    {
        using var sut = CreateSut ( ) ;

        var result = await sut.TryWriteRawControl2 ( [] ) ;

        result.Should ( )
              .BeFalse ( ) ;
    }

    private static Control CreateSut ( )
    {
        return new Control ( ) ;
    }
}