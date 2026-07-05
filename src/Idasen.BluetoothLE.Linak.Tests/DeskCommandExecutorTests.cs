using FluentAssertions ;
using Idasen.BluetoothLE.Characteristics.Interfaces.Characteristics ;
using Idasen.BluetoothLE.Common.Tests ;
using Idasen.BluetoothLE.Linak.Control ;
using Idasen.BluetoothLE.Linak.Interfaces ;
using NSubstitute ;
using Serilog ;
using Serilog.Core ;

namespace Idasen.BluetoothLE.Linak.Tests ;

[ TestClass ]
public class DeskCommandExecutorTests
{
    [ TestMethod ]
    public void Constructor_ForLoggerNull_Throws ( )
    {
        var errorManager = Substitute.For < IErrorManager > ( ) ;
        var provider     = Substitute.For < IDeskCommandsProvider > ( ) ;
        var control      = Substitute.For < IControl > ( ) ;

        var action = ( ) => new DeskCommandExecutor ( null! ,
                                                      errorManager ,
                                                      provider ,
                                                      control ) ;

        action.Should ( )
              .Throw < ArgumentNullException > ( )
              .WithParameter ( "logger" ) ;
    }

    [ TestMethod ]
    public void Constructor_ForProviderNull_Throws ( )
    {
        var logger       = Logger.None ;
        var errorManager = Substitute.For < IErrorManager > ( ) ;
        var control      = Substitute.For < IControl > ( ) ;

        var action = ( ) => new DeskCommandExecutor ( logger ,
                                                      errorManager ,
                                                      null! ,
                                                      control ) ;

        action.Should ( )
              .Throw < ArgumentNullException > ( )
              .WithParameter ( "provider" ) ;
    }

    [ TestMethod ]
    public void Constructor_ForControlNull_Throws ( )
    {
        var logger       = Logger.None ;
        var errorManager = Substitute.For < IErrorManager > ( ) ;
        var provider     = Substitute.For < IDeskCommandsProvider > ( ) ;

        var action = ( ) => new DeskCommandExecutor ( logger ,
                                                      errorManager ,
                                                      provider ,
                                                      null! ) ;

        action.Should ( )
              .Throw < ArgumentNullException > ( )
              .WithParameter ( "control" ) ;
    }

    [ TestMethod ]
    public void Constructor_ForErrorManagerNull_Throws ( )
    {
        var logger   = Logger.None ;
        var provider = Substitute.For < IDeskCommandsProvider > ( ) ;
        var control  = Substitute.For < IControl > ( ) ;

        var action = ( ) => new DeskCommandExecutor ( logger ,
                                                      null! ,
                                                      provider ,
                                                      control ) ;

        action.Should ( )
              .Throw < ArgumentNullException > ( )
              .WithParameter ( "errorManager" ) ;
    }

    [ TestMethod ]
    public async Task Up_ForInvokedWithUnknownCommand_ReturnsFalse ( )
    {
        var provider = Substitute.For < IDeskCommandsProvider > ( ) ;
        var control  = Substitute.For < IControl > ( ) ;
        var sut      = CreateSut ( provider : provider ,
                                   control : control ) ;

        provider.TryGetValue ( DeskCommands.MoveUp ,
                               out Arg.Any < IEnumerable < byte > > ( ) )
                .Returns ( x =>
                           {
                               x [ 1 ] = null ;

                               return false ;
                           } ) ;
        await sut.Up ( ) ;

        await control.DidNotReceive ( )
                     .TryWriteRawControl2 ( Arg.Any < IEnumerable < byte > > ( ) ) ;
    }

    [ TestMethod ]
    public async Task Up_ForInvoked_ReturnsTrueForSuccess ( )
    {
        var provider = Substitute.For < IDeskCommandsProvider > ( ) ;
        var control  = Substitute.For < IControl > ( ) ;
        var sut      = CreateSut ( provider : provider ,
                                   control : control ) ;

        var bytes = new byte [ ]
                    {
                        0 ,
                        1
                    } ;

        provider.TryGetValue ( DeskCommands.MoveUp ,
                               out Arg.Any < IEnumerable < byte > > ( ) )
                .Returns ( x =>
                           {
                               x [ 1 ] = bytes ;

                               return true ;
                           } ) ;

        control.TryWriteRawControl2 ( bytes )
               .Returns ( true ) ;

        var actual = await sut.Up ( ) ;

        actual.Should ( )
              .BeTrue ( ) ;
    }

    [ TestMethod ]
    public async Task Up_ForInvoked_CallsControl ( )
    {
        var provider = Substitute.For < IDeskCommandsProvider > ( ) ;
        var control  = Substitute.For < IControl > ( ) ;
        var sut      = CreateSut ( provider : provider ,
                                   control : control ) ;

        var bytes = new byte [ ]
                    {
                        0 ,
                        1
                    } ;

        provider.TryGetValue ( DeskCommands.MoveUp ,
                               out Arg.Any < IEnumerable < byte > > ( ) )
                .Returns ( x =>
                           {
                               x [ 1 ] = bytes ;

                               return true ;
                           } ) ;
        await sut.Up ( ) ;

        await control.Received ( )
                     .TryWriteRawControl2 ( bytes ) ;
    }

