using Castle.DynamicProxy ;

namespace Idasen.Aop.Interfaces ;

/// <summary>
///     Converts a Castle DynamicProxy <see cref="IInvocation" /> into a human-readable string representation.
/// </summary>
public interface IInvocationToTextConverter
{
    /// <summary>
    ///     Converts the specified invocation into a readable string.
    /// </summary>
    /// <param name="invocation">The intercepted method invocation.</param>
    /// <returns>A string describing the invocation.</returns>
    string Convert ( IInvocation invocation ) ;
}
