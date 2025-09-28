// ReSharper disable UnusedMemberInSuper.Global

namespace Idasen.BluetoothLE.Characteristics.Interfaces.Characteristics ;

/// <summary>
///     Reference Input service contract.
/// </summary>
public interface IReferenceInput
    : ICharacteristicBase
{
    /// <summary>
    ///     Raw bytes of Ctrl1 characteristic.
    /// </summary>
    IEnumerable < byte > Ctrl1 { get ; }
}
