using System.Diagnostics.CodeAnalysis;

namespace Idasen.Launcher ;

[ExcludeFromCodeCoverage]
public static class LoggingFile
{
    public static string FullPath { get; internal set ; } = string.Empty;

    public static string Path => System.IO.Path.GetDirectoryName(FullPath) ?? string.Empty; 
}