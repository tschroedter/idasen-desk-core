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
    private readonly IRawValueReader _valueReader ;
    private readonly IRawValueWriter _valueWriter ;

    public RawValueHandler ( IRawValueReader valueReader ,
                             IRawValueWriter valueWriter )
    {
        Guard.ArgumentNotNull ( valueReader ,
                                nameof ( valueReader ) ) ;
        Guard.ArgumentNotNull ( valueWriter ,
                                nameof ( valueWriter ) ) ;

        _valueReader = valueReader ;
        _valueWriter = valueWriter ;
    }

    public async Task < (bool , byte [ ]) > TryReadValueAsync ( IGattCharacteristicWrapper characteristic )
    {
        return await _valueReader.TryReadValueAsync ( characteristic ) ;
    }

    public async Task < bool > TryWriteValueAsync ( IGattCharacteristicWrapper characteristic ,
                                                    IBuffer                    buffer )
    {
        return await _valueWriter.TryWriteValueAsync ( characteristic ,
                                                       buffer ) ;
    }

    public Task < bool > TryWritableAuxiliariesValueAsync ( IGattCharacteristicWrapper characteristic ,
                                                            IBuffer                    buffer )
    {
        return _valueWriter.TryWritableAuxiliariesValueAsync ( characteristic ,
                                                               buffer ) ;
    }

    public Task < IGattWriteResultWrapper > TryWriteWithoutResponseAsync ( IGattCharacteristicWrapper characteristic ,
                                                                           IBuffer                    buffer )
    {
        return _valueWriter.TryWriteWithoutResponseAsync ( characteristic ,
                                                           buffer ) ;
    }
}
