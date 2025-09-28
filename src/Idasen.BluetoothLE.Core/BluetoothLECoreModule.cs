using Device = Idasen.BluetoothLE.Core.DevicesDiscovery.Device ;
using DeviceFactory = Idasen.BluetoothLE.Core.DevicesDiscovery.DeviceFactory ;
using IDevice = Idasen.BluetoothLE.Core.Interfaces.DevicesDiscovery.IDevice ;
using IDeviceFactory = Idasen.BluetoothLE.Core.Interfaces.DevicesDiscovery.IDeviceFactory ;

namespace Idasen.BluetoothLE.Core ;

using System.Diagnostics.CodeAnalysis ;
using System.Reactive.Concurrency ;
using System.Reactive.Subjects ;
using Aop ;
using Autofac ;
using Autofac.Extras.DynamicProxy ;
using DevicesDiscovery ;
using Interfaces ;
using Interfaces.DevicesDiscovery ;
using Interfaces.ServicesDiscovery ;
using Interfaces.ServicesDiscovery.Wrappers ;
using ServicesDiscovery ;
using ServicesDiscovery.Wrappers ;
using Device = DevicesDiscovery.Device ;
using DeviceFactory = DevicesDiscovery.DeviceFactory ;
using IDevice = Interfaces.DevicesDiscovery.IDevice ;
using IDeviceFactory = Interfaces.DevicesDiscovery.IDeviceFactory ;

