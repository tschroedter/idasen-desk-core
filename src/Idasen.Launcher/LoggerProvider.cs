using System.Reflection ;
using Idasen.BluetoothLE.Core ;
using JetBrains.Annotations ;
using Microsoft.Extensions.Configuration ;
using Serilog ;
using Serilog.Core ;
using Serilog.Debugging ;
using Serilog.Events ;

namespace Idasen.Launcher ;

/// <summary>
///     Provides factory methods for creating and managing Serilog loggers for the application.
///     Supports a thread-safe singleton lifecycle when using the overload that accepts application metadata.
/// </summary>
public static class LoggerProvider
{
    private const string LogTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss.ffff} " +
                                       "{Level:u3}] {Message} " +
                                       "(at {Caller}){NewLine}{Exception}" ;

    private static readonly object Sync = new ( ) ;
    private static Logger? _logger ;
    private static bool _selfLogEnabled ;

    /// <summary>
    ///     Creates a logger from Serilog configuration in the local <c>appsettings.json</c> next to the entry assembly.
    ///     Also enables Serilog self-log output to <c>serilog-selflog.txt</c>.
    /// </summary>
    /// <returns>An <see cref="ILogger" /> configured from application settings.</returns>
    [ UsedImplicitly ]
    public static ILogger CreateLogger ( string baseDir = "" )
    {
        EnsureSelfLogEnabled ( ) ;

        lock (Sync)
        {
            if ( _logger != null )
            {
                return _logger ;
            }

            if ( string.IsNullOrEmpty ( baseDir ) )
            {
                baseDir = GetBaseDirectoryName ( ) ;
            }

            var configuration = new ConfigurationBuilder ( )
                               .SetBasePath ( baseDir )
                               .AddJsonFile ( "appsettings.json" ,
                                              true ,
                                              true )
                               .Build ( ) ;

            _logger = new LoggerConfiguration ( )
                     .ReadFrom.Configuration ( configuration )
                     .CreateLogger ( ) ;

            // Keep Serilog's global reference in sync for third-party components
            Log.Logger = _logger ;

            return _logger ;
        }
    }

    /// <summary>
    ///     Creates or returns a singleton logger that writes to the console and a file in the <c>logs</c> folder.
    ///     If a logger already exists, it is reused.
    /// </summary>
    /// <param name="appName">Logical application name, used for diagnostic messages only.</param>
    /// <param name="appLogFileName">Log file name (e.g., <c>app.log</c>).</param>
    /// <returns>The singleton <see cref="ILogger" /> instance.</returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="appName" /> or <paramref name="appLogFileName" />
    ///     is <c>null</c>.
    /// </exception>
    public static ILogger CreateLogger ( string appName , string appLogFileName )
    {
        Guard.ArgumentNotEmptyOrWhitespace ( appName ,
                                             nameof ( appName ) ) ;
        Guard.ArgumentNotEmptyOrWhitespace ( appLogFileName ,
                                             nameof ( appLogFileName ) ) ;

        EnsureSelfLogEnabled ( ) ;

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

            // Keep Serilog's global reference in sync for third-party components
            Log.Logger = _logger ;

            _logger.Debug ( "Created logger for '{AppName}' in folder '{LogFile}'" ,
                            appName ,
                            appLogFileName ) ;
            return _logger ;
        }
    }

    /// <summary>
    ///     Builds a Serilog <see cref="Logger" /> that logs to the console and to the specified file under the <c>logs</c>
    ///     folder.
    /// </summary>
    /// <param name="appLogFileName">File name to use for the log file.</param>
    /// <returns>A configured <see cref="Logger" /> instance.</returns>
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

        // Make the path available immediately; hook will update on open/roll
        LoggingFile.FullPath = logFile ;

#pragma warning disable CA1305
        var loggerConfiguration = new LoggerConfiguration ( )
                                 .MinimumLevel.Debug ( )
                                 .Enrich.WithCaller ( )
                                 .WriteTo.Console ( LogEventLevel.Debug ,
                                                    LogTemplate )
                                 .WriteTo.File ( logFile ,
                                                 LogEventLevel.Debug ,
                                                 LogTemplate ,
                                                 hooks : new LoggingFileHooks ( ) ) ;
#pragma warning restore CA1305

        return loggerConfiguration.CreateLogger ( ) ;
    }

    /// <summary>
    ///     Disposes the singleton logger (if any) and flushes Serilog sinks. Safe to call multiple times.
    /// </summary>
    public static void Shutdown ( )
    {
        lock (Sync)
        {
            if ( _logger == null )
            {
                return ;
            }

            _logger.Dispose ( ) ;
            _logger = null ;

            // If Log.Logger has been used elsewhere, flush it as well
            Log.CloseAndFlush ( ) ;

            // Turn off Serilog self-log
            SelfLog.Disable ( ) ;
            _selfLogEnabled = false ;
        }
    }

    /// <summary>
    ///     Combines the provided folder and file name into a full path.
    /// </summary>
    /// <param name="folder">Target directory.</param>
    /// <param name="fileName">File name.</param>
    /// <returns>The combined full path.</returns>
    public static string CreateFullPathLogFileName ( string folder , string fileName )
    {
        ArgumentNullException.ThrowIfNull ( folder ) ;
        ArgumentNullException.ThrowIfNull ( fileName ) ;

        return Path.Combine ( folder ,
                              fileName ) ;
    }

    private static void EnsureSelfLogEnabled ( )
    {
        if ( _selfLogEnabled )
        {
            return ;
        }

        SelfLog.Enable ( msg =>
                         {
                             try
                             {
                                 File.AppendAllText ( Path.Combine ( AppContext.BaseDirectory ,
                                                                     "serilog-selflog.txt" ) ,
                                                      msg ) ;
                             }
                             catch
                             {
                                 /* ignore IO errors */
                             }
                         } ) ;
        _selfLogEnabled = true ;
    }

    private static string GetBaseDirectoryName ( )
    {
        return Path.GetDirectoryName ( Assembly.GetEntryAssembly ( )?.Location ?? AppContext.BaseDirectory ) ?? AppContext.BaseDirectory ;
    }
}