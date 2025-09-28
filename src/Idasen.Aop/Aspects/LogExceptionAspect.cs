

// ReSharper disable UnusedMember.Global

namespace Idasen.Aop.Aspects ;

using System.Diagnostics.CodeAnalysis ;
using Castle.DynamicProxy ;
using Interfaces ;
using Serilog ;

/// <summary>
///     Interceptor that executes the invocation and logs any thrown exception along with a formatted
///     invocation description.
/// </summary>
/// <param name="logger">Logger used to write error messages.</param>
/// <param name="converter">Converter that formats the intercepted invocation.</param>
[ ExcludeFromCodeCoverage ]
public sealed class LogExceptionAspect ( ILogger logger ,
                                         IInvocationToTextConverter converter )
    : IInterceptor
{
    /// <summary>
    ///     Proceeds with the invocation and logs an error if an exception is thrown.
    /// </summary>
    /// <param name="invocation">The intercepted method invocation.</param>
    public void Intercept ( IInvocation invocation )
    {
        try
        {
#pragma warning disable CA1062
            invocation.Proceed ( ) ;
#pragma warning restore CA1062
        }
#pragma warning disable CA1031
        catch ( Exception exception )
#pragma warning restore CA1031
        {
            logger.Error ( exception ,
                           "{Invocation}" ,
                           converter.Convert ( invocation ) ) ;
        }
    }
}
