using Castle.DynamicProxy ;
using Idasen.Aop.Interfaces ;
using Serilog ;
using Serilog.Events ;

namespace Idasen.Aop.Aspects ;

public class LogAspect ( ILogger logger ,
                         IInvocationToTextConverter converter )
    : IInterceptor
{
    private readonly IInvocationToTextConverter _converter = converter ?? throw new ArgumentNullException ( nameof ( converter ) ) ;
    private readonly ILogger _logger = logger ?? throw new ArgumentNullException ( nameof ( logger ) ) ;

    public void Intercept ( IInvocation invocation )
    {
        if ( _logger.IsEnabled ( LogEventLevel.Debug ) )
        {
            var message = $"[LogAspect] ({invocation.InvocationTarget.GetHashCode ( ):D10}) {_converter.Convert ( invocation )}" ;
            _logger.Debug ( message ) ;
        }

        invocation.Proceed ( ) ;
    }
}