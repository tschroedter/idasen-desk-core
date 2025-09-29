using FluentAssertions ;
using Idasen.BluetoothLE.Characteristics.Characteristics.Unknowns ;

namespace Idasen.BluetoothLE.Characteristics.Tests.Characteristics.Unknowns ;

[ TestClass ]
public class ReferenceInputTests
{
    [ TestMethod ]
    public void Ctrl1_ForInvoked_Empty ( )
    {
        using var sut = CreateSut ( ) ;

        sut.Ctrl1
           .Should ( )
           .BeEmpty ( ) ;
    }

    private static ReferenceInput CreateSut ( )
    {
        return new ReferenceInput ( ) ;
    }
}