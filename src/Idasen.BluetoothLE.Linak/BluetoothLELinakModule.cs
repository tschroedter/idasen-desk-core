using System.Diagnostics.CodeAnalysis ;
using System.Reactive.Concurrency ;
using Autofac ;
using Autofac.Extras.DynamicProxy ;
using Idasen.Aop ;
using Idasen.BluetoothLE.Characteristics ;
using Idasen.BluetoothLE.Linak.Control ;
using Idasen.BluetoothLE.Linak.Interfaces ;
using Microsoft.Extensions.Configuration ;
using Serilog ;

namespace Idasen.BluetoothLE.Linak ;

/// <summary>
///     Autofac module that wires up LINAK desk services, control components, and characteristics.
/// </summary>
[ ExcludeFromCodeCoverage ]
// ReSharper disable once InconsistentNaming
public class BluetoothLELinakModule
    : Module
{
    /// <summary>
    ///     Registers LINAK services into the Autofac container.
    /// </summary>
    /// <param name="builder">The container builder.</param>
    protected override void Load ( ContainerBuilder builder )
    {
        builder.RegisterModule < BluetoothLEAop > ( ) ;
        builder.RegisterModule < BluetoothLEDeskCharacteristics > ( ) ;

        builder.RegisterInstance ( TaskPoolScheduler.Default )
               .As < IScheduler > ( ) ;

        builder.RegisterType < DeskCharacteristics > ( )
               .As < IDeskCharacteristics > ( )
               .EnableInterfaceInterceptors ( ) ;

        builder.RegisterType < Desk > ( )
               .As < IDesk > ( )
               .EnableInterfaceInterceptors ( ) ;

        builder.RegisterType < DeskFactory > ( )
               .As < IDeskFactory > ( )
               .EnableInterfaceInterceptors ( ) ;

        builder.RegisterType < DeskHeightAndSpeed > ( )
               .As < IDeskHeightAndSpeed > ( )
               .EnableInterfaceInterceptors ( ) ;

        builder.RegisterType < DeskHeightAndSpeedFactory > ( )
               .As < IDeskHeightAndSpeedFactory > ( )
               .EnableInterfaceInterceptors ( ) ;

        builder.RegisterType < RawValueToHeightAndSpeedConverter > ( )
               .As < IRawValueToHeightAndSpeedConverter > ( )
               .EnableInterfaceInterceptors ( ) ;

        builder.RegisterType < DeskCommandExecutor > ( )
               .As < IDeskCommandExecutor > ( )
               .EnableInterfaceInterceptors ( ) ;

        builder.RegisterType < DeskCommandExecutorFactory > ( )
               .As < IDeskCommandExecutorFactory > ( )
               .EnableInterfaceInterceptors ( ) ;

        builder.RegisterType < DeskCommandsProvider > ( )
               .As < IDeskCommandsProvider > ( )
               .EnableInterfaceInterceptors ( ) ;

        // Manager
        builder.RegisterType < DeskMover > ( )
               .As < IDeskMover > ( )
               .EnableInterfaceInterceptors ( ) ;

        builder.RegisterType < DeskMoverFactory > ( )
               .As < IDeskMoverFactory > ( )
               .EnableInterfaceInterceptors ( ) ;

        // New collaborators
        builder.RegisterType < DeskMoveEngine > ( )
               .As < IDeskMoveEngine > ( )
               .EnableInterfaceInterceptors ( ) ;

        builder.Register ( ctx =>
                           {
                               var logger        = ctx.Resolve < ILogger > ( ) ;
                               var settings      = ctx.Resolve < DeskMoverSettings > ( ) ;
                               var heightMonitor = ctx.Resolve < IDeskHeightMonitor > ( ) ;
                               var calculator    = ctx.Resolve < IStoppingHeightCalculator > ( ) ;
                               return new DeskStopper ( logger ,
                                                        settings ,
                                                        heightMonitor ,
                                                        calculator ) ;
                           } )
               .As < IDeskStopper > ( )
               .EnableInterfaceInterceptors ( ) ;

        builder.RegisterType < DeskMovementMonitor > ( )
               .As < IDeskMovementMonitor > ( )
               .EnableInterfaceInterceptors ( ) ;

        builder.RegisterType < DeskMovementMonitorFactory > ( )
               .As < IDeskMovementMonitorFactory > ( )
               .EnableInterfaceInterceptors ( ) ;

        builder.RegisterType < StoppingHeightCalculator > ( )
               .As < IStoppingHeightCalculator > ( )
               .EnableInterfaceInterceptors ( ) ;

        builder.RegisterType < HasReachedTargetHeightCalculator > ( )
               .As < IHasReachedTargetHeightCalculator > ( )
               .EnableInterfaceInterceptors ( ) ;

        builder.RegisterType < InitialHeightProvider > ( )
               .As < IInitialHeightProvider > ( )
               .EnableInterfaceInterceptors ( ) ;

        builder.RegisterType < InitialHeightAndSpeedProviderFactory > ( )
               .As < IInitialHeightAndSpeedProviderFactory > ( )
               .EnableInterfaceInterceptors ( ) ;

        builder.RegisterType < DeskCharacteristicsCreator > ( )
               .As < IDeskCharacteristicsCreator > ( )
               .EnableInterfaceInterceptors ( ) ;

        builder.RegisterType < DeskConnector > ( )
               .As < IDeskConnector > ( )
               .EnableInterfaceInterceptors ( ) ;

        builder.RegisterType < DeskDetector > ( )
               .As < IDeskDetector > ( )
               .EnableInterfaceInterceptors ( ) ;

        builder.RegisterType < DeskProvider > ( )
               .As < IDeskProvider > ( )
               .EnableInterfaceInterceptors ( ) ;

        builder.RegisterType < DeskHeightMonitor > ( )
               .As < IDeskHeightMonitor > ( )
               .EnableInterfaceInterceptors ( ) ;

        builder.RegisterType < TaskRunner > ( )
               .As < ITaskRunner > ( ) ;

        builder.RegisterType < DeskLocker > ( )
               .As < IDeskLocker > ( )
               .EnableInterfaceInterceptors ( ) ;

        builder.RegisterType < DeskLockerFactory > ( )
               .As < IDeskLockerFactory > ( )
               .EnableInterfaceInterceptors ( ) ;

        builder.RegisterType < ErrorManager > ( )
               .As < IErrorManager > ( )
               .SingleInstance ( )
               .EnableInterfaceInterceptors ( ) ;

        // Bind DeskMoverSettings from configuration (if available), fallback to defaults
        builder.Register ( ctx =>
                           {
                               if ( ! ctx.TryResolve ( out IConfiguration ? config ) )
                                   return DeskMoverSettings.Default ;

                               var section = config.GetSection ( "DeskMoverSettings" ) ;

                               if ( ! section.Exists ( ) )
                                   return DeskMoverSettings.Default ;

                               var defaults = DeskMoverSettings.Default ;

                               if ( ! TimeSpan.TryParse ( section [ nameof ( DeskMoverSettings.TimerInterval ) ] ,
                                                          out var timerInterval ) )
                                   timerInterval =
                                       long.TryParse ( section [ nameof ( DeskMoverSettings.TimerInterval ) ] ,
                                                       out var ms )
                                           ? TimeSpan.FromMilliseconds ( ms )
                                           : defaults.TimerInterval ;

                               if ( ! TimeSpan.TryParse ( section [ nameof ( DeskMoverSettings.KeepAliveInterval ) ] ,
                                                          out var keepAliveInterval ) )
                                   keepAliveInterval =
                                       long.TryParse ( section [ nameof ( DeskMoverSettings.KeepAliveInterval ) ] ,
                                                       out var kam )
                                           ? TimeSpan.FromMilliseconds ( kam )
                                           : defaults.KeepAliveInterval ;

                               uint ReadUInt ( string key , uint fallback )
                               {
                                   return uint.TryParse ( section [ key ] ,
                                                          out var v )
                                              ? v
                                              : fallback ;
                               }

                               return new DeskMoverSettings
                                      {
                                          TimerInterval = timerInterval ,
                                          NearTargetBaseTolerance =
                                              ReadUInt ( nameof ( DeskMoverSettings.NearTargetBaseTolerance ) ,
                                                         defaults.NearTargetBaseTolerance ) ,
                                          NearTargetMaxDynamicTolerance =
                                              ReadUInt ( nameof ( DeskMoverSettings.NearTargetMaxDynamicTolerance ) ,
                                                         defaults.NearTargetMaxDynamicTolerance ) ,
                                          OvershootCompensation =
                                              ReadUInt ( nameof ( DeskMoverSettings.OvershootCompensation ) ,
                                                         defaults.OvershootCompensation ) ,
                                          KeepAliveInterval = keepAliveInterval
                                      } ;
                           } )
               .AsSelf ( )
               .SingleInstance ( ) ;
    }
}