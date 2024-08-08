

// ReSharper disable UnusedMemberInSuper.Global

namespace Idasen.BluetoothLE.Linak.Interfaces
{
    public interface IErrorDetails
    {
        string Message { get ; }

        Exception ? Exception { get ; }
        string      Caller    { get ; }
    }
}