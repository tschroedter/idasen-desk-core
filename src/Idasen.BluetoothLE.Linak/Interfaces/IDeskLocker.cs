

// ReSharper disable UnusedMemberInSuper.Global

namespace Idasen.BluetoothLE.Linak.Interfaces
{
    public interface IDeskLocker
        : IDisposable
    {
        bool        IsLocked { get ; }
        IDeskLocker Lock ( ) ;
        IDeskLocker Unlock ( ) ;
        IDeskLocker Initialize ( ) ;
    }
}