    [ TestMethod ]
    public async Task Down_ForInvokedWithUnknownCommand_ReturnsFalse ( )
    {
        var provider = Substitute.For < IDeskCommandsProvider > ( ) ;
        var control  = Substitute.For < IControl > ( ) ;
        var sut      = CreateSut ( provider : provider ,
                                   control : control ) ;

        provider.TryGetValue ( DeskCommands.MoveDown ,
                               out Arg.Any < IEnumerable < byte > > ( ) )
                .Returns ( x =>
                           {
                               x [ 1 ] = null ;

                               return false ;
                           } ) ;
        await sut.Down ( ) ;

        await control.DidNotReceive ( )
                     .TryWriteRawControl2 ( Arg.Any < IEnumerable < byte > > ( ) ) ;
    }

    [ TestMethod ]
    public async Task Down_ForInvoked_ReturnsTrueForSuccess ( )
    {
        var provider = Substitute.For < IDeskCommandsProvider > ( ) ;
        var control  = Substitute.For < IControl > ( ) ;
        var sut      = CreateSut ( provider : provider ,
                                   control : control ) ;

        var bytes = new byte [ ]
                    {
                        0 ,
                        1
                    } ;

        provider.TryGetValue ( DeskCommands.MoveDown ,
                               out Arg.Any < IEnumerable < byte > > ( ) )
                .Returns ( x =>
                           {
                               x [ 1 ] = bytes ;

                               return true ;
                           } ) ;

        control.TryWriteRawControl2 ( bytes )
               .Returns ( true ) ;

        var actual = await sut.Down ( ) ;

        actual.Should ( )
              .BeTrue ( ) ;
    }

    [ TestMethod ]
    public async Task Down_ForInvoked_CallsControl ( )
    {
        var provider = Substitute.For < IDeskCommandsProvider > ( ) ;
        var control  = Substitute.For < IControl > ( ) ;
        var sut      = CreateSut ( provider : provider ,
                                   control : control ) ;

        var bytes = new byte [ ]
                    {
                        0 ,
                        1
                    } ;

        provider.TryGetValue ( DeskCommands.MoveDown ,
                               out Arg.Any < IEnumerable < byte > > ( ) )
                .Returns ( x =>
                           {
                               x [ 1 ] = bytes ;

                               return true ;
                           } ) ;
        await sut.Down ( ) ;

        await control.Received ( )
                     .TryWriteRawControl2 ( bytes ) ;
    }

    [ TestMethod ]
    public async Task Stop_ForInvokedWithUnknownCommand_ReturnsFalse ( )
    {
        var provider = Substitute.For < IDeskCommandsProvider > ( ) ;
        var control  = Substitute.For < IControl > ( ) ;
        var sut      = CreateSut ( provider : provider ,
                                   control : control ) ;

        provider.TryGetValue ( DeskCommands.MoveStop ,
                               out Arg.Any < IEnumerable < byte > > ( ) )
                .Returns ( x =>
                           {
                               x [ 1 ] = null ;

                               return false ;
                           } ) ;
        await sut.StopMovement ( ) ;

        await control.DidNotReceive ( )
                     .TryWriteRawControl2 ( Arg.Any < IEnumerable < byte > > ( ) ) ;
    }

    [ TestMethod ]
    public async Task Stop_ForInvoked_ReturnsTrueForSuccess ( )
    {
        var provider = Substitute.For < IDeskCommandsProvider > ( ) ;
        var control  = Substitute.For < IControl > ( ) ;
        var sut      = CreateSut ( provider : provider ,
                                   control : control ) ;

        var bytes = new byte [ ]
                    {
                        0 ,
                        1
                    } ;

        provider.TryGetValue ( DeskCommands.MoveStop ,
                               out Arg.Any < IEnumerable < byte > > ( ) )
                .Returns ( x =>
                           {
                               x [ 1 ] = bytes ;

                               return true ;
                           } ) ;

        control.TryWriteRawControl2 ( bytes )
               .Returns ( true ) ;

        var actual = await sut.StopMovement ( ) ;

        actual.Should ( )
              .BeTrue ( ) ;
    }

    [ TestMethod ]
    public async Task Stop_ForInvoked_CallsControl ( )
    {
        var provider = Substitute.For < IDeskCommandsProvider > ( ) ;
        var control  = Substitute.For < IControl > ( ) ;
        var sut      = CreateSut ( provider : provider ,
                                   control : control ) ;

        var bytes = new byte [ ]
                    {
                        0 ,
                        1
                    } ;

        provider.TryGetValue ( DeskCommands.MoveStop ,
                               out Arg.Any < IEnumerable < byte > > ( ) )
                .Returns ( x =>
                           {
                               x [ 1 ] = bytes ;

                               return true ;
                           } ) ;
        await sut.StopMovement ( ) ;

        await control.Received ( )
                     .TryWriteRawControl2 ( bytes ) ;
    }

    private static DeskCommandExecutor CreateSut ( ILogger?               logger        = null ,
                                                   IErrorManager?         errorManager  = null ,
                                                   IDeskCommandsProvider? provider      = null ,
                                                   IControl?              control       = null )
    {
        logger       ??= Logger.None ;
        errorManager ??= Substitute.For < IErrorManager > ( ) ;
        provider     ??= Substitute.For < IDeskCommandsProvider > ( ) ;
        control      ??= Substitute.For < IControl > ( ) ;

        return new DeskCommandExecutor ( logger ,
                                         errorManager ,
                                         provider ,
                                         control ) ;
    }
}
