using System.Runtime.CompilerServices ;
using JetBrains.Annotations ;

// ReSharper disable UnusedMember.Global

namespace Idasen.BluetoothLE.Linak.Interfaces ;

/// <summary>
///     Publishes error notifications to interested subscribers and helpers to create standard error messages.
/// </summary>
public interface IErrorManager : IDisposable
{
    /// <summary>
    ///     Notify when an error happened.
    /// </summary>
    [ UsedImplicitly ]
    IObservable < IErrorDetails > ErrorChanged { get ; }

    /// <summary>
    ///     Publish an error so the UI can show it.
    /// </summary>
    /// <param name="details">
    ///     The details about the error.
    /// </param>
    [ UsedImplicitly ]
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