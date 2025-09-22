using System.Diagnostics.CodeAnalysis ;
using System.Text ;
using JetBrains.Annotations ;
using Serilog.Sinks.File ;

namespace Idasen.Launcher ;

/// <summary>
///     Serilog file sink lifecycle hooks that capture the fully-resolved log file path when the sink opens the file.
/// </summary>
[ UsedImplicitly ]
[ ExcludeFromCodeCoverage ]
public sealed class LoggingFileHooks : FileLifecycleHooks
{
    /// <summary>
    ///     Gets the full path of the currently opened log file as reported by the sink.
    /// </summary>
    public string FullPath { get ; private set ; } = string.Empty ;

    /// <summary>
    ///     Called by Serilog when the file sink opens the log file. Updates <see cref="FullPath" /> and
    ///     the global <see cref="LoggingFile.FullPath" /> value.
    /// </summary>
    /// <param name="path">Full path of the file being opened.</param>
    /// <param name="underlyingStream">The underlying stream used by the sink.</param>
    /// <param name="encoding">The text encoding used by the sink.</param>
    /// <returns>The stream to be used by the sink (typically the base implementation's result).</returns>
    public override Stream OnFileOpened ( string path , Stream underlyingStream , Encoding encoding )
    {
        ArgumentNullException.ThrowIfNull ( path ) ;
        ArgumentNullException.ThrowIfNull ( underlyingStream ) ;
        ArgumentNullException.ThrowIfNull ( encoding ) ;

        FullPath = path ;
        LoggingFile.FullPath = path ;

        return base.OnFileOpened ( path ,
                                   underlyingStream ,
                                   encoding ) ;
    }
}