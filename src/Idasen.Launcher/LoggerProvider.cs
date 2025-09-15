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

    private static Lazy < Logger >? _logger ;

    public static ILogger CreateLogger ( string appName ,
                                         string appLogFileName )
    {
        Guard.ArgumentNotNull ( appName ,
                                nameof ( appName ) ) ;
        Guard.ArgumentNotNull ( appLogFileName ,
                                nameof ( appLogFileName ) ) ;

        if ( _logger != null )
        {
            _logger.Value.Debug ( "Using existing logger for '{AppName}' in folder {LogFile}" , appName , appLogFileName ) ;

            return _logger.Value ;
        }

        _logger = DoCreateLogger ( appLogFileName ) ;

        _logger.Value.Debug ( "Created logger for '{AppName}' in folder '{LogFile}'" , appName , appLogFileName ) ;

        return _logger.Value ;
    }

    private static Lazy < Logger > DoCreateLogger ( string appLogFileName )
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
                                                 hooks : new LoggingFileHooks ( ) ) ;
#pragma warning restore CA1305

        var logger = loggerConfiguration.CreateLogger ( ) ;

        Console.WriteLine ( "Log file name: {0} {1}" , LoggingFile.FullPath , LoggingFile.Path ) ;

        return new Lazy < Logger > ( logger ) ;
    }

    public static string CreateFullPathLogFileName ( string folder ,
                                                     string fileName )
    {
        var fullPath = Path.Combine ( folder ,
                                      fileName ) ;

        return fullPath ;
    }
}