using Windows.Storage.Streams ;

namespace Idasen.BluetoothLE.Characteristics.Interfaces.Common
{
    public interface IBufferReader
    {
        bool TryReadValue (
            IBuffer  buffer ,
            out         byte [ ] bytes ) ;
    }
}