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
[ ExcludeFromCodeCoverage ]
public class BluetoothLEDeskCharacteristics : Module
{
    protected override void Load ( ContainerBuilder builder )
    {
        builder.RegisterModule < BluetoothLECoreModule > ( ) ;

        RegisterWithInterceptors < IAllGattCharacteristicsProvider , AllGattCharacteristicsProvider > ( builder ) ;
        RegisterWithInterceptors < IGattCharacteristicProvider , GattCharacteristicProvider > ( builder ) ;
        RegisterWithInterceptors < IGattCharacteristicsProviderFactory , GattCharacteristicsProviderFactory > ( builder ) ;
        RegisterWithInterceptors < IRawValueReader , RawValueReader > ( builder ) ;
        RegisterWithInterceptors < IRawValueWriter , RawValueWriter > ( builder ) ;
        RegisterWithInterceptors < IGenericAccess , GenericAccess > ( builder ) ;
        RegisterWithInterceptors < IGenericAttribute , GenericAttribute > ( builder ) ;
        RegisterWithInterceptors < IReferenceInput , ReferenceInput > ( builder ) ;
        RegisterWithInterceptors < IReferenceOutput , ReferenceOutput > ( builder ) ;
        RegisterWithInterceptors < IDpg , Dpg > ( builder ) ;
        RegisterWithInterceptors < IControl , Control > ( builder ) ;
        RegisterWithInterceptors < ICharacteristicBaseToStringConverter , CharacteristicBaseToStringConverter > ( builder ) ;
        RegisterWithInterceptors < IBufferReader , BufferReader > ( builder ) ;
        RegisterWithInterceptors < IDescriptionToUuid , DescriptionToUuid > ( builder ) ;
        RegisterWithInterceptors < ICharacteristicBaseFactory , CharacteristicBaseFactory > ( builder ) ;
    }

    private void RegisterWithInterceptors<TInterface , TImplementation> ( ContainerBuilder builder )
        where TImplementation : TInterface where TInterface : notnull
    {
        builder.RegisterType < TImplementation > ( )
               .As < TInterface > ( )
               .EnableInterfaceInterceptors ( ) ;
    }
}