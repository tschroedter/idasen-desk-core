using Autofac ;
using Autofac.Core ;
using AutofacSerilogIntegration ;
using Idasen.BluetoothLE.Core ;
using Idasen.BluetoothLE.Linak ;
using Microsoft.Extensions.Configuration ;
using Serilog ;
using Serilog.Configuration ;

namespace Idasen.Launcher ;

public static class ContainerProvider
{
    public static IContainer Create ( string appName ,
                                      string appLogFileName ,
                                      IEnumerable < IModule >? otherModules = null )
    {
        Log.Logger = LoggerProvider.CreateLogger ( appName ,
                                                   appLogFileName ) ;

        return Register ( otherModules ) ;
    }

    public static IContainer Create ( ILoggerSettings settings ,
                                      IEnumerable < IModule >? otherModules = null )
    {
        Log.Logger = new LoggerConfiguration ( ).ReadFrom
                                                .Settings ( settings )
                                                .Enrich.WithCaller ( )
                                                .CreateLogger ( ) ;

        return Register ( otherModules ) ;
    }

    public static IContainer Create ( IConfiguration configuration ,
                                      IEnumerable < IModule >? otherModules = null )
    {
        var loggerConfiguration = new LoggerConfiguration ( ).ReadFrom
                                                             .Configuration ( configuration )
                                                             .Enrich.WithCaller ( ) ;

        Log.Logger = loggerConfiguration.CreateLogger ( ) ;

        return Register ( otherModules ) ;
    }

    private static IContainer Register ( IEnumerable < IModule >? otherModules )
    {
        var builder = new ContainerBuilder ( ) ;

        builder.RegisterLogger ( ) ;
        builder.RegisterModule < BluetoothLECoreModule > ( ) ;
        builder.RegisterModule < BluetoothLELinakModule > ( ) ;

        if ( otherModules != null )
        {
            foreach (var otherModule in otherModules)
            {
                builder.RegisterModule ( otherModule ) ;
            }
        }

        return builder.Build ( ) ;
    }
}