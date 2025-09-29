using System.Diagnostics.CodeAnalysis ;
using Windows.Devices.Bluetooth.GenericAttributeProfile ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery.Wrappers ;

namespace Idasen.BluetoothLE.Core.ServicesDiscovery.Wrappers ;

[ ExcludeFromCodeCoverage ]
public class GattDeviceServicesResultWrapper
    : IGattDeviceServicesResultWrapper ,
      IDisposable
{
    public delegate IGattDeviceServicesResultWrapper Factory ( GattDeviceServicesResult service ) ;

    private readonly GattDeviceServicesResult _service ;

    public GattDeviceServicesResultWrapper ( GattDeviceServiceWrapper.Factory serviceWrapperFactory ,
                                             GattDeviceServicesResult         service )
    {
        Guard.ArgumentNotNull ( serviceWrapperFactory ,
                                nameof ( serviceWrapperFactory ) ) ;
        Guard.ArgumentNotNull ( service ,
                                nameof ( service ) ) ;

        _service = service ;
        var serviceWrapperFactory1 = serviceWrapperFactory ;

        IReadOnlyList < GattDeviceService > services = _service.Services ?? [] ;

        Services = services.Select ( s => serviceWrapperFactory1 ( s ) )
                           .ToArray ( ) ;
    }

    public void Dispose ( )
    {
        foreach ( var s in Services ) s.Dispose ( ) ;

        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public GattCommunicationStatus Status => _service.Status ;

    /// <inheritdoc />
    public IEnumerable < IGattDeviceServiceWrapper > Services { get ; }

    /// <inheritdoc />
    public byte ? ProtocolError => _service.ProtocolError ;
}
