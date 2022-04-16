using System ;
// ReSharper disable UnusedMemberInSuper.Global

namespace Idasen.BluetoothLE.Linak.Interfaces
{
    public interface IDeskLocker
        : IDisposable
    {
        IDeskLocker Lock ( ) ;
        IDeskLocker Unlock ( ) ;
        IDeskLocker Initialize ( ) ;
        bool        IsLocked { get ; }
    }
}