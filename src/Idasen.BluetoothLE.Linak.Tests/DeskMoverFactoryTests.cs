namespace Idasen.BluetoothLE.Linak.Tests ;

using Common.Tests ;
using FluentAssertions ;
using Interfaces ;
using Linak.Control ;
using NSubstitute ;

[ TestClass ]
public class DeskMoverFactoryTests
{
    private IDeskCommandExecutor _executor = null! ;
    private DeskMover.Factory _factory = null! ;
    private IDeskHeightAndSpeed _heightAndSpeed = null! ;

    [ TestInitialize ]
    public void Initialize ( )
    {
        _factory = TestFactory ;

        _executor = Substitute.For < IDeskCommandExecutor > ( ) ;
        _heightAndSpeed = Substitute.For < IDeskHeightAndSpeed > ( ) ;
    }

    private IDeskMover TestFactory ( IDeskCommandExecutor executor ,
                                     IDeskHeightAndSpeed heightAndSpeed ) =>
        Substitute.For < IDeskMover > ( ) ;

    [ TestMethod ]
    public void Create_ForExecutorNull_Throws ( )
    {
        Action action = ( ) =>
                        {
                            CreateSut ( ).Create ( null! ,
                                                   _heightAndSpeed ) ;
                        } ;

        action.Should ( )
              .Throw < ArgumentNullException > ( )
              .WithParameter ( "executor" ) ;
    }

    [ TestMethod ]
    public void CreateForInvoked_ReturnsInstance ( )
    {
        CreateSut ( ).Create ( _executor ,
                               _heightAndSpeed )
                     .Should ( )
                     .NotBeNull ( ) ;
    }

    private DeskMoverFactory CreateSut ( ) => new ( _factory ) ;
}
