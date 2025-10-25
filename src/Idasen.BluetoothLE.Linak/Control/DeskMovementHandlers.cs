using System.Diagnostics.CodeAnalysis ;
using Idasen.BluetoothLE.Linak.Interfaces ;
using Serilog ;

namespace Idasen.BluetoothLE.Linak.Control ;

[ ExcludeFromCodeCoverage ]
public class DeskMovementHandlers
    : IDeskMovementHandlers
{
    public DeskMovementHandlers ( ILogger                     logger ,
                                  IDeskHeightAndSpeed         heightAndSpeed ,
                                  IDeskMovementMonitorFactory monitorFactory ,
                                  IDeskCommandExecutor        executor ,
                                  IStoppingHeightCalculator   calculator )
    {
        MonitorFactory = monitorFactory ;
        Executor       = executor ;
        Calculator     = calculator ;
        MoveEngine = new DeskMoveEngine ( logger ,
                                          executor ) ;
        MoveGuard = new DeskMoveGuard ( logger ,
                                        heightAndSpeed ,
                                        calculator ) ;
    }

    internal DeskMovementHandlers ( IDeskMovementMonitorFactory monitorFactory ,
                                    IDeskCommandExecutor        executor ,
                                    IStoppingHeightCalculator   calculator ,
                                    IDeskMoveEngine             engine ,
                                    IDeskMoveGuard              guard )
    {
        MonitorFactory = monitorFactory ;
        Executor       = executor ;
        Calculator     = calculator ;
        MoveEngine     = engine ;
        MoveGuard      = guard ;
    }

    public IDeskMovementMonitorFactory MonitorFactory { get ; }
    public IDeskCommandExecutor        Executor       { get ; }
    public IStoppingHeightCalculator   Calculator     { get ; }
    public IDeskMoveEngine             MoveEngine     { get ; }
    public IDeskMoveGuard              MoveGuard      { get ; }
}