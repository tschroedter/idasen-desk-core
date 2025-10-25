namespace Idasen.BluetoothLE.Linak.Interfaces ;

public interface IDeskConnectorFactories
{
    IDeskHeightAndSpeedFactory  HeightAndSpeedFactory  { get ; }
    IDeskCommandExecutorFactory CommandExecutorFactory { get ; }
    IDeskMoverFactory           MoverFactory           { get ; }
    IDeskLockerFactory          LockerFactory          { get ; }
}