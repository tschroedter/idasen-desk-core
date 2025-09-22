using Autofac.Extras.DynamicProxy ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Linak.Interfaces ;

namespace Idasen.BluetoothLE.Linak ;

/// <inheritdoc />
[Intercept ( typeof ( LogAspect ) ) ]
public sealed class Desk
    : IDesk
{
    private readonly IDeskConnector _connector ;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Desk" /> class.
    /// </summary>
    /// <param name="connector">Connector handling the actual BLE communication and state.</param>
    public Desk (
        IDeskConnector connector )
    {
        ArgumentNullException.ThrowIfNull ( connector ) ;

        _connector = connector ;
    }

    /// <inheritdoc />
    public ulong BluetoothAddress => _connector.BluetoothAddress ;

    /// <inheritdoc />
    public string BluetoothAddressType => _connector.BluetoothAddressType ;

    /// <inheritdoc />
    public void Connect ( ) => _connector.Connect ( ) ;

    /// <inheritdoc />
    public IObservable < IEnumerable < byte > > DeviceNameChanged => _connector.DeviceNameChanged ;

    /// <inheritdoc />
    public IObservable < uint > HeightChanged => _connector.HeightChanged ;

    /// <inheritdoc />
    public IObservable < int > SpeedChanged => _connector.SpeedChanged ;

    /// <inheritdoc />
    public IObservable < HeightSpeedDetails > HeightAndSpeedChanged =>
        _connector.HeightAndSpeedChanged ;

    /// <inheritdoc />
    public IObservable < uint > FinishedChanged => _connector.FinishedChanged ;

    /// <inheritdoc />
    public IObservable < bool > RefreshedChanged => _connector.RefreshedChanged ;

    /// <inheritdoc />
    public string Name => _connector.DeviceName ;

    /// <inheritdoc />
    public void MoveTo ( uint targetHeight )
    {
        if ( targetHeight == 0u )
        {
            throw new ArgumentOutOfRangeException ( nameof ( targetHeight ) , "Target height must be greater than 0." ) ;
        }

        _connector.MoveTo ( targetHeight ) ;
    }

    /// <inheritdoc />
    [Obsolete("Use MoveUpAsync() instead.")]
    public void MoveUp ( ) => _ = _connector.MoveUp ( ) ;

    /// <inheritdoc />
    [Obsolete("Use MoveDownAsync() instead.")]
    public void MoveDown ( ) => _ = _connector.MoveDown ( ) ;

    /// <inheritdoc />
    [Obsolete("Use MoveStopAsync() instead.")]
    public void MoveStop ( ) => _ = _connector.MoveStop ( ) ;

    /// <inheritdoc />
    [Obsolete("Use MoveLockAsync() instead.")]
    public void MoveLock ( ) => _ = _connector.MoveLock ( ) ;

    /// <inheritdoc />
    [Obsolete("Use MoveUnlockAsync() instead.")]
    public void MoveUnlock ( ) => _ = _connector.MoveUnlock ( ) ;

    /// <inheritdoc />
    public Task<bool> MoveUpAsync() => _connector.MoveUp();

    /// <inheritdoc />
    public Task<bool> MoveDownAsync() => _connector.MoveDown();

    /// <inheritdoc />
    public Task<bool> MoveStopAsync() => _connector.MoveStop();

    /// <inheritdoc />
    public Task<bool> MoveLockAsync() => _connector.MoveLock();

    /// <inheritdoc />
    public Task<bool> MoveUnlockAsync() => _connector.MoveUnlock();

    /// <inheritdoc />
    public void Dispose ( ) => _connector.Dispose ( ) ;

    /// <inheritdoc />
    public string DeviceName => _connector.DeviceName ;
}