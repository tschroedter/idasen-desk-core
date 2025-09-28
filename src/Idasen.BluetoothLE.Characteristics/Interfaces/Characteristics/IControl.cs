// ReSharper disable UnusedMemberInSuper.Global

namespace Idasen.BluetoothLE.Characteristics.Interfaces.Characteristics ;

/// <summary>
///     Control service for sending raw commands to the device.
/// </summary>
public interface IControl
    : ICharacteristicBase
{
    /// <summary>
    ///     Raw bytes of the Control2 characteristic.
    /// </summary>
    IEnumerable < byte > RawControl2 { get ; }

    /// <summary>
    ///     Raw bytes of the Control3 characteristic.
    /// </summary>
    IEnumerable < byte > RawControl3 { get ; }

    /// <summary>
    ///     Attempts to write raw bytes to the Control2 characteristic.
    /// </summary>
    Task < bool > TryWriteRawControl2 ( IEnumerable < byte > bytes ) ;
}
