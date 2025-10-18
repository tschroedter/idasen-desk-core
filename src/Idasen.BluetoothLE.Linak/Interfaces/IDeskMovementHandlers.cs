namespace Idasen.BluetoothLE.Linak.Interfaces ;

public interface IDeskMovementHandlers
{

    public IDeskMovementMonitorFactory MonitorFactory  { get ; }
    public IDeskCommandExecutor        CommandExecutor { get ; }
    public IStoppingHeightCalculator   Calculator      { get ; }
    public IDeskMoveEngine             MoveEngine      { get ; }
    public IDeskMoveGuard              MoveGuard       { get ; }
}
