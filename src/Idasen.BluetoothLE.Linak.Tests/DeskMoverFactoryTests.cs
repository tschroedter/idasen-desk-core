using FluentAssertions ;
using Idasen.BluetoothLE.Common.Tests ;
using Idasen.BluetoothLE.Linak.Control ;
using Idasen.BluetoothLE.Linak.Interfaces ;
using NSubstitute ;
using Serilog ;

namespace Idasen.BluetoothLE.Linak.Tests ;

[ TestClass ]
public class DeskMoverFactoryTests
{
    private IStoppingHeightCalculator             _calculator      = null! ;
    private IDeskCommandExecutor                  _executor        = null! ;
    private DeskMover.Factory                     _factory         = null! ;
    private IDeskHeightAndSpeed                   _heightAndSpeed  = null! ;
    private ILogger                               _logger          = null! ;
    private IDeskMovementMonitorFactory           _monitorFactory  = null! ;
    private IInitialHeightAndSpeedProviderFactory _providerFactory = null! ;

    [ TestInitialize ]
    public void Initialize ( )
    {
        _factory = TestFactory ;

        _executor       = Substitute.For < IDeskCommandExecutor > ( ) ;
        _heightAndSpeed = Substitute.For < IDeskHeightAndSpeed > ( ) ;

        _logger          = Substitute.For < ILogger > ( ) ;
        _providerFactory = Substitute.For < IInitialHeightAndSpeedProviderFactory > ( ) ;
        _monitorFactory  = Substitute.For < IDeskMovementMonitorFactory > ( ) ;
        _calculator      = Substitute.For < IStoppingHeightCalculator > ( ) ;
    }

    private static IDeskMover TestFactory ( IDeskLocationHandlers locationHandlers ,
                                            IDeskMovementHandlers movementHandlers )
    {
        return Substitute.For < IDeskMover > ( ) ;
    }

    [ TestMethod ]
    public void Create_ForExecutorNull_Throws ( )
    {
        var action = ( ) =>
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

    private DeskMoverFactory CreateSut ( )
    {
        return new DeskMoverFactory ( _logger ,
                                      _factory ,
                                      _providerFactory ,
                                      _monitorFactory ,
                                      _calculator ) ;
    }
}