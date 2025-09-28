namespace Idasen.BluetoothLE.Characteristics.Interfaces.Characteristics.Customs ;

using Core.Interfaces.ServicesDiscovery.Wrappers ;

/// <summary>
///     Factory for creating <see cref="IGattCharacteristicProvider" /> instances from platform wrappers.
/// </summary>
public interface IGattCharacteristicsProviderFactory
{
    /// <summary>
    ///     Creates a provider for the given GATT characteristics result.
    /// </summary>
    /// <param name="wrapper">The platform result wrapper.</param>
    /// <returns>An initialized provider.</returns>
    IGattCharacteristicProvider Create ( IGattCharacteristicsResultWrapper wrapper ) ;
}
