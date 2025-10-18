using System.Diagnostics.CodeAnalysis ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery.Wrappers ;
using Windows.Devices.Bluetooth.GenericAttributeProfile ;

namespace Idasen.BluetoothLE.Core.ServicesDiscovery.Wrappers ;

/// <inheritdoc />
[ ExcludeFromCodeCoverage ]
public class GattDeviceServiceWrapper
    : IGattDeviceServiceWrapper
{
    public delegate IGattDeviceServiceWrapper Factory ( GattDeviceService gattDeviceService ) ;

    private readonly IGattCharacteristicsResultWrapperFactory _characteristicsFactory ;
    private readonly GattDeviceService                        _gattDeviceService ;
    private          IGattCharacteristicsResultWrapper ?      _lastCharacteristics ;
    private          bool                                     _disposed ;

    public GattDeviceServiceWrapper ( IGattCharacteristicsResultWrapperFactory characteristicsFactory ,
                                      GattDeviceService                        gattDeviceService )
    {
        Guard.ArgumentNotNull ( characteristicsFactory ,
                                nameof ( characteristicsFactory ) ) ;
        Guard.ArgumentNotNull ( gattDeviceService ,
                                nameof ( gattDeviceService ) ) ;

        _characteristicsFactory = characteristicsFactory ;
        _gattDeviceService      = gattDeviceService ;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);

        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing) {
            _lastCharacteristics?.Dispose();
            _lastCharacteristics = null;
            _gattDeviceService.Dispose();
        }

        _disposed = true;
    }

    /// <inheritdoc />
    public Guid Uuid => _gattDeviceService.Uuid ;

    /// <inheritdoc />
    public string DeviceId => _gattDeviceService.DeviceId ;

    /// <inheritdoc />
    public async Task < IGattCharacteristicsResultWrapper > GetCharacteristicsAsync ( )
    {
        var characteristics = await _gattDeviceService.GetCharacteristicsAsync ( ).AsTask ( ) ;

        var result = _characteristicsFactory.Create ( characteristics ) ;

        _lastCharacteristics?.Dispose ( ) ;
        _lastCharacteristics = await result.Initialize ( ) ;

        return _lastCharacteristics ;
    }
}
