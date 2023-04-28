using Idasen.BluetoothLE.Linak.Control ;

namespace Idasen.BluetoothLE.Linak.Interfaces
{
    public interface IInitialHeightAndSpeedProviderFactory
    {
        IInitialHeightProvider Create ( IDeskCommandExecutor   executor ,
                                        IDeskHeightAndSpeed heightAndSpeed ) ;
    }
}