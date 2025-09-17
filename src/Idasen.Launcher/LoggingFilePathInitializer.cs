using System.Diagnostics.CodeAnalysis ;
using System.Reflection ;
using Microsoft.Extensions.Configuration ;
using Serilog.Configuration ;

namespace Idasen.Launcher ;

[ ExcludeFromCodeCoverage ]
public static class LoggingFilePathInitializer
{
    /// <summary>
    ///     Optionally set the log file path eagerly from configuration.
    /// </summary>
    public static void TrySetFromConfiguration ( IConfiguration? configuration )
    {
        if ( configuration == null )
        {
            return ;
        }

        var writeTo = configuration.GetSection ( "Serilog:WriteTo" ) ;

        foreach (var sink in writeTo.GetChildren ( ))
        {
            var name = sink["Name"] ;

            if ( ! string.Equals ( name ,
                                   "File" ,
                                   StringComparison.OrdinalIgnoreCase ) )
            {
                continue ;
            }

            var path = sink.GetSection ( "Args" )["path"] ;

            if ( string.IsNullOrWhiteSpace ( path ) )
            {
                continue ;
            }

            var fullPath = Path.IsPathRooted ( path )
                               ? path
                               : Path.Combine ( AppDomain.CurrentDomain.BaseDirectory ,
                                                path ) ;

            LoggingFile.FullPath = fullPath ;

            break ;
        }
    }

    /// <summary>
    ///     Attempts to extract IConfiguration from ILoggerSettings (for Serilog.Settings.Configuration-backed settings)
    ///     and set the log file path eagerly.
    /// </summary>
    public static void TrySetFromSettings ( ILoggerSettings? settings )
    {
        if ( settings == null )
        {
            return ;
        }

        try
        {
            var type = settings.GetType ( ) ;

            var prop = type.GetProperty ( "Configuration" ,
                                          BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic ) ??
                       type.GetProperty ( "ConfigurationRoot" ,
                                          BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic ) ??
                       type.GetProperty ( "Config" ,
                                          BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic ) ;

            if ( prop?.GetValue ( settings ) is IConfiguration configuration )
            {
                TrySetFromConfiguration ( configuration ) ;
            }
        }
        catch
        {
            // Best-effort only; ignore if we can't infer the configuration
        }
    }
}