using System.Diagnostics.CodeAnalysis ;
using Autofac ;
using Autofac.Extras.DynamicProxy ;
using Idasen.BluetoothLE.Characteristics.Characteristics ;
using Idasen.BluetoothLE.Characteristics.Characteristics.Customs ;
using Idasen.BluetoothLE.Characteristics.Common ;
using Idasen.BluetoothLE.Characteristics.Interfaces.Characteristics ;
using Idasen.BluetoothLE.Characteristics.Interfaces.Characteristics.Customs ;
using Idasen.BluetoothLE.Characteristics.Interfaces.Common ;
using Idasen.BluetoothLE.Core ;

namespace Idasen.BluetoothLE.Characteristics ;

// ReSharper disable once InconsistentNaming
/// <summary>
///     Autofac module registering Bluetooth LE characteristics, providers, and helpers
///     with interface interceptors for AOP logging.
/// </summary>
[ ExcludeFromCodeCoverage ]
public sealed class BluetoothLEDeskCharacteristics : Module
{
    /// <summary>
    ///     Registers all services for the characteristics package into the Autofac container.
    /// </summary>
    /// <param name="builder">The container builder.</param>
    protected override void Load ( ContainerBuilder builder )
    {
        Guard.ArgumentNotNull ( builder ,
                                nameof ( builder ) ) ;

        builder.RegisterModule < BluetoothLECoreModule > ( ) ;

        RegisterWithInterceptors < IAllGattCharacteristicsProvider , AllGattCharacteristicsProvider > ( builder ) ;
        RegisterWithInterceptors < IGattCharacteristicProvider , GattCharacteristicProvider > ( builder ) ;
        RegisterWithInterceptors < IGattCharacteristicsProviderFactory ,
            GattCharacteristicsProviderFactory > ( builder ) ;
        RegisterWithInterceptors < IRawValueReader , RawValueReader > ( builder ) ;
        RegisterWithInterceptors < IRawValueWriter , RawValueWriter > ( builder ) ;
        RegisterWithInterceptors < IGenericAccess , GenericAccess > ( builder ) ;
        RegisterWithInterceptors < IGenericAttributeService , GenericAttributeService > ( builder ) ;
        RegisterWithInterceptors < IReferenceInput , ReferenceInput > ( builder ) ;
        RegisterWithInterceptors < IReferenceOutput , ReferenceOutput > ( builder ) ;
        RegisterWithInterceptors < IDpg , Dpg > ( builder ) ;
        RegisterWithInterceptors < IControl , Control > ( builder ) ;
        RegisterWithInterceptors < ICharacteristicBaseToStringConverter ,
            CharacteristicBaseToStringConverter > ( builder ) ;
        RegisterWithInterceptors < IBufferReader , BufferReader > ( builder ) ;
        RegisterWithInterceptors < IDescriptionToUuid , DescriptionToUuid > ( builder ) ;
        RegisterWithInterceptors < ICharacteristicBaseFactory , CharacteristicBaseFactory > ( builder ) ;
        RegisterWithInterceptors < IRawValueHandler , RawValueHandler > ( builder ) ;
    }

    /// <summary>
    ///     Registers the implementation with its interface and enables interface interceptors.
    /// </summary>
    private static void RegisterWithInterceptors < TInterface , TImplementation > ( ContainerBuilder builder )
        where TImplementation : TInterface where TInterface : notnull
    {
        builder.RegisterType < TImplementation > ( )
               .As < TInterface > ( )
               .EnableInterfaceInterceptors ( ) ;
    }
}