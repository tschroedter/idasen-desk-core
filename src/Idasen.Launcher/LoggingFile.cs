using System.Diagnostics.CodeAnalysis ;
using JetBrains.Annotations ;

namespace Idasen.Launcher ;

/// <summary>
///     Provides access to the current log file path for the application.
/// </summary>
[ ExcludeFromCodeCoverage ]
public static class LoggingFile
{
    /// <summary>
    ///     Gets the full path of the current log file. Set internally by logging infrastructure.
    /// </summary>
    public static string FullPath { get ; internal set ; } = string.Empty ;

    /// <summary>
    ///     Gets the directory that contains the current log file, or an empty string if not available.
    /// </summary>
    [ UsedImplicitly ]
    public static string Path => System.IO.Path.GetDirectoryName ( FullPath ) ?? string.Empty ;
}