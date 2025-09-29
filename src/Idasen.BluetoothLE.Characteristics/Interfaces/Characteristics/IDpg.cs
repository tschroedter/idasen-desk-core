// ReSharper disable UnusedMemberInSuper.Global

using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery ;

namespace Idasen.BluetoothLE.Characteristics.Interfaces.Characteristics ;

/// <summary>
///     DPG (desk panel) service contract.
/// </summary>
public interface IDpg
    : ICharacteristicBase
{
    /// <summary>
    ///     Factory for creating instances per device.
    /// </summary>
    delegate IDpg Factory ( IDevice device ) ;

    /// <summary>
    ///     Raw bytes of the DPG characteristic.
    /// </summary>
    IEnumerable < byte > RawDpg { get ; }
}