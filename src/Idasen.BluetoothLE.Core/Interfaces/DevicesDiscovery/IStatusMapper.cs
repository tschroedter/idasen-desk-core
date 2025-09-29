using Idasen.BluetoothLE.Core.DevicesDiscovery;
using Windows.Devices.Bluetooth.Advertisement;

namespace Idasen.BluetoothLE.Core.Interfaces.DevicesDiscovery;

public interface IStatusMapper
{
    Status Map(BluetoothLEAdvertisementWatcherStatus status);
}
