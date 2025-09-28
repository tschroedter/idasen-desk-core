namespace Idasen.BluetoothLE.Characteristics.Common ;

/// <summary>
///     DTO representing an official GATT characteristic record from the CSV resource.
/// </summary>
public sealed class CsvGattCharacteristic
{
    /// <summary>
    ///     The characteristic UUID as a string.
    /// </summary>
    public string Uuid { get ; set ; } = string.Empty ;

    /// <summary>
    ///     The official characteristic name.
    /// </summary>
    public string Name { get ; set ; } = string.Empty ;
}
