namespace Idasen.BluetoothLE.Linak.Interfaces
{
    public interface ITaskRunner
    {
        Task Run ( Action            action ,
                   CancellationToken token ) ;
    }
}