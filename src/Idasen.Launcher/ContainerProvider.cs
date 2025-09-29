using System.Diagnostics.CodeAnalysis ;
using Autofac ;
using Autofac.Core ;
using AutofacSerilogIntegration ;
using Idasen.BluetoothLE.Core ;
using Idasen.BluetoothLE.Linak ;
using Microsoft.Extensions.Configuration ;
using Serilog ;
using Serilog.Configuration ;

namespace Idasen.Launcher ;

/// <summary>
///     Provides factory methods to create and configure an Autofac <see cref="IContainer" />, wiring up Serilog and the
///     BluetoothLE modules. Overloads allow configuring Serilog from app name/log file, <see cref="ILoggerSettings" />, or
///     <see cref="IConfiguration" />.
/// </summary>
[ ExcludeFromCodeCoverage ]
public static class ContainerProvider
{
    /// <summary>
    ///     Creates an Autofac container after configuring Serilog using the provided application name and log file name via
    ///     <see cref="LoggerProvider" />.
    /// </summary>
    /// <param name="appName">Logical application name used for logger diagnostics.</param>
    /// <param name="appLogFileName">Log file name (e.g., <c>app.log</c>).</param>
    /// <param name="otherModules">Optional additional Autofac modules to register.</param>
    /// <returns>A built <see cref="IContainer" /> ready for use.</returns>
    public static IContainer Create ( string                    appName ,
                                      string                    appLogFileName ,
                                      IEnumerable < IModule > ? otherModules = null )
    {
        Guard.ArgumentNotEmptyOrWhitespace ( appName ,
                                             nameof ( appName ) ) ;
        Guard.ArgumentNotEmptyOrWhitespace ( appLogFileName ,
                                             nameof ( appLogFileName ) ) ;

        Log.Logger = LoggerProvider.CreateLogger ( appName ,
                                                   appLogFileName ) ;

        return Register ( otherModules ) ;
    }

    /// <summary>
    ///     Creates an Autofac container after configuring Serilog using the provided <see cref="ILoggerSettings" />.
    /// </summary>
    /// <param name="settings">Serilog settings source.</param>
    /// <param name="otherModules">Optional additional Autofac modules to register.</param>
    /// <returns>A built <see cref="IContainer" /> ready for use.</returns>
    public static IContainer Create ( ILoggerSettings           settings ,
                                      IEnumerable < IModule > ? otherModules = null )
    {
        ArgumentNullException.ThrowIfNull ( settings ) ;

        Log.Logger = new LoggerConfiguration ( ).ReadFrom
                                                .Settings ( settings )
                                                .Enrich.WithCaller ( )
                                                .CreateLogger ( ) ;

        return Register ( otherModules ) ;
    }

    /// <summary>
    ///     Creates an Autofac container after configuring Serilog using the provided Microsoft <see cref="IConfiguration" />.
    ///     The configuration root is also registered so modules can bind their own settings objects.
    /// </summary>
    /// <param name="configuration">Configuration root containing Serilog settings.</param>
    /// <param name="otherModules">Optional additional Autofac modules to register.</param>
    /// <returns>A built <see cref="IContainer" /> ready for use.</returns>
    public static IContainer Create ( IConfiguration            configuration ,
                                      IEnumerable < IModule > ? otherModules = null )
    {
        ArgumentNullException.ThrowIfNull ( configuration ) ;

        var loggerConfiguration = new LoggerConfiguration ( ).ReadFrom
                                                             .Configuration ( configuration )
                                                             .Enrich.WithCaller ( ) ;

        Log.Logger = loggerConfiguration.CreateLogger ( ) ;

        return Register ( otherModules ,
                          configuration ) ;
    }

    private static IContainer Register ( IEnumerable < IModule > ? otherModules ,
                                         IConfiguration ?          configuration = null )
    {
        var builder = new ContainerBuilder ( ) ;

        builder.RegisterLogger ( ) ;

        if ( configuration != null )
            builder.RegisterInstance ( configuration )
                   .As < IConfiguration > ( )
                   .SingleInstance ( ) ;

        builder.RegisterModule < BluetoothLECoreModule > ( ) ;
        builder.RegisterModule < BluetoothLELinakModule > ( ) ;

        if ( otherModules != null )
            foreach ( var otherModule in otherModules )
                builder.RegisterModule ( otherModule ) ;

        return builder.Build ( ) ;
    }
}