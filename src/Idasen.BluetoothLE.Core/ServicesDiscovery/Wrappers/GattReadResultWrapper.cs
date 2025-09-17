using System.Diagnostics.CodeAnalysis ;
using Windows.Devices.Bluetooth.GenericAttributeProfile ;
using Windows.Storage.Streams ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery.Wrappers ;

// ReSharper disable UnusedMember.Global

namespace Idasen.BluetoothLE.Core.ServicesDiscovery.Wrappers ;

[ ExcludeFromCodeCoverage ]
/// <summary>
///     Wrapper around <see cref="GattReadResult"/> exposing status, protocol error, and value.
/// </summary>
public class GattReadResultWrapper
    : IGattReadResultWrapper
{
    /// <summary>
    ///     Factory delegate for creating wrapper instances from platform results.
    /// </summary>
    public delegate IGattReadResultWrapper Factory ( GattReadResult result ) ;

    /// <summary>
    ///     Sentinel instance indicating the operation is not supported.
    /// </summary>
    public static readonly IGattReadResultWrapper NotSupported = new GattReadResultWrapperNotSupported ( ) ;

    private readonly GattReadResult _result ;

    /// <summary>
    ///     Initializes a new instance of the <see cref="GattReadResultWrapper"/> class.
    /// </summary>
    public GattReadResultWrapper ( GattReadResult result )
    {
        Guard.ArgumentNotNull ( result ,
                                nameof ( result ) ) ;

        _result = result ;
    }

    /// <summary>
    ///     Gets the GATT communication status.
    /// </summary>
    public GattCommunicationStatus Status => _result.Status ;

    /// <summary>
    ///     Gets the protocol error, if provided.
    /// </summary>
    public byte? ProtocolError => _result.ProtocolError ;

    /// <summary>
    ///     Gets the underlying value buffer.
    /// </summary>
    public IBuffer? Value => _result.Value ;
}