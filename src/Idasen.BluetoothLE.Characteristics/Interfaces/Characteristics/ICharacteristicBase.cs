namespace Idasen.BluetoothLE.Characteristics.Interfaces.Characteristics ;

/// <summary>
///     Base contract for all characteristic services.
/// </summary>
public interface ICharacteristicBase
    : IDisposable
{
    /// <summary>
    ///     Initializes the characteristic and returns this instance cast to the requested type.
    /// </summary>
    /// <typeparam name="T">The characteristic-specific type to return.</typeparam>
    /// <returns>The current instance cast to <typeparamref name="T" />.</returns>
    T? Initialize<T> ( )
        where T : class ;

    /// <summary>
    ///     Refreshes the characteristic values by reading from the device and updating internal state.
    /// </summary>
    public Task Refresh ( ) ;
}
