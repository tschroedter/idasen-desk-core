namespace Idasen.BluetoothLE.Core.Interfaces.DevicesDiscovery ;

using Windows.Devices.Bluetooth.Advertisement ;
using Core.DevicesDiscovery ;

public interface IStatusMapper
{
    Status Map ( BluetoothLEAdvertisementWatcherStatus status ) ;
}
