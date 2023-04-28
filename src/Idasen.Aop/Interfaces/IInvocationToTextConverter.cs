using Castle.DynamicProxy ;

namespace Idasen.Aop.Interfaces
{
    public interface IInvocationToTextConverter
    {
        string Convert ( IInvocation invocation ) ;
    }
}