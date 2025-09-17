using Idasen.BluetoothLE.Characteristics.Characteristics ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery ;

// ReSharper disable UnusedMemberInSuper.Global

namespace Idasen.BluetoothLE.Characteristics.Interfaces.Characteristics ;

/// <summary>
///     Reference Output service contract.
/// </summary>
public interface IReferenceOutput
    : ICharacteristicBase
{
    /// <summary>
    ///     Factory for creating instances per device.
    /// </summary>
    delegate IReferenceOutput Factory ( IDevice device ) ;

    /// <summary>
    ///     UUID of the service.
    /// </summary>
    Guid GattServiceUuid { get ; }
    /// <summary>
    ///     Raw HeightSpeed value.
    /// </summary>
    IEnumerable < byte > RawHeightSpeed { get ; }
    IEnumerable < byte > RawTwo { get ; }
    IEnumerable < byte > RawThree { get ; }
    IEnumerable < byte > RawFour { get ; }
    IEnumerable < byte > RawFive { get ; }
    IEnumerable < byte > RawSix { get ; }
    IEnumerable < byte > RawSeven { get ; }
    IEnumerable < byte > RawEight { get ; }
    IEnumerable < byte > RawMask { get ; }
    IEnumerable < byte > RawDetectMask { get ; }

    /// <summary>
    ///     Stream of HeightSpeed change notifications.
    /// </summary>
    IObservable < RawValueChangedDetails > HeightSpeedChanged { get ; }
}