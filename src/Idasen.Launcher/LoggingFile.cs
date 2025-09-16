using System.Diagnostics.CodeAnalysis ;
using JetBrains.Annotations ;

namespace Idasen.Launcher ;

[ ExcludeFromCodeCoverage ]
public static class LoggingFile
{
    public static string FullPath { get ; internal set ; } = string.Empty ;

    [ UsedImplicitly ]
    public static string Path => System.IO.Path.GetDirectoryName ( FullPath ) ?? string.Empty ;
}