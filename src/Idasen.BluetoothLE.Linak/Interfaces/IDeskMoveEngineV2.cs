using Idasen.BluetoothLE.Linak.Control;

namespace Idasen.BluetoothLE.Linak.Interfaces ;

/// <summary>
///     Interface for DeskMoveEngineV2, issues repeated move commands until stopped.
/// </summary>
public interface IDeskMoveEngineV2
{
    /// <summary>
    ///     Gets or sets the interval between repeated move commands.
    /// </summary>
    TimeSpan DelayInterval { get ; set ; }

    /// <summary>
    ///     Gets the current movement direction of the desk.
    /// </summary>
    Direction CurrentDirection { get ; }

    /// <summary>
    ///     Indicates whether the desk is currently moving.
    /// </summary>
    bool IsMoving { get ; }

    /// <summary>
    ///     Starts moving in the desired direction, issuing commands every DelayInterval until the cancellation token is
    ///     triggered.
    /// </summary>
    Task StartMoveAsync ( Direction         desired ,
                          CancellationToken cancellationToken ) ;

    /// <summary>
    ///     Stops issuing move commands.
    /// </summary>
    Task StopMoveAsync ( ) ;
}
