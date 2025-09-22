using Autofac.Extras.DynamicProxy ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Characteristics.Common ;
using Idasen.BluetoothLE.Characteristics.Interfaces.Characteristics ;
using Idasen.BluetoothLE.Core ;
using Idasen.BluetoothLE.Linak.Interfaces ;
using Serilog ;

namespace Idasen.BluetoothLE.Linak.Control ;

/// <inheritdoc />
[Intercept ( typeof ( LogAspect ) ) ]
public class DeskCommandExecutor
    : IDeskCommandExecutor
{
    public delegate IDeskCommandExecutor Factory ( IControl control ) ;

    private readonly IControl _control ;
    private readonly IErrorManager _errorManager ;

    private readonly ILogger _logger ;
    private readonly IDeskCommandsProvider _provider ;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DeskCommandExecutor" /> class.
    /// </summary>
    public DeskCommandExecutor ( ILogger logger ,
                                 IErrorManager errorManager ,
                                 IDeskCommandsProvider provider ,
                                 IControl control )
    {
        ArgumentNullException.ThrowIfNull ( logger ) ;
        ArgumentNullException.ThrowIfNull ( provider ) ;
        ArgumentNullException.ThrowIfNull ( control ) ;
        ArgumentNullException.ThrowIfNull ( errorManager ) ;

        _logger = logger ;
        _errorManager = errorManager ;
        _provider = provider ;
        _control = control ;
    }

    /// <inheritdoc />
    public Task < bool > Up ( ) => Execute ( DeskCommands.MoveUp ) ;

    /// <inheritdoc />
    public Task < bool > Down ( ) => Execute ( DeskCommands.MoveDown ) ;

    /// <inheritdoc />
    public Task < bool > Stop ( ) => Execute ( DeskCommands.MoveStop ) ;

    private async Task < bool > Execute ( DeskCommands deskCommand )
    {
        if ( ! _provider.TryGetValue ( deskCommand ,
                                       out var bytes ) )
        {
            _logger.Error ( "Failed for unknown command {Command}" , deskCommand ) ;

            return false ;
        }

        var result = await _control.TryWriteRawControl2 ( bytes )
                                   .ConfigureAwait ( false ) ;

        if ( ! result )
        {
            ExecutionFailed ( deskCommand ) ;
        }

        return result ;
    }

    private void ExecutionFailed ( DeskCommands deskCommand )
    {
        _logger.Error ( "Failed for '{Command}' command. {Hint}" ,
                        deskCommand ,
                        Constants.CheckAndEnableBluetooth ) ;

        _errorManager.PublishForMessage ( Constants.CheckAndEnableBluetooth ) ;
    }
}