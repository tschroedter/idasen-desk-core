using System ;
using System.Threading.Tasks ;
using Microsoft.AspNetCore.Mvc ;
using Microsoft.AspNetCore.Mvc.Filters ;
using Microsoft.Extensions.Configuration ;
using Microsoft.Extensions.DependencyInjection ;

namespace Idasen.RESTAPI.Filters
{
    [ AttributeUsage ( AttributeTargets.Class | AttributeTargets.Method ) ]
    public class ApiKeyAuthAttribute
        : Attribute ,
          IAsyncActionFilter
    {
        public Task OnActionExecutionAsync ( ActionExecutingContext context ,
                                             ActionExecutionDelegate next )
        {
            var httpContext = context.HttpContext ;

            if ( ! httpContext.Request.Headers.TryGetValue ( ApiKeyHeaderName ,
                                                             out var requestApiKey ) )
            {
                context.Result = new UnauthorizedResult ( ) ;

                return Task.CompletedTask;
            }

            var configuration = httpContext.RequestServices.GetRequiredService < IConfiguration > ( ) ;
            var apiKey        = configuration.GetValue < string > ( ApiKeyHeaderName ) ;

            if ( ! apiKey.Equals ( requestApiKey ) )
            {
                context.Result = new UnauthorizedResult ( ) ;

                return Task.CompletedTask;
            }

            return next ( ) ;
        }

        private const string ApiKeyHeaderName = "x-api-key" ;
    }
}