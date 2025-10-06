namespace Idasen.BluetoothLE.Linak.Control;

public static class StoppingHeightCalculatorSettings
{
    /// <summary>
    ///     The estimated units required to stop from max. movement speed.
    /// </summary>
    public static uint MaxSpeedToStopMovement { get; set; } = 40;

    /// <summary>
    ///     A multiplier used to adjust calculations for stopping height.
    /// </summary>
    public static float FudgeFactor { get; set; } = 2.0f;
}
