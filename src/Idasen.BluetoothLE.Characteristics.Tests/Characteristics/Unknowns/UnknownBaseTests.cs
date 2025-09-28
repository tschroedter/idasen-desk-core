using FluentAssertions ;
using Idasen.BluetoothLE.Characteristics.Characteristics.Unknowns ;

namespace Idasen.BluetoothLE.Characteristics.Tests.Characteristics.Unknowns ;

[ TestClass ]
public class UnknownBaseTests
{
    [ TestMethod ]
    public void Initialize_ForInvoked_DoesNothing ( )
    {
        Action action = ( ) => CreateSut ( ).Initialize < object > ( ) ;

        action.Should ( )
              .NotThrow < Exception > ( ) ;
    }

    [ TestMethod ]
    public void Refresh_ForInvoked_DoesNothing ( )
    {
        Action action = ( ) => CreateSut ( ).Refresh ( ) ;

        action.Should ( )
              .NotThrow < Exception > ( ) ;
    }

    private UnknownBase CreateSut ( ) => new ( ) ;
}
