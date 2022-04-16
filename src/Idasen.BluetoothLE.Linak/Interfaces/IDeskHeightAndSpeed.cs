using System ;
using System.Threading.Tasks ;
// ReSharper disable UnusedMemberInSuper.Global

namespace Idasen.BluetoothLE.Linak.Interfaces
{
    public interface IDeskHeightAndSpeed
        : IDisposable
    {
        IObservable < uint >               HeightChanged         { get ; }
        IObservable < int >                SpeedChanged          { get ; }
        uint                               Height                { get ; }
        int                                Speed                 { get ; }
        IObservable < HeightSpeedDetails > HeightAndSpeedChanged { get ; }
        Task                               Refresh ( ) ;
        IDeskHeightAndSpeed                Initialize ( ) ;
    }
}