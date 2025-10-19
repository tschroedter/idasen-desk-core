namespace Idasen.BluetoothLE.Linak.Interfaces ;

public interface IDeskMovementHandlers
{

    public IDeskMovementMonitorFactory MonitorFactory  { get ; }
    public IDeskCommandExecutor        Executor { get ; }
    public IStoppingHeightCalculator   Calculator      { get ; }
    public IDeskMoveEngine             MoveEngine      { get ; }
    public IDeskMoveGuard              MoveGuard       { get ; }
}
