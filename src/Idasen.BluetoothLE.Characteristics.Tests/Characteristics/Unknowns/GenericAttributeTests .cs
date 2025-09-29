using FluentAssertions ;
using Idasen.BluetoothLE.Characteristics.Characteristics.Unknowns ;

namespace Idasen.BluetoothLE.Characteristics.Tests.Characteristics.Unknowns ;

[ TestClass ]
public class GenericAttributeTests
{
    [ TestMethod ]
    public void RawServiceChanged_ForInvoked_Empty ( )
    {
        using var sut = CreateSut();

        sut.RawServiceChanged
           .Should ( )
           .BeEmpty ( ) ;
    }

    private GenericAttribute CreateSut ( ) => new ( ) ;
}
