using System.Diagnostics.CodeAnalysis ;
using Idasen.BluetoothLE.Linak.Interfaces ;
using Serilog ;

namespace Idasen.BluetoothLE.Linak.Control ;

[ ExcludeFromCodeCoverage ]
public class DeskMovementHandlers ( ILogger                     logger ,
                                    IDeskHeightAndSpeed         heightAndSpeed ,
                                    IDeskMovementMonitorFactory monitorFactory ,
                                    IDeskCommandExecutor        commandExecutor ,
                                    IStoppingHeightCalculator   calculator )
    : IDeskMovementHandlers
{
    public IDeskMovementMonitorFactory MonitorFactory  { get ; } = monitorFactory ;
    public IDeskCommandExecutor        CommandExecutor { get ; } = commandExecutor ;
    public IStoppingHeightCalculator   Calculator      { get ; } = calculator ;

    public IDeskMoveEngine MoveEngine { get ; } = new DeskMoveEngine ( logger ,
                                                                       commandExecutor ) ;

    public IDeskMoveGuard MoveGuard { get ; } = new DeskMoveGuard ( logger ,
                                                                    heightAndSpeed ,
                                                                    calculator ) ;
}
