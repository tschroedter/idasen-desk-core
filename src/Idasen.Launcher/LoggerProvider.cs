using System ;
using System.IO ;
using Idasen.BluetoothLE.Core ;
using JetBrains.Annotations ;
using Serilog ;
using Serilog.Events ;

namespace Idasen.Launcher
{
    public static class LoggerProvider
    {
        private const string LogTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss.ffff} " +
                                           "{Level:u3}] {Message} "                 +
                                           "(at {Caller}){NewLine}{Exception}" ;

        private static ILogger Logger ;

        public static ILogger CreateLogger (
            [ NotNull ] string appName ,
            [ NotNull ] string appLogFileName )
        {
            Guard.ArgumentNotNull ( appName ,
                                    nameof ( appName ) ) ;
            Guard.ArgumentNotNull ( appLogFileName ,
                                    nameof ( appLogFileName ) ) ;

            if ( Logger != null )
            {
                Logger.Debug ( $"Using existing logger for '{appName}' in folder {appLogFileName}" ) ;

                return Logger ;
            }

            Logger = DoCreateLogger ( appLogFileName ) ;

            Logger.Debug ( $"Created logger for '{appName}' in folder '{appLogFileName}'" ) ;

            return Logger ;
        }

        private static ILogger DoCreateLogger ( string appLogFileName )
        {
            var logFolder = AppDomain.CurrentDomain.BaseDirectory + "\\logs\\" ;

            var logFile = CreateFullPathLogFileName ( logFolder,
                                                      appLogFileName ) ;

            if ( ! Directory.Exists ( logFolder ) )
                Directory.CreateDirectory ( logFolder ) ;

            var loggerConfiguration = new LoggerConfiguration ( )
                                     .MinimumLevel
                                     .Debug ( )
                                     .Enrich
                                     .WithCaller ( )
                                     .WriteTo.Console ( LogEventLevel.Debug ,
                                                        LogTemplate )
                                     .WriteTo.File ( logFile ,
                                                     LogEventLevel.Debug ,
                                                     LogTemplate ) ;

            return loggerConfiguration.CreateLogger ( ) ;
        }

        public static string CreateFullPathLogFileName ( string folder ,
                                                         string fileName )
        {
            var fullPath = Path.Combine ( folder ,
                                          fileName ) ;
            return fullPath ;
        }
    }
}