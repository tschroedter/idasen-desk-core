using System ;
using System.Runtime.CompilerServices ;

// ReSharper disable UnusedMember.Global

namespace Idasen.BluetoothLE.Linak.Interfaces
{
    public interface IErrorManager
    {
        /// <summary>
        ///     Notify when an error happened.
        /// </summary>
        IObservable < IErrorDetails > ErrorChanged { get ; }

        /// <summary>
        ///     Publish an error so the UI can show it.
        /// </summary>
        /// <param name="details">
        ///     The details about the error.
        /// </param>
        void Publish ( IErrorDetails details ) ;

        /// <summary>
        ///     Publish an error so the UI can show it.
        /// </summary>
        /// <param name="message">
        ///     The message to be displayed.
        /// </param>
        /// <param name="caller">
        ///     The caller information.
        /// </param>
        void PublishForMessage ( string message ,
                                 [ CallerMemberName ] string caller = "" ) ;
    }
}