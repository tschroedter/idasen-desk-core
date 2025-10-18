using System.Diagnostics.CodeAnalysis ;
using Serilog ;
using Serilog.Configuration ;

namespace Idasen.Launcher ;

/// <summary>
///     Extension methods for configuring Serilog caller enrichment.
/// </summary>
[ ExcludeFromCodeCoverage ]
public static class LoggerCallerEnrichmentConfiguration
{
    /// <summary>
    ///     Registers the <see cref="CallerEnricher" /> to populate the <c>Caller</c> log property.
    /// </summary>
    /// <param name="enrichmentConfiguration">The enrichment configuration builder.</param>
    /// <returns>The updated <see cref="LoggerConfiguration" />.</returns>
    public static LoggerConfiguration WithCaller ( this LoggerEnrichmentConfiguration enrichmentConfiguration )
    {
        ArgumentNullException.ThrowIfNull ( enrichmentConfiguration ) ;

        return enrichmentConfiguration.With < CallerEnricher > ( ) ;
    }
}
