using Autofac ;
using Autofac.Core ;
using AutofacSerilogIntegration ;
using Idasen.BluetoothLE.Core ;
using Idasen.BluetoothLE.Linak ;
using Microsoft.Extensions.Configuration ;
using Serilog ;
using Serilog.Configuration ;

namespace Idasen.Launcher
{
    public static class ContainerProvider
    {
        public static ContainerBuilder Builder { get ; } = new( ) ;

        public static IContainer Create ( string                    appName ,
                                          string                    appLogFileName ,
                                          IEnumerable < IModule > ? otherModules = null )
        {
            Log.Logger = LoggerProvider.CreateLogger ( appName ,
                                                       appLogFileName ) ;

            return Register ( otherModules ) ;
        }

        public static IContainer Create ( ILoggerSettings           settings ,
                                          IEnumerable < IModule > ? otherModules = null )
        {
            Log.Logger = new LoggerConfiguration ( ).ReadFrom
                                                    .Settings ( settings )
                                                    .Enrich.WithCaller()
                                                    .CreateLogger ( ) ;

            return Register ( otherModules ) ;
        }

        public static IContainer Create ( IConfiguration            configuration ,
                                          IEnumerable < IModule > ? otherModules = null )
        {
            var loggerConfiguration = new LoggerConfiguration ( ).ReadFrom
                                                                 .Configuration ( configuration )
                                                                 .Enrich.WithCaller ( ) ;

            Log.Logger = Log.Logger = loggerConfiguration.CreateLogger ( ) ;

            return Register ( otherModules ) ;
        }

        private static IContainer Register ( IEnumerable < IModule > ? otherModules )
        {
            Builder.RegisterLogger ( ) ;
            Builder.RegisterModule < BluetoothLECoreModule > ( ) ;
            Builder.RegisterModule < BluetoothLELinakModule > ( ) ;

            if ( otherModules == null )
                return Builder.Build ( ) ;

            foreach ( var otherModule in otherModules )
            {
                Builder.RegisterModule ( otherModule ) ;
            }

            return Builder.Build ( ) ;
        }
    }
}