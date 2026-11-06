using FluentAssertions ;
using Idasen.BluetoothLE.Linak.Control ;

namespace Idasen.BluetoothLE.Linak.Tests ;

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
        var expected = "F\0"u8.ToArray ( ) ;

        CreateSut ( ).TryGetValue ( DeskCommands.MoveDown ,
                                    out var bytes ) ;

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
        var expected = "G\0"u8.ToArray ( ) ;

        CreateSut ( ).TryGetValue ( DeskCommands.MoveUp ,
                                    out var bytes ) ;

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
        var expected = "H\0"u8.ToArray ( ) ;

        CreateSut ( ).TryGetValue ( DeskCommands.MoveStop ,
                                    out var bytes ) ;

        bytes.Should ( )
             .BeEquivalentTo ( expected ) ;
    }

    private static DeskCommandsProvider CreateSut ( )
    {
        return new DeskCommandsProvider ( ) ;
    }
}
