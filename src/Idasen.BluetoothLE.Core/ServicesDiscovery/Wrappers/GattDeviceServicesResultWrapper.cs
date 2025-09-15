using System.Diagnostics.CodeAnalysis ;
using Windows.Devices.Bluetooth.GenericAttributeProfile ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery.Wrappers ;

namespace Idasen.BluetoothLE.Core.ServicesDiscovery.Wrappers
{
    /// <inheritdoc />
    [ ExcludeFromCodeCoverage ]
    public class GattDeviceServicesResultWrapper
        : IGattDeviceServicesResultWrapper, IDisposable
    {
        public GattDeviceServicesResultWrapper (
            GattDeviceServiceWrapper.Factory serviceWrapperFactory ,
            GattDeviceServicesResult         service )
        {
            Guard.ArgumentNotNull ( serviceWrapperFactory , nameof ( serviceWrapperFactory ) ) ;
            Guard.ArgumentNotNull ( service , nameof ( service ) ) ;

            _service               = service ;
            _serviceWrapperFactory = serviceWrapperFactory ;

            var services = _service.Services ?? Array.Empty<GattDeviceService>();

            _services = services
                                .Select ( s => _serviceWrapperFactory ( s ) )
                                .ToArray ( ) ;
        }

        /// <inheritdoc />
        public GattCommunicationStatus Status => _service.Status ;

        /// <inheritdoc />
        public IEnumerable < IGattDeviceServiceWrapper > Services => _services ;

        /// <inheritdoc />
        public byte ? ProtocolError => _service.ProtocolError ;

        public delegate IGattDeviceServicesResultWrapper Factory ( GattDeviceServicesResult service ) ;

        public void Dispose ( )
        {
            foreach ( var s in _services )
            {
                s.Dispose ( ) ;
            }
        }

        private readonly GattDeviceServicesResult                  _service ;
        private readonly GattDeviceServiceWrapper.Factory          _serviceWrapperFactory ;
        private readonly IEnumerable < IGattDeviceServiceWrapper > _services ;
    }
}