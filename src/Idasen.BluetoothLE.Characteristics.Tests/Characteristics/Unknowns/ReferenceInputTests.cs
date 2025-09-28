namespace Idasen.BluetoothLE.Characteristics.Tests.Characteristics.Unknowns ;

using BluetoothLE.Characteristics.Characteristics.Unknowns ;
using FluentAssertions ;

[ TestClass ]
public class ReferenceInputTests
{
    [ TestMethod ]
    public void Ctrl1_ForInvoked_Empty ( )
    {
        CreateSut ( ).Ctrl1
                     .Should ( )
                     .BeEmpty ( ) ;
    }

    private ReferenceInput CreateSut ( ) => new ( ) ;
}