// ReSharper disable once InconsistentNaming
/// <summary>
///     Autofac module wiring up core Bluetooth LE services, discovery wrappers, and AOP.
/// </summary>
[ ExcludeFromCodeCoverage ]
public sealed class BluetoothLECoreModule
    : Module
{
    /// <summary>
    ///     Registers all services and factories into the Autofac container.
    /// </summary>
    /// <param name="builder">The container builder.</param>
    protected override void Load ( ContainerBuilder builder )
    {
        Guard.ArgumentNotNull ( builder ,
                                nameof ( builder ) ) ;

        builder.RegisterModule < BluetoothLEAop > ( ) ;

        builder.RegisterGeneric ( typeof ( Subject <> ) )
               .As ( typeof ( ISubject <> ) ) ;

        builder.RegisterInstance ( TaskPoolScheduler.Default )
               .As < IScheduler > ( ) ;

        builder.RegisterType < ObservableTimerFactory > ( )
               .As < IObservableTimerFactory > ( )
               .EnableInterfaceInterceptors ( ) ;

        builder.RegisterType < DateTimeOffsetWrapper > ( )
               .As < IDateTimeOffset > ( ) ;

        builder.RegisterType < DeviceComparer > ( )
               .As < IDeviceComparer > ( )
               .EnableInterfaceInterceptors ( ) ;

        builder.RegisterType < Device > ( )
               .As < IDevice > ( )
               .EnableInterfaceInterceptors ( ) ;

        builder.RegisterType < DeviceMonitor > ( )
               .As < IDeviceMonitor > ( )
               .EnableInterfaceInterceptors ( ) ;

        builder.RegisterType < DeviceMonitorWithExpiry > ( )
               .As < IDeviceMonitorWithExpiry > ( )
               .EnableInterfaceInterceptors ( ) ;

        builder.RegisterType < Devices > ( )
               .As < IDevices > ( )
               .EnableInterfaceInterceptors ( ) ;

        builder.RegisterType < Watcher > ( )
               .As < IWatcher > ( )
               .EnableInterfaceInterceptors ( ) ;

        builder.RegisterType < Wrapper > ( )
               .As < IWrapper > ( )
               .EnableInterfaceInterceptors ( ) ;

        builder.RegisterType < DeviceFactory > ( )
               .As < IDeviceFactory > ( )
               .EnableInterfaceInterceptors ( ) ;

        builder.RegisterType < BluetoothLEDeviceProvider > ( )
               .As < IBluetoothLEDeviceProvider > ( )
               .EnableInterfaceInterceptors ( ) ;

        builder.RegisterType < StatusMapper > ( )
               .As < IStatusMapper > ( )
               .EnableInterfaceInterceptors ( ) ;

        builder.RegisterType < GattCharacteristicValueChangedObservables > ( )
               .As < IGattCharacteristicValueChangedObservables > ( )
               .EnableInterfaceInterceptors ( ) ;

        builder.RegisterType < GattCharacteristicWrapper > ( )
               .As < IGattCharacteristicWrapper > ( )
               .EnableInterfaceInterceptors ( ) ;

        builder.RegisterType < GattCharacteristicWrapperFactory > ( )
               .As < IGattCharacteristicWrapperFactory > ( )
               .EnableInterfaceInterceptors ( ) ;

        builder.RegisterType < MatchMaker > ( )
               .As < IMatchMaker > ( )
               .EnableInterfaceInterceptors ( ) ;

        builder.RegisterType < OfficialGattServices > ( )
               .As < IOfficialGattServices > ( )
               .EnableInterfaceInterceptors ( ) ;

        builder.RegisterType < GattServicesDictionary > ( )
               .As < IGattServicesDictionary > ( )
               .EnableInterfaceInterceptors ( ) ;

        builder.RegisterType < GattServicesProvider > ( )
               .As < IGattServicesProvider > ( )
               .EnableInterfaceInterceptors ( ) ;

        builder.RegisterType < GattServicesProviderFactory > ( )
               .As < IGattServicesProviderFactory > ( )
               .EnableInterfaceInterceptors ( ) ;

        builder.RegisterType < ServicesDiscovery.Device > ( )
               .As < Interfaces.ServicesDiscovery.IDevice > ( )
               .EnableInterfaceInterceptors ( ) ;

        builder.RegisterType < ServicesDiscovery.DeviceFactory > ( )
               .As < Interfaces.ServicesDiscovery.IDeviceFactory > ( )
               .EnableInterfaceInterceptors ( ) ;

        builder.RegisterType < BluetoothLeDeviceWrapper > ( )
               .As < IBluetoothLeDeviceWrapper > ( )
               .EnableInterfaceInterceptors ( ) ;

        builder.RegisterType < BluetoothLeDeviceWrapperFactory > ( )
               .As < IBluetoothLeDeviceWrapperFactory > ( )
               .EnableInterfaceInterceptors ( ) ;

        builder.RegisterType < GattDeviceServiceWrapper > ( )
               .As < IGattDeviceServiceWrapper > ( )
               .EnableInterfaceInterceptors ( ) ;

        builder.RegisterType < GattDeviceServicesResultWrapper > ( )
               .As < IGattDeviceServicesResultWrapper > ( ) ;

        builder.RegisterType < GattDeviceServicesResultWrapperFactory > ( )
               .As < IGattDeviceServicesResultWrapperFactory > ( )
               .EnableInterfaceInterceptors ( ) ;

        builder.RegisterType < GattCharacteristicsResultWrapper > ( )
               .As < IGattCharacteristicsResultWrapper > ( )
               .EnableInterfaceInterceptors ( ) ;

        builder.RegisterType < GattCharacteristicsResultWrapperFactory > ( )
               .As < IGattCharacteristicsResultWrapperFactory > ( )
               .EnableInterfaceInterceptors ( ) ;

        builder.RegisterType < GattReadResultWrapper > ( )
               .As < IGattReadResultWrapper > ( ) ;

        builder.RegisterType < GattReadResultWrapperFactory > ( )
               .As < IGattReadResultWrapperFactory > ( )
               .EnableInterfaceInterceptors ( ) ;

        builder.RegisterType < GattWriteResultWrapper > ( )
               .As < IGattWriteResultWrapper > ( ) ;

        builder.RegisterType < GattWriteResultWrapperFactory > ( )
               .As < IGattWriteResultWrapperFactory > ( )
               .EnableInterfaceInterceptors ( ) ;
    }
}
