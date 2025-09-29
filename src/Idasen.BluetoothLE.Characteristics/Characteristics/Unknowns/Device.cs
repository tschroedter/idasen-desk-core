using System.Collections.Immutable ;
using Windows.Devices.Bluetooth ;
using Windows.Devices.Bluetooth.GenericAttributeProfile ;
using Idasen.BluetoothLE.Characteristics.Common ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery.Wrappers ;

namespace Idasen.BluetoothLE.Characteristics.Characteristics.Unknowns ;

public class Device ( bool isPaired = false ) : IDevice
{
    internal const ulong UnknownBluetoothAddress = 0u ;

    internal const string UnknownBluetoothAddressType = "Unknown Address Type" ;
    internal const string UnknownName                 = "Unknown Device" ;
    internal const string UnknownId                   = "Unknown Device Id" ;
    internal const string Message                     = "Can't use an unknown instance" ;

    public void Dispose ( )
    {
        GC.SuppressFinalize ( this ) ;
    }

    public IObservable < BluetoothConnectionStatus > ConnectionStatusChanged =>
        throw new NotInitializeException ( Message ) ;

    public GattCommunicationStatus   GattCommunicationStatus { get ; } = GattCommunicationStatus.Unreachable ;
    public string                    Name                    { get ; } = UnknownName ;
    public string                    Id                      { get ; } = UnknownId ;
    public bool                      IsPaired                { get ; } = isPaired ;
    public BluetoothConnectionStatus ConnectionStatus        { get ; } = BluetoothConnectionStatus.Disconnected ;

    public IReadOnlyDictionary < IGattDeviceServiceWrapper , IGattCharacteristicsResultWrapper >
        GattServices { get ; } =
        new Dictionary < IGattDeviceServiceWrapper , IGattCharacteristicsResultWrapper > ( )
           .ToImmutableDictionary ( ) ;

    public IObservable < GattCommunicationStatus > GattServicesRefreshed =>
        throw new NotInitializeException ( Message ) ;

    public ulong BluetoothAddress => UnknownBluetoothAddress ;

    public string BluetoothAddressType => UnknownBluetoothAddressType ;

    public void Connect ( )
    {
        // do nothing
    }
}