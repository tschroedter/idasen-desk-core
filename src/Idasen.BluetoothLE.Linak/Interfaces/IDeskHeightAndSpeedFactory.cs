using Idasen.BluetoothLE.Characteristics.Interfaces.Characteristics ;

namespace Idasen.BluetoothLE.Linak.Interfaces ;

/// <summary>
///     Creates <see cref="IDeskHeightAndSpeed" /> instances bound to a reference output characteristic.
/// </summary>
public interface IDeskHeightAndSpeedFactory
{
    /// <summary>
    ///     Creates a new instance for the given reference output.
    /// </summary>
    /// <param name="referenceOutput">The reference output characteristic to observe.</param>
    IDeskHeightAndSpeed Create ( IReferenceOutput referenceOutput ) ;
}