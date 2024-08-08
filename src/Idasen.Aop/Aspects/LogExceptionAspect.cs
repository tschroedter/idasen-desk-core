using Castle.DynamicProxy ;
using Idasen.Aop.Interfaces ;
using Serilog ;

// ReSharper disable UnusedMember.Global

namespace Idasen.Aop.Aspects
{
    public class LogExceptionAspect ( ILogger logger ,
                                      IInvocationToTextConverter converter )
        : IInterceptor
    {
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
}