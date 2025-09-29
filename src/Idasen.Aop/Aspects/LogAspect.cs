using System.Diagnostics.CodeAnalysis ;
using Castle.DynamicProxy ;
using Idasen.Aop.Interfaces ;
using Serilog ;
using Serilog.Events ;

namespace Idasen.Aop.Aspects ;

/// <summary>
///     Interceptor that logs method invocations at <see cref="LogEventLevel.Debug" /> using the provided
///     <see cref="ILogger" /> and <see cref="IInvocationToTextConverter" />.
/// </summary>
/// <param name="logger">Logger used to write debug messages.</param>
/// <param name="converter">Converter that formats the intercepted invocation.</param>
[ ExcludeFromCodeCoverage ]
public sealed class LogAspect ( ILogger                    logger ,
                                IInvocationToTextConverter converter )
    : IInterceptor
{
    private readonly IInvocationToTextConverter _converter =
        converter ?? throw new ArgumentNullException ( nameof ( converter ) ) ;

    private readonly ILogger _logger = logger ?? throw new ArgumentNullException ( nameof ( logger ) ) ;

    /// <summary>
    ///     Logs the invocation at debug level and proceeds with the target invocation.
    /// </summary>
    /// <param name="invocation">The intercepted method invocation.</param>
    public void Intercept ( IInvocation invocation )
    {
        if ( _logger.IsEnabled ( LogEventLevel.Debug ) )
            _logger.Debug ( "[LogAspect] ({HashCode:D10}) {Invocation}" ,
#pragma warning disable CA1062
                            invocation.InvocationTarget.GetHashCode ( ) ,
#pragma warning restore CA1062
                            _converter.Convert ( invocation ) ) ;

        invocation.Proceed ( ) ;
    }
}