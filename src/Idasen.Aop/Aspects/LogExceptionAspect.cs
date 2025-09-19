using Castle.DynamicProxy ;
using Idasen.Aop.Interfaces ;
using Serilog ;
using System.Diagnostics.CodeAnalysis;

// ReSharper disable UnusedMember.Global

namespace Idasen.Aop.Aspects ;

/// <summary>
///     Interceptor that executes the invocation and logs any thrown exception along with a formatted
///     invocation description.
/// </summary>
/// <param name="logger">Logger used to write error messages.</param>
/// <param name="converter">Converter that formats the intercepted invocation.</param>
[ExcludeFromCodeCoverage]
public class LogExceptionAspect ( ILogger logger ,
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
            invocation.Proceed ( ) ;
        }
        catch ( Exception exception )
        {
            logger.Error ( $"{converter.Convert ( invocation )} " +
                           exception ) ;
        }
    }
}