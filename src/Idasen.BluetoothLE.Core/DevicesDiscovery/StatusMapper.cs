namespace Idasen.BluetoothLE.Core.DevicesDiscovery ;

using Windows.Devices.Bluetooth.Advertisement ;
using Interfaces.DevicesDiscovery ;

public sealed class StatusMapper
    : IStatusMapper
{
    public Status Map ( BluetoothLEAdvertisementWatcherStatus status )
    {
        return status switch
               {
                   BluetoothLEAdvertisementWatcherStatus.Started => Status.Started ,
                   BluetoothLEAdvertisementWatcherStatus.Aborted => Status.Aborted ,
                   BluetoothLEAdvertisementWatcherStatus.Created => Status.Created ,
                   BluetoothLEAdvertisementWatcherStatus.Stopped => Status.Stopped ,
                   BluetoothLEAdvertisementWatcherStatus.Stopping => Status.Stopping ,
                   _ => throw new ArgumentException ( $"Unknown status: '{status}'!" )
               } ;
    }
}
