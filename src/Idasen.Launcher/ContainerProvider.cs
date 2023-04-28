using System.Collections.Generic ;
using Autofac ;
using Autofac.Core ;
using AutofacSerilogIntegration ;
using Idasen.BluetoothLE.Core ;
using Idasen.BluetoothLE.Linak ;
using Serilog ;
using Serilog.Configuration ;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace Idasen.Launcher
{
    public static class ContainerProvider
    {
        public static ContainerBuilder Builder { get ; } = new( ) ;

        public static IContainer Create (
            string                    appName ,
            string                    appLogFileName ,
            IEnumerable < IModule > ? otherModules = null )
        {
            Log.Logger = LoggerProvider.CreateLogger ( appName ,
                                                       appLogFileName ) ;

            return Register ( otherModules ) ;
        }

        public static IContainer Create ( ILoggerSettings        settings,
                                          IEnumerable<IModule> ? otherModules = null)
        {
            Log.Logger = new LoggerConfiguration ( ).ReadFrom
                                                    .Settings ( settings )
                                                    .CreateLogger ( ) ;

            return Register(otherModules);
        }

        public static IContainer Create(IConfiguration         configuration,
                                        IEnumerable<IModule> ? otherModules = null)
        {
            Log.Logger = Log.Logger = new LoggerConfiguration ( ).ReadFrom
                                                                 .Configuration ( configuration )
                                                                 .CreateLogger ( ) ;

            return Register(otherModules);
        }

        private static IContainer Register(IEnumerable<IModule> ? otherModules)
        {
            Builder.RegisterLogger();
            Builder.RegisterModule<BluetoothLECoreModule>();
            Builder.RegisterModule<BluetoothLELinakModule>();

            if (otherModules == null)
                return Builder.Build();

            foreach (var otherModule in otherModules)
            {
                Builder.RegisterModule(otherModule);
            }

            return Builder.Build();
        }
    }
}