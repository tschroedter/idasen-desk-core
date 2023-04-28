using Idasen.BluetoothLE.Characteristics.Interfaces.Characteristics ;

namespace Idasen.BluetoothLE.Linak.Interfaces
{
    public interface IDeskCommandExecutorFactory
    {
        IDeskCommandExecutor Create ( IControl control ) ;
    }
}