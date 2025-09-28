namespace Idasen.BluetoothLE.Characteristics.Tests.Characteristics.Unknowns ;

using BluetoothLE.Characteristics.Characteristics.Unknowns ;
using FluentAssertions ;

[ TestClass ]
public class DpgTests
{
    [ TestMethod ]
    public void RawDpg_ForInvoked_Empty ( )
    {
        CreateSut ( ).RawDpg
                     .Should ( )
                     .BeEmpty ( ) ;
    }

    private Dpg CreateSut ( ) => new ( ) ;
}
