using System.Diagnostics.CodeAnalysis ;
using Idasen.BluetoothLE.Linak.Interfaces ;

namespace Idasen.BluetoothLE.Linak.Control ;

[ExcludeFromCodeCoverage]
public class DeskMovementHandlers ( IDeskMovementMonitorFactory monitorFactory ,
                                    IDeskCommandExecutor        commandExecutor ,
                                    IStoppingHeightCalculator   calculator ,
                                    IDeskMoveEngine             moveEngine ,
                                    IDeskMoveGuard              moveGuard )
    : IDeskMovementHandlers
{
    public IDeskMovementMonitorFactory MonitorFactory  { get ; } = monitorFactory ;
    public IDeskCommandExecutor        CommandExecutor { get ; } = commandExecutor ;
    public IStoppingHeightCalculator   Calculator      { get ; } = calculator ;
    public IDeskMoveEngine             MoveEngine      { get ; } = moveEngine ;
    public IDeskMoveGuard              MoveGuard       { get ; } = moveGuard ;
}