using FluentAssertions ;
using Idasen.BluetoothLE.Characteristics.Characteristics.Unknowns ;

namespace Idasen.BluetoothLE.Characteristics.Tests.Characteristics.Unknowns ;

[ TestClass ]
public class GenericAttributeServiceTests
{
    [ TestMethod ]
    public void RawServiceChanged_ForInvoked_Empty ( )
    {
        using var sut = CreateSut ( ) ;

        sut.RawServiceChanged
           .Should ( )
           .BeEmpty ( ) ;
    }

    private static GenericAttributeService CreateSut ( )
    {
        return new GenericAttributeService ( ) ;
    }
}
