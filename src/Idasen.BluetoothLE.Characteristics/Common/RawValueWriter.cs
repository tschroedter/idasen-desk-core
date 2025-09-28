using Windows.Devices.Bluetooth.GenericAttributeProfile ;
using Windows.Storage.Streams ;
using Autofac.Extras.DynamicProxy ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Characteristics.Interfaces.Common ;
using Idasen.BluetoothLE.Core ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery.Wrappers ;
using Idasen.BluetoothLE.Core.ServicesDiscovery.Wrappers ;
using Serilog ;

namespace Idasen.BluetoothLE.Characteristics.Common ;

/// <inheritdoc />
[ Intercept ( typeof ( LogAspect ) ) ]
public class RawValueWriter
    : IRawValueWriter
{
    /// <inheritdoc />
    public async Task < bool > TryWriteValueAsync (
        IGattCharacteristicWrapper characteristic ,
        IBuffer                    buffer )
    {
        Guard.ArgumentNotNull (
                               characteristic ,
                               nameof ( characteristic ) ) ;
        Guard.ArgumentNotNull (
                               buffer ,
                               nameof ( buffer ) ) ;

        if ( ! IsSupported (
                            characteristic ,
                            GattCharacteristicProperties.Write ) ) {
            LogUnsupported (
                            characteristic ,
                            "Write" ) ;
            return false ;
        }

        var status = await characteristic.WriteValueAsync ( buffer ) ;

        return status == GattCommunicationStatus.Success ;
    }

    /// <inheritdoc />
    public async Task < bool > TryWritableAuxiliariesValueAsync (
        IGattCharacteristicWrapper characteristic ,
        IBuffer                    buffer )
    {
        Guard.ArgumentNotNull (
                               characteristic ,
                               nameof ( characteristic ) ) ;
        Guard.ArgumentNotNull (
                               buffer ,
                               nameof ( buffer ) ) ;

        if ( ! IsSupported (
                            characteristic ,
                            GattCharacteristicProperties.WritableAuxiliaries ) ) {
            LogUnsupported (
                            characteristic ,
                            "WritableAuxiliaries" ) ;
            return false ;
        }

        var status = await characteristic.WriteValueAsync ( buffer ) ;

        return status == GattCommunicationStatus.Success ;
    }

    /// <inheritdoc />
    public async Task < IGattWriteResultWrapper > TryWriteWithoutResponseAsync (
        IGattCharacteristicWrapper characteristic ,
        IBuffer                    buffer )
    {
        Guard.ArgumentNotNull (
                               characteristic ,
                               nameof ( characteristic ) ) ;
        Guard.ArgumentNotNull (
                               buffer ,
                               nameof ( buffer ) ) ;

        if ( ! IsSupported (
                            characteristic ,
                            GattCharacteristicProperties.WriteWithoutResponse ) ) {
            LogUnsupported (
                            characteristic ,
                            "WriteWithoutResponse" ) ;
            return GattWriteResultWrapper.NotSupported ;
        }

        var status = await characteristic.WriteValueWithResultAsync ( buffer ) ;

        return status ;
    }

    private static bool IsSupported (
        IGattCharacteristicWrapper   characteristic ,
        GattCharacteristicProperties needed ) =>
        ( characteristic.CharacteristicProperties & needed ) == needed ;

    private static void LogUnsupported (
        IGattCharacteristicWrapper characteristic ,
        string                     capability )
    {
        Log.Information (
                         "GattCharacteristic '{Uuid}' doesn't support '{Capability}'" ,
                         characteristic.Uuid ,
                         capability ) ;
    }
}
