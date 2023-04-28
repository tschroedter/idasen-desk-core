namespace Idasen.BluetoothLE.Linak.Interfaces
{
    public interface IDeskLockerFactory
    {
        IDeskLocker Create ( IDeskMover           deskMover ,
                             IDeskCommandExecutor executer ,
                             IDeskHeightAndSpeed  heightAndSpeed ) ;
    }
}