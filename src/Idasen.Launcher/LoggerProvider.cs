using Idasen.BluetoothLE.Core ;
using Serilog ;
using Serilog.Events ;
using Serilog.Sinks.File;
using System.Text;

namespace Idasen.Launcher;

public static class LoggerProvider
{
    public static ILogger CreateLogger ( string appName ,
                                         string appLogFileName )
    {
        Guard.ArgumentNotNull ( appName ,
                                nameof ( appName ) ) ;
        Guard.ArgumentNotNull ( appLogFileName ,
                                nameof ( appLogFileName ) ) ;

        if ( _logger != null )
        {
            _logger.Debug ( $"Using existing logger for '{appName}' in folder {appLogFileName}" ) ;

            return _logger ;
        }

        _logger = DoCreateLogger ( appLogFileName ) ;

        _logger.Debug ( $"Created logger for '{appName}' in folder '{appLogFileName}'" ) ;

        return _logger ;
    }

    private static Serilog.Core.Logger DoCreateLogger ( string appLogFileName )
    {
        var logFolder = AppDomain.CurrentDomain.BaseDirectory + "\\logs\\" ;
        var logFile = CreateFullPathLogFileName ( logFolder ,
                                                  appLogFileName ) ;

        if ( ! Directory.Exists ( logFolder ) )
            Directory.CreateDirectory ( logFolder ) ;

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
                                                 hooks : new LoggingFileHooks() ) ;
#pragma warning restore CA1305

        var logger = loggerConfiguration.CreateLogger ( ) ;

        Console.WriteLine($"Log file name: {LoggingFile.FullPath} {LoggingFile.Path}" ) ;

        return logger ;
    }

    public static string CreateFullPathLogFileName ( string folder ,
                                                     string fileName )
    {
        var fullPath = Path.Combine ( folder ,
                                      fileName ) ;

        return fullPath ;
    }

    private const string LogTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss.ffff} " +
                                       "{Level:u3}] {Message} " +
                                       "(at {Caller}){NewLine}{Exception}" ;

    private static Serilog.Core.Logger? _logger ;
}