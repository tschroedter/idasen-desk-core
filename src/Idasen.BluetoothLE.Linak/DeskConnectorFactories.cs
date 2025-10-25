using Idasen.BluetoothLE.Linak.Interfaces ;

namespace Idasen.BluetoothLE.Linak ;

public class DeskConnectorFactories ( IDeskHeightAndSpeedFactory  heightAndSpeedFactory ,
                                      IDeskCommandExecutorFactory commandExecutorFactory ,
                                      IDeskMoverFactory           moverFactory ,
                                      IDeskLockerFactory          lockerFactory )
    : IDeskConnectorFactories
{
    public IDeskHeightAndSpeedFactory  HeightAndSpeedFactory  { get ; } = heightAndSpeedFactory ;
    public IDeskCommandExecutorFactory CommandExecutorFactory { get ; } = commandExecutorFactory ;
    public IDeskMoverFactory           MoverFactory           { get ; } = moverFactory ;
    public IDeskLockerFactory          LockerFactory          { get ; } = lockerFactory ;
}