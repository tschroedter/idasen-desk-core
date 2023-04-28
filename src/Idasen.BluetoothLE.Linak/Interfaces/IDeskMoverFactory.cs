namespace Idasen.BluetoothLE.Linak.Interfaces
{
    public interface IDeskMoverFactory
    {
        IDeskMover Create ( IDeskCommandExecutor executor ,
                            IDeskHeightAndSpeed  heightAndSpeed ) ;
    }
}