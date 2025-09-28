namespace Idasen.BluetoothLE.Linak.Tests ;

using Characteristics.Interfaces.Characteristics ;
using Common.Tests ;
using FluentAssertions ;
using Interfaces ;
using Linak.Control ;
using NSubstitute ;

[ TestClass ]
public class DeskCommandExecutorFactoryTests
{
    private IControl _control = null! ;
    private DeskCommandExecutor.Factory _factory = null! ;

    [ TestInitialize ]
    public void Initialize ( )
    {
        _factory = TestFactory ;

        _control = Substitute.For < IControl > ( ) ;
    }

    private IDeskCommandExecutor TestFactory ( IControl executor ) => Substitute.For < IDeskCommandExecutor > ( ) ;

    [ TestMethod ]
    public void Create_ForControlNull_Throws ( )
    {
        Action action = ( ) => { CreateSut ( ).Create ( null! ) ; } ;

        action.Should ( )
              .Throw < ArgumentNullException > ( )
              .WithParameter ( "control" ) ;
    }

    [ TestMethod ]
    public void CreateForInvoked_ReturnsInstance ( )
    {
        CreateSut ( ).Create ( _control )
                     .Should ( )
                     .NotBeNull ( ) ;
    }

    private DeskCommandExecutorFactory CreateSut ( ) => new ( _factory ) ;
}
