using System.Text ;
using System.Text.Json ;
using System.Text.Json.Serialization ;
using Castle.DynamicProxy ;
using Idasen.Aop.Interfaces ;
using JetBrains.Annotations ;
using Serilog ;

namespace Idasen.Aop ;

/// <summary>
///     Default implementation of <see cref="IInvocationToTextConverter" /> that renders
///     target type, method name and a JSON-like list of arguments.
/// </summary>
/// <param name="logger">Logger used to report serialization failures at debug level.</param>
public sealed class InvocationToTextConverter ( ILogger logger ) : IInvocationToTextConverter
{
    private static readonly JsonSerializerOptions SafeLogJsonOptions = new ( )
    {
        ReferenceHandler       = ReferenceHandler.IgnoreCycles ,
        MaxDepth               = 16 ,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull ,
        WriteIndented          = false
    } ;

    private readonly ILogger _logger = logger ?? throw new ArgumentNullException ( nameof ( logger ) ) ;

    /// <summary>
    ///     Converts the specified invocation to a formatted string.
    /// </summary>
    /// <param name="invocation">The intercepted method invocation.</param>
    /// <returns>A string describing the invocation.</returns>
    public string Convert ( IInvocation invocation )
    {
        ArgumentNullException.ThrowIfNull ( invocation ) ;

        var targetTypeName = invocation.TargetType?.FullName ??
                             invocation.Method.DeclaringType?.FullName ??
                             "UnknownType" ;

        var arguments = ConvertArgumentsToString ( invocation.Arguments ) ;

        return $"{targetTypeName}.{invocation.Method.Name}({arguments})" ;
    }

    /// <summary>
    ///     Converts an array of arguments to a comma-separated string.
    /// </summary>
    /// <param name="arguments">The method arguments.</param>
    /// <returns>Formatted argument list.</returns>
    [ UsedImplicitly ]
    internal string ConvertArgumentsToString ( object [ ] arguments )
    {
        if ( arguments.Length == 0 )
        {
            return string.Empty ;
        }

        var builder = new StringBuilder ( ) ;

        for ( var i = 0 ; i < arguments.Length ; i++ )
        {
            builder.Append ( DumpObject ( arguments [ i ] ) ) ;

            if ( i < arguments.Length - 1 )
            {
                builder.Append ( "," ) ;
            }
        }

        return builder.ToString ( ) ;
    }

    private string DumpObject ( object? argument )
    {
        if ( argument is null )
        {
            return "null" ;
        }

        try
        {
            switch ( argument )
            {
                case string s:                        return JsonSerializer.Serialize ( s , SafeLogJsonOptions ) ;
                case bool b:                          return b ? "true" : "false" ;
                case int i:                           return i.ToString ( System.Globalization.CultureInfo.InvariantCulture ) ;
                case uint ui:                         return ui.ToString ( System.Globalization.CultureInfo.InvariantCulture ) ;
                case float f:                         return f.ToString ( "R" , System.Globalization.CultureInfo.InvariantCulture ) ;
                case CancellationToken:               return "CancellationToken" ;
                case nint:                            return "IntPtr" ;
                case Task:                            return "Task" ;
                case Stream:                          return "Stream" ;
                case Exception ex:                    return $"{ex.GetType ( ).FullName}: {ex.Message}" ;
                case Type t:                          return $"typeof({t.FullName ?? t.Name})" ;
            }

            if ( IsWindowsBluetoothInstance ( argument ) )
            {
                return argument.ToString ( ) ?? "null" ;
            }

            if ( argument is IProxyTargetAccessor )
            {
                return $"{argument.GetType ( ).Name}[proxy]" ;
            }

            return IsIdasenType ( argument )
                       ? JsonSerializer.Serialize ( argument , SafeLogJsonOptions )
                       : argument.GetType ( ).FullName ?? argument.GetType ( ).Name ;
        }
        catch ( Exception ex )
        {
            _logger.Debug ( "Failed to convert object '{Type}' to JSON - Message: '{Message}'" ,
                            argument.GetType ( ).FullName ,
                            ex.Message ) ;

            try
            {
                return argument.ToString ( ) ?? argument.GetType ( ).Name ;
            }
            catch
            {
                return argument.GetType ( ).Name ;
            }
        }
    }

    private static bool IsWindowsBluetoothInstance ( object argument )
    {
        return argument.GetType ( )
                       .Namespace?
                       .StartsWith ( "Windows.Devices.Bluetooth" ) ==
               true ;
    }

    private static bool IsIdasenType ( object argument )
    {
        var ns = argument.GetType ( ).Namespace ;

        return ns is "Idasen" || ns?.StartsWith ( "Idasen." , System.StringComparison.Ordinal ) == true ;
    }
}