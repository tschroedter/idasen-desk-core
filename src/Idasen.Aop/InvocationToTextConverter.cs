using System.Text ;
using System.Text.Json ;
using Castle.DynamicProxy ;
using Idasen.Aop.Interfaces ;
using JetBrains.Annotations ;
using Serilog ;

namespace Idasen.Aop ;

/// <summary>
///     Default implementation of <see cref="IInvocationToTextConverter"/> that renders
///     target type, method name and a JSON-like list of arguments.
/// </summary>
/// <param name="logger">Logger used to report serialization failures at debug level.</param>
public class InvocationToTextConverter ( ILogger logger ) : IInvocationToTextConverter
{
    private readonly ILogger _logger = logger ?? throw new ArgumentNullException ( nameof ( logger ) ) ;

    /// <summary>
    ///     Converts the specified invocation to a formatted string.
    /// </summary>
    /// <param name="invocation">The intercepted method invocation.</param>
    /// <returns>A string describing the invocation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="invocation"/> is null.</exception>
    public string Convert ( IInvocation? invocation )
    {
        if ( invocation == null )
        {
            throw new ArgumentNullException ( nameof ( invocation ) ) ;
        }

        var arguments = ConvertArgumentsToString ( invocation.Arguments ) ;

        return $"{invocation.TargetType.FullName}.{invocation.Method.Name}({arguments})" ;
    }

    /// <summary>
    ///     Converts an array of arguments to a comma-separated string.
    /// </summary>
    /// <param name="arguments">The method arguments.</param>
    /// <returns>Formatted argument list.</returns>
    [ UsedImplicitly ]
    internal string ConvertArgumentsToString ( object [ ] arguments )
    {
        var builder = new StringBuilder ( ) ;

        foreach (var argument in arguments)
        {
            builder.Append ( DumpObject ( argument ) ).Append ( "," ) ;
        }

        if ( builder.Length > 0 )
        {
            builder.Length-- ; // Remove the trailing comma
        }

        return builder.ToString ( ) ;
    }

    private string DumpObject ( object? argument )
    {
        if ( argument == null )
        {
            return "null" ;
        }

        try
        {
            if ( argument is CancellationToken )
            {
                return nameof ( CancellationToken ) ;
            }

            if ( argument is IntPtr )
            {
                return nameof ( IntPtr ) ;
            }

            if ( IsWindowsBluetoothInstance ( argument ) )
            {
                return argument.ToString ( ) ?? "null" ;
            }

            return JsonSerializer.Serialize ( argument ) ;
        }
        catch ( Exception e )
        {
            _logger.Debug ( "Failed to convert object '{Type}' to JSON - Message: '{Message}'" ,
                            argument.GetType ( ).FullName ,
                            e.Message ) ;

            return argument.ToString ( ) ?? "null" ;
        }
    }

    private static bool IsWindowsBluetoothInstance ( object argument )
    {
        return argument.GetType ( )
                       .Namespace?
                       .StartsWith ( "Windows.Devices.Bluetooth" ) ==
               true ;
    }
}