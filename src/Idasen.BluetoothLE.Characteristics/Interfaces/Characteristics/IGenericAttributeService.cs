// ReSharper disable UnusedMemberInSuper.Global

namespace Idasen.BluetoothLE.Characteristics.Interfaces.Characteristics ;

/// <summary>
///     Generic Attribute service contract.
/// </summary>
public interface IGenericAttributeService
    : ICharacteristicBase
{
    /// <summary>
    ///     Raw bytes of the Service Changed characteristic.
    /// </summary>
    IEnumerable < byte > RawServiceChanged { get ; }
}