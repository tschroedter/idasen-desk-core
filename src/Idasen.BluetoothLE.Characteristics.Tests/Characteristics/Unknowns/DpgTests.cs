using FluentAssertions ;
using Idasen.BluetoothLE.Characteristics.Characteristics.Unknowns ;

namespace Idasen.BluetoothLE.Characteristics.Tests.Characteristics.Unknowns ;

[ TestClass ]
public class DpgTests
{
    [ TestMethod ]
    public void RawDpg_ForInvoked_Empty ( )
    {
        using var sut = CreateSut();

        sut.RawDpg
           .Should ( )
           .BeEmpty ( ) ;
    }

    private Dpg CreateSut ( ) => new ( ) ;
}
