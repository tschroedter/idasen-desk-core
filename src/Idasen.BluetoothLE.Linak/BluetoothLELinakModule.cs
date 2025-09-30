using System.Diagnostics.CodeAnalysis ;
using System.Reactive.Concurrency ;
using Autofac ;
using Autofac.Extras.DynamicProxy ;
using Idasen.Aop ;
using Idasen.BluetoothLE.Characteristics ;
using Idasen.BluetoothLE.Linak.Control ;
using Idasen.BluetoothLE.Linak.Interfaces ;

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

        builder.RegisterType < DeskMover > ( )
               .As < IDeskMover > ( )
               .EnableInterfaceInterceptors ( ) ;

        builder.RegisterType < DeskMoverFactory > ( )
               .As < IDeskMoverFactory > ( )
               .EnableInterfaceInterceptors ( ) ;

        builder.RegisterType < DeskMoveEngine > ( )
               .As < IDeskMoveEngine > ( )
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

        builder.RegisterType < DeskMoveGuard > ( )
               .As < IDeskMoveGuard > ( ) ;
    }
}
