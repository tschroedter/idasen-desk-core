namespace Idasen.BluetoothLE.Characteristics.Tests.Characteristics.Unknowns ;

using BluetoothLE.Characteristics.Characteristics.Unknowns ;
using FluentAssertions ;

[ TestClass ]
public class GenericAttributeTests
{
    [ TestMethod ]
    public void RawServiceChanged_ForInvoked_Empty ( )
    {
        CreateSut ( ).RawServiceChanged
                     .Should ( )
                     .BeEmpty ( ) ;
    }

    private GenericAttribute CreateSut ( ) => new ( ) ;
}
