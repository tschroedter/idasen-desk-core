using System.Collections ;
using System.Reflection ;
using System.Text ;
using Castle.DynamicProxy ;
using Idasen.Aop.Interfaces ;
using JetBrains.Annotations ;

namespace Idasen.Aop ;

/// <summary>
///     Default implementation of <see cref="IInvocationToTextConverter" /> that renders
///     target type, method name and a list of parameter names (with modifiers and values using ToString).
/// </summary>
public sealed class InvocationToTextConverter : IInvocationToTextConverter
{
    /// <summary>
    ///     Converts the specified invocation to a formatted string.
    /// </summary>
    /// <param name="invocation">The intercepted method invocation.</param>
    /// <returns>A string describing the invocation.</returns>
    public string Convert ( IInvocation invocation )
    {
        ArgumentNullException.ThrowIfNull ( invocation ) ;

        // Prefer the declaring type to avoid Castle proxy type names (e.g., SampleProxy)
        var type = invocation.Method.DeclaringType ??
                   invocation.TargetType ??
                   invocation.Proxy?.GetType ( ).BaseType ;

        var typeName = type?.FullName ?? "UnknownType" ;

        var arguments = ConvertArgumentsToString ( invocation ) ;

        return $"{typeName}.{invocation.Method.Name}({arguments})" ;
    }

    /// <summary>
    ///     Converts an array of arguments to a comma-separated string.
    ///     Kept for backward compatibility and test usage. Since names are unknown, uses arg0=value0,arg1=value1,...
    /// </summary>
    /// <param name="arguments">The method arguments.</param>
    /// <returns>Formatted argument list.</returns>
    [ UsedImplicitly ]
    internal static string ConvertArgumentsToString ( object [ ] arguments )
    {
        if ( arguments.Length == 0 )
            return string.Empty ;

        var builder = new StringBuilder ( ) ;

        for ( var i = 0 ; i < arguments.Length ; i ++ )
        {
            _ = builder.Append ( $"arg{i}=" ) ;
            _ = builder.Append ( ToValueString ( arguments [ i ] ) ) ;

            if ( i < arguments.Length - 1 )
                _ = builder.Append ( ',' ) ;
        }

        return builder.ToString ( ) ;
    }

    /// <summary>
    ///     Converts an invocation's arguments to a comma-separated string including parameter names (and modifiers) with
    ///     values.
    /// </summary>
    private static string ConvertArgumentsToString ( IInvocation invocation )
    {
        var args = invocation.Arguments ;

        if ( args.Length == 0 )
            return string.Empty ;

        var parameters = invocation.Method.GetParameters ( ) ;
        var builder    = new StringBuilder ( ) ;

        for ( var i = 0 ; i < args.Length ; i ++ )
        {
            var name = i < parameters.Length
                           ? parameters [ i ].Name
                           : $"arg{i}" ;
            var p = i < parameters.Length
                        ? parameters [ i ]
                        : null ;
            var modifier = GetParamModifier ( p ) ;

            _ = builder.Append ( modifier ) ;
            _ = builder.Append ( name ) ;
            _ = builder.Append ( '=' ) ;
            _ = builder.Append ( ToValueString ( args [ i ] ) ) ;

            if ( i < args.Length - 1 )
                _ = builder.Append ( ',' ) ;
        }

        return builder.ToString ( ) ;
    }

    private static string GetParamModifier ( ParameterInfo ? p )
    {
        if ( p == null )
            return string.Empty ;

        if ( p.IsOut )
            return "out " ;

        // IsByRef is true for ref/out
        return p.ParameterType.IsByRef
                   ? "ref "
                   : p.IsDefined ( typeof ( ParamArrayAttribute ) ,
                                   false )
                       ? "params "
                       : string.Empty ;
    }

    private static string ToValueString ( object ? value )
    {
        if ( value is null )
            return "null" ;

        // Mask sensitive data based on type or name
        if ( value is string strValue &&
             IsSensitiveData ( strValue ) )
            return "[REDACTED]" ;

        // Arrays -> length in brackets, e.g., [100]
        if ( value is Array array )
        {
            if ( array.Length > 1000 ) // Safeguard for large arrays
                return "[Array too large to process]" ;

            return $"[{array.Length}]" ;
        }

        // Non-generic IDictionary -> number of keys in brackets
        if ( value is IDictionary dict )
        {
            if ( dict.Count > 1000 ) // Safeguard for large dictionaries
                return "[Dictionary too large to process]" ;

            return $"[{dict.Count}]" ;
        }

        // Generic IDictionary<,> or IReadOnlyDictionary<,> -> use Count
        var genericDictCount = TryGetGenericDictionaryCount ( value ) ;

        if ( genericDictCount.HasValue )
        {
            if ( genericDictCount.Value > 1000 ) // Safeguard for large generic dictionaries
                return "[Generic Dictionary too large to process]" ;

            return $"[{genericDictCount.Value}]" ;
        }

        // Other IEnumerable -> log count of items if possible
        if ( value is IEnumerable enumerable &&
             value is not string )
        {
            var count = enumerable.Cast < object > ( ).Take ( 1000 ).Count ( ) ; // Limit to 1000 items for performance
            return $"enumerable[{count}]" ;
        }

        // Fallback: plain ToString
        return value.ToString ( ) ?? value.GetType ( ).Name ;
    }

    private static bool IsSensitiveData ( string value )
    {
        // Example: Check for sensitive keywords
        var sensitiveKeywords = new [ ]
                                {
                                    "password" ,
                                    "token" ,
                                    "secret" ,
                                    "key" ,
                                    "address" ,
                                    "mac"
                                } ;

        // Allow configuration to exclude certain parameters
        if ( value.StartsWith ( "exclude:" ,
                                StringComparison.OrdinalIgnoreCase ) )
            return false ;

        return sensitiveKeywords.Any ( keyword => value.Contains ( keyword ,
                                                                   StringComparison.OrdinalIgnoreCase ) ) ;
    }

    private static int ? TryGetGenericDictionaryCount ( object ? value )
    {
        if ( value == null )
            return null ;

        var t = value.GetType ( ) ;

        foreach ( var i in t.GetInterfaces ( ) )
        {
            if ( ! i.IsGenericType )
                continue ;

            var def = i.GetGenericTypeDefinition ( ) ;

            if ( def != typeof ( IDictionary < , > ) &&
                 def != typeof ( IReadOnlyDictionary < , > ) )
                continue ;

            // Validate the type before accessing properties
            if ( ! typeof ( IEnumerable ).IsAssignableFrom ( i ) )
                continue ;

            // Prefer the Count property on the interface, else try on the concrete type
            var prop = i.GetProperty ( "Count" ) ?? t.GetProperty ( "Count" ) ;

            if ( prop              == null ||
                 prop.PropertyType != typeof ( int ) )
                continue ;

            var v = prop.GetValue ( value ) ;

            if ( v is int c )
                return c ;
        }

        return null ;
    }
}
