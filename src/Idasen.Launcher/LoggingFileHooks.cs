using System.Diagnostics.CodeAnalysis ;
using System.Text ;
using JetBrains.Annotations ;
using Serilog.Sinks.File ;

namespace Idasen.Launcher ;

[UsedImplicitly]
[ExcludeFromCodeCoverage]
public class LoggingFileHooks : FileLifecycleHooks
{
    public string FullPath { get; private set; } = string.Empty;

    public override Stream OnFileOpened(string path, Stream underlyingStream, Encoding encoding)
    {
        FullPath = path;
        LoggingFile.FullPath = path;

        return base.OnFileOpened(path, underlyingStream, encoding);
    }
}