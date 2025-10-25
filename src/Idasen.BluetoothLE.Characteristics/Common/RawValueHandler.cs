using Windows.Storage.Streams ;
using Idasen.BluetoothLE.Characteristics.Interfaces.Common ;
using Idasen.BluetoothLE.Core ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery.Wrappers ;

namespace Idasen.BluetoothLE.Characteristics.Common ;

/// <summary>
///     Handles reading and writing raw byte values to and from GATT characteristics.
/// </summary>
public sealed class RawValueHandler
    : IRawValueHandler
{
    public RawValueHandler ( IRawValueReader valueReader ,
                             IRawValueWriter valueWriter )
    {
        Guard.ArgumentNotNull ( valueReader ,
                                nameof ( valueReader ) ) ;
        Guard.ArgumentNotNull ( valueWriter ,
                                nameof ( valueWriter ) ) ;

        RawValueReader = valueReader ;
        RawValueWriter = valueWriter ;
    }

    public async Task < (bool , byte [ ]) > TryReadValueAsync ( IGattCharacteristicWrapper characteristic )
    {
        return await RawValueReader.TryReadValueAsync ( characteristic ) ;
    }

    public async Task < bool > TryWriteValueAsync ( IGattCharacteristicWrapper characteristic ,
                                                    IBuffer                    buffer )
    {
        return await RawValueWriter.TryWriteValueAsync ( characteristic ,
                                                         buffer ) ;
    }

    public Task < bool > TryWritableAuxiliariesValueAsync ( IGattCharacteristicWrapper characteristic ,
                                                            IBuffer                    buffer )
    {
        return RawValueWriter.TryWritableAuxiliariesValueAsync ( characteristic ,
                                                                 buffer ) ;
    }

    public Task < IGattWriteResultWrapper > TryWriteWithoutResponseAsync ( IGattCharacteristicWrapper characteristic ,
                                                                           IBuffer                    buffer )
    {
        return RawValueWriter.TryWriteWithoutResponseAsync ( characteristic ,
                                                             buffer ) ;
    }

    public IRawValueReader RawValueReader { get ; }

    public IRawValueWriter RawValueWriter { get ; }
}