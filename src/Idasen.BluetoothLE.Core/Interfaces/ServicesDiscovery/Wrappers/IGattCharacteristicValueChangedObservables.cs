using Idasen.BluetoothLE.Core.ServicesDiscovery.Wrappers;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery.Wrappers;

public interface IGattCharacteristicValueChangedObservables
    : IDisposable
{
    IObservable<GattCharacteristicValueChangedDetails> ValueChanged { get; }
    Task Initialise(GattCharacteristic characteristic);
}
