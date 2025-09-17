using Idasen.BluetoothLE.Core ;
using Serilog ;
using Serilog.Core ;
using Serilog.Events ;

namespace Idasen.Launcher ;

public static class LoggerProvider
{
    private const string LogTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss.ffff} " +
                                       "{Level:u3}] {Message} " +
                                       "(at {Caller}){NewLine}{Exception}" ;

    private static readonly object Sync = new ( ) ;
    private static Logger? _logger ;

    public static ILogger CreateLogger ( string appName ,
                                         string appLogFileName )
    {
        Guard.ArgumentNotNull ( appName ,
                                nameof ( appName ) ) ;
        Guard.ArgumentNotNull ( appLogFileName ,
                                nameof ( appLogFileName ) ) ;

        lock (Sync)
        {
            if ( _logger != null )
            {
                _logger.Debug ( "Using existing logger for '{AppName}' in folder {LogFile}" ,
                                appName ,
                                appLogFileName ) ;

                return _logger ;
            }

            _logger = DoCreateLogger ( appLogFileName ) ;

            _logger.Debug ( "Created logger for '{AppName}' in folder '{LogFile}'" ,
                            appName ,
                            appLogFileName ) ;

            return _logger ;
        }
    }

    private static Logger DoCreateLogger ( string appLogFileName )
    {
        var logFolder = Path.Combine ( AppDomain.CurrentDomain.BaseDirectory ,
                                       "logs" ) ;
        var logFile = CreateFullPathLogFileName ( logFolder ,
                                                  appLogFileName ) ;

        if ( ! Directory.Exists ( logFolder ) )
        {
            Directory.CreateDirectory ( logFolder ) ;
        }

        // Make the path available immediately; the hook will set it again once the file is opened
        LoggingFile.FullPath = logFile ;

#pragma warning disable CA1305
        var loggerConfiguration = new LoggerConfiguration ( )
                                 .MinimumLevel
                                 .Debug ( )
                                 .Enrich
                                 .WithCaller ( )
                                 .WriteTo.Console ( LogEventLevel.Debug ,
                                                    LogTemplate )
                                 .WriteTo.File ( logFile ,
                                                 LogEventLevel.Debug ,
                                                 LogTemplate ,
                                                 hooks : new LoggingFileHooks ( ) ) ;
#pragma warning restore CA1305

        return loggerConfiguration.CreateLogger ( ) ;
    }

    public static void Shutdown ( )
    {
        lock (Sync)
        {
            if ( _logger == null )
            {
                return ;
            }

            // Dispose the instance logger to flush/close sinks
            _logger.Dispose ( ) ;
            _logger = null ;

            // Optional: if you also used the static Log.Logger elsewhere
            Log.CloseAndFlush ( ) ;
        }
    }

    public static string CreateFullPathLogFileName ( string folder ,
                                                     string fileName )
    {
        var fullPath = Path.Combine ( folder ,
                                      fileName ) ;

        return fullPath ;
    }
}