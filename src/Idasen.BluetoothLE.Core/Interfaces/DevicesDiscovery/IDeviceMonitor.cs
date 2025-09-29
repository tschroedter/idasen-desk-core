namespace Idasen.BluetoothLE.Core.Interfaces.DevicesDiscovery ;

/// <summary>
///     Monitors discovered devices.
/// </summary>
public interface IDeviceMonitor
    : IDisposable
{
    /// <summary>
    ///     Collection of discovered devices.
    /// </summary>
    IReadOnlyCollection < IDevice > DiscoveredDevices { get ; }

    /// <summary>
    ///     Flag indicating if the watcher is listening or not.
    /// </summary>
    bool IsListening { get ; }

    /// <summary>
    ///     Fired when a device is updated.
    /// </summary>
    IObservable < IDevice > DeviceUpdated { get ; }

    /// <summary>
    ///     Fired when a new device is discovered.
    /// </summary>
    IObservable < IDevice > DeviceDiscovered { get ; }

    /// <summary>
    ///     Fired when a device name is updated.
    /// </summary>
    IObservable < IDevice > DeviceNameUpdated { get ; }

    /// <summary>
    ///     Starts listening.
    /// </summary>
    void StartListening ( ) ;

    /// <summary>
    ///     Stops listening.
    /// </summary>
    void StopListening ( ) ;

    /// <summary>
    ///     Remove a device from the list of discovered devices.
    /// </summary>
    /// <param name="device"></param>
    void RemoveDevice ( IDevice device ) ;
}
