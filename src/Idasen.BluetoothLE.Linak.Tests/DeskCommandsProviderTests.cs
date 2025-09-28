namespace Idasen.BluetoothLE.Linak.Tests ;

using FluentAssertions ;
using Linak.Control ;

[ TestClass ]
public class DeskCommandsProviderTests
{
    [ TestMethod ]
    public void TryGetValue_ForDeskCommandsMoveDown_ReturnsTrue ( )
    {
        CreateSut ( ).TryGetValue ( DeskCommands.MoveDown ,
                                    out _ )
                     .Should ( )
                     .BeTrue ( ) ;
    }

    [ TestMethod ]
    public void TryGetValue_ForDeskCommandsMoveDown_ReturnsBytes ( )
    {
        // ReSharper disable once UseUtf8StringLiteral
        var expected = new byte [ ] { 0x46 , 0x00 } ;

        CreateSut ( ).TryGetValue ( DeskCommands.MoveDown ,
                                    out IEnumerable < byte > bytes ) ;

        bytes.Should ( )
             .BeEquivalentTo ( expected ) ;
    }

    [ TestMethod ]
    public void TryGetValue_ForDeskCommandsMoveUp_ReturnsTrue ( )
    {
        CreateSut ( ).TryGetValue ( DeskCommands.MoveUp ,
                                    out _ )
                     .Should ( )
                     .BeTrue ( ) ;
    }

    [ TestMethod ]
    public void TryGetValue_ForDeskCommandsMoveUp_ReturnsBytes ( )
    {
        // ReSharper disable once UseUtf8StringLiteral
        var expected = new byte [ ] { 0x47 , 0x00 } ;

        CreateSut ( ).TryGetValue ( DeskCommands.MoveUp ,
                                    out IEnumerable < byte > bytes ) ;

        bytes.Should ( )
             .BeEquivalentTo ( expected ) ;
    }

    [ TestMethod ]
    public void TryGetValue_ForDeskCommandsMoveStop_ReturnsTrue ( )
    {
        CreateSut ( ).TryGetValue ( DeskCommands.MoveStop ,
                                    out _ )
                     .Should ( )
                     .BeTrue ( ) ;
    }

    [ TestMethod ]
    public void TryGetValue_ForDeskCommandsMoveStop_ReturnsBytes ( )
    {
        // ReSharper disable once UseUtf8StringLiteral
        var expected = new byte [ ] { 0x48 , 0x00 } ;

        CreateSut ( ).TryGetValue ( DeskCommands.MoveStop ,
                                    out IEnumerable < byte > bytes ) ;

        bytes.Should ( )
             .BeEquivalentTo ( expected ) ;
    }

    private static DeskCommandsProvider CreateSut ( ) => new ( ) ;
}
