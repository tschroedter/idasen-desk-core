using System ;
using Idasen.Launcher ;
using Serilog ;
using Serilog.Events ;
using Serilog.Sinks.SystemConsole.Themes ;
using Topshelf ;

namespace Idasen.RESTAPI
{
    internal class Program
    {
        private const string LogTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss.ffff} " +
                                           "{Level:u3}] {Message} "                 +
                                           "(at {Caller}){NewLine}{Exception}" ;

        private static void Main ( )
        {
            Log.Logger = CreateLoggerConfiguration ( ).CreateLogger ( ) ;


            var rc = HostFactory.Run ( x =>
                                       {
                                           x.Service < IdasenRestApi > ( s =>
                                                                         {
                                                                             s.ConstructUsing ( name =>
                                                                                                    new
                                                                                                        IdasenRestApi ( ) ) ;
                                                                             s.WhenStarted ( tc => tc.Start ( ) ) ;
                                                                             s.WhenStopped ( tc => tc.Stop ( ) ) ;
                                                                         } ) ;
                                           x.RunAsLocalSystem ( ) ;

                                           x.SetDescription ( "Idasen REST API Host" ) ;
                                           x.SetDisplayName ( "Idasen REST API" ) ;
                                           x.SetServiceName ( "Idasen REST API" ) ;
                                           x.StartAutomatically ( ) ;
                                           x.UseSerilog ( ) ;
                                           //x.RunAs("username", "password");
                                       } ) ;

            var exitCode = ( int )Convert.ChangeType ( rc ,
                                                       rc.GetTypeCode ( ) ) ;

            Environment.ExitCode = exitCode ;
        }

        private static LoggerConfiguration CreateLoggerConfiguration ( )
        {
            var logFile = AppDomain.CurrentDomain.BaseDirectory
                        + "logs\\idasen-desk-rest-api.log" ;

            Console.WriteLine ( $"Logging to '{logFile}'" ) ;

            return new LoggerConfiguration ( ).MinimumLevel
                                              .Debug ( )
                                              .Enrich
                                              .WithCaller ( )
                                              .WriteTo.Console ( LogEventLevel.Debug ,
                                                                 LogTemplate ,
                                                                 theme : AnsiConsoleTheme.Code )
                                              .WriteTo
                                              .File ( logFile ,
                                                      LogEventLevel.Debug ,
                                                      LogTemplate ) ;
        }
    }
}