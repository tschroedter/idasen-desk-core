namespace Idasen.BluetoothLE.Linak.Control;

/// <summary>
///     Result of a stop evaluation.
/// </summary>
public readonly record struct StopDetails(bool ShouldStop, Direction Desired);
