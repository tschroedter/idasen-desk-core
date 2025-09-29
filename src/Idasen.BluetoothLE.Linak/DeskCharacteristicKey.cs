namespace Idasen.BluetoothLE.Linak ;

/// <summary>
///     Keys for known LINAK GATT characteristics.
/// </summary>
public enum DeskCharacteristicKey
{
    /// <summary>
    ///     No characteristic.
    /// </summary>
    None = 0 ,

    /// <summary>
    ///     Generic Access service characteristics.
    /// </summary>
    GenericAccess = 1 ,

    /// <summary>
    ///     Generic Attribute service characteristics.
    /// </summary>
    GenericAttribute = 2 ,

    /// <summary>
    ///     Reference Input service characteristics.
    /// </summary>
    ReferenceInput = 3 ,

    /// <summary>
    ///     Reference Output service characteristics.
    /// </summary>
    ReferenceOutput = 4 ,

    /// <summary>
    ///     DPG service characteristics.
    /// </summary>
    Dpg = 5 ,

    /// <summary>
    ///     Control service characteristics.
    /// </summary>
    Control = 6
}