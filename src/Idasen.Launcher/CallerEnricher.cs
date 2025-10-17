using System.Diagnostics ;
using System.Diagnostics.CodeAnalysis ;
using Serilog ;
using Serilog.Core ;
using Serilog.Events ;

namespace Idasen.Launcher ;

/// <summary>
///     Serilog enricher that populates the <c>Caller</c> property with the fully-qualified method
///     name of the user-code frame that initiated the log call (skipping Serilog infrastructure frames).
/// </summary>
[ ExcludeFromCodeCoverage ]
public sealed class CallerEnricher : ILogEventEnricher
{
    /// <summary>
    ///     Adds the <c>Caller</c> property to the <paramref name="logEvent" />, identifying the
    ///     originating method in the call stack, or <c>&lt;unknown method&gt;</c> if it cannot be resolved.
    /// </summary>
    /// <param name="logEvent">The log event to enrich.</param>
    /// <param name="propertyFactory">Factory for creating log event properties.</param>
    public void Enrich ( LogEvent                 logEvent ,
                         ILogEventPropertyFactory propertyFactory )
    {
        ArgumentNullException.ThrowIfNull ( logEvent ) ;
        ArgumentNullException.ThrowIfNull ( propertyFactory ) ;

        var skip = 3 ;

        while ( true )
        {
            var stack = new StackFrame ( skip ) ;

            if ( ! stack.HasMethod ( ) )
            {
                logEvent.AddPropertyIfAbsent ( new LogEventProperty ( "Caller" ,
                                                                      new ScalarValue ( "<unknown method>" ) ) ) ;

                return ;
            }

            var method = stack.GetMethod ( ) ;

            if ( method != null &&
                 method.DeclaringType != null &&
                 method.DeclaringType.Assembly != typeof ( Log ).Assembly )
            {
                var caller =
                    $"{method.DeclaringType.FullName}.{method.Name}({string.Join ( ", " , method.GetParameters ( ).Select ( pi => pi.ParameterType.FullName ) )})" ;
                logEvent.AddPropertyIfAbsent ( new LogEventProperty ( "Caller" ,
                                                                      new ScalarValue ( caller ) ) ) ;

                return ;
            }

            skip ++ ;
        }
    }
}
