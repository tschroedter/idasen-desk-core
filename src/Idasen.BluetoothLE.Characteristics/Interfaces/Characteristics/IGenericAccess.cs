// ReSharper disable UnusedMemberInSuper.Global

using System.Reactive.Subjects ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery ;

namespace Idasen.BluetoothLE.Characteristics.Interfaces.Characteristics ;

/// <summary>
///     More details can be found here: https://www.bluetooth.com/
/// </summary>
public interface IGenericAccess
    : ICharacteristicBase
{
    delegate IGenericAccess Factory ( IDevice device ) ;

    /// <summary>
    ///     Raw Central Address Resolution
    /// </summary>
    IEnumerable < byte > RawResolution { get ; }

    /// <summary>
    ///     Raw Peripheral Preferred Connection Parameters
    /// </summary>
    IEnumerable < byte > RawParameters { get ; }

    /// <summary>
    ///     Raw Appearance
    /// </summary>
    IEnumerable < byte > RawAppearance { get ; }

    /// <summary>
    ///     Raw Device Name
    /// </summary>
    IEnumerable < byte > RawDeviceName { get ; }

    /// <summary>
    ///     Raised when the Appearance changes.
    /// </summary>
    ISubject < IEnumerable < byte > > AppearanceChanged { get ; }

    /// <summary>
    ///     Raised when the Parameters change.
    /// </summary>
    ISubject < IEnumerable < byte > > ParametersChanged { get ; }

    /// <summary>
    ///     Raised when the Resolution changes.
    /// </summary>
    ISubject < IEnumerable < byte > > ResolutionChanged { get ; }

    /// <summary>
    ///     Raised when the DeviceName changes.
    /// </summary>
    ISubject < IEnumerable < byte > > DeviceNameChanged { get ; }

    /// <summary>
    ///     The UUID of the Gatt Service.
    /// </summary>
    Guid GattServiceUuid { get ; }
}
