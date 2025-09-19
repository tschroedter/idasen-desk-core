using System.Collections ;
using System.Globalization ;
using System.Reflection ;
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
    private const int MaxStringLengthPerArg = 256 ;

    private static readonly JsonSerializerOptions SafeLogJsonOptions = new ( )
    {
        ReferenceHandler = ReferenceHandler.IgnoreCycles ,
        MaxDepth = 4 ,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull ,
        WriteIndented = false
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

        var arguments = ConvertArgumentsToString ( invocation ) ;

        return $"{targetTypeName}.{invocation.Method.Name}({arguments})" ;
    }

    /// <summary>
    ///     Converts an array of arguments to a comma-separated string.
    ///     Kept for backward compatibility and test usage. Does not include parameter names.
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

        for (var i = 0; i < arguments.Length; i++)
        {
            builder.Append ( DumpObject ( arguments[i] ) ) ;

            if ( i < arguments.Length - 1 )
            {
                builder.Append ( "," ) ;
            }
        }

        return builder.ToString ( ) ;
    }

    /// <summary>
    ///     Converts an invocation's arguments to a comma-separated string including parameter names.
    /// </summary>
    private string ConvertArgumentsToString ( IInvocation invocation )
    {
        var args = invocation.Arguments ;

        if ( args.Length == 0 )
        {
            return string.Empty ;
        }

        var parameters = invocation.Method.GetParameters ( ) ;
        var builder = new StringBuilder ( ) ;

        for (var i = 0; i < args.Length; i++)
        {
            var name = i < parameters.Length
                           ? parameters[i].Name
                           : $"arg{i}" ;
            var p = i < parameters.Length
                        ? parameters[i]
                        : null ;
            var modifier = GetParamModifier ( p ) ;

            builder.Append ( modifier ) ;
            builder.Append ( name ) ;
            builder.Append ( "=" ) ;
            builder.Append ( DumpObject ( args[i] ) ) ;

            if ( i < args.Length - 1 )
            {
                builder.Append ( "," ) ;
            }
        }

        return builder.ToString ( ) ;
    }

    private static string GetParamModifier ( ParameterInfo? p )
    {
        if ( p == null )
        {
            return string.Empty ;
        }

        if ( p.IsOut )
        {
            return "out " ;
        }

        // IsByRef is true for ref/out
        if ( p.ParameterType.IsByRef )
        {
            return "ref " ;
        }

        if ( p.IsDefined ( typeof ( ParamArrayAttribute ) ,
                           false ) )
        {
            return "params " ;
        }

        return string.Empty ;
    }

    private string DumpObject ( object? argument )
    {
        if ( argument is null )
        {
            return "null" ;
        }

        try
        {
            switch (argument)
            {
                case string s:
                    return Truncate ( JsonSerializer.Serialize ( s ,
                                                                 SafeLogJsonOptions ) ,
                                      MaxStringLengthPerArg ) ;

                case char c:
                    return JsonSerializer.Serialize ( c ,
                                                      SafeLogJsonOptions ) ;

                case bool b:
                    return b
                               ? "true"
                               : "false" ;

                case byte [ ] bytes:
                    return DumpByteArray ( bytes ) ;

                case CancellationToken ct:
                    return $"CancellationToken(IsCancellationRequested={ct.IsCancellationRequested.ToString ( CultureInfo.InvariantCulture )})" ;

                case Task t:
                    return $"Task(Status={t.Status})" ;

                case IDictionary dictionary:
                    return DumpDictionary ( dictionary ) ;

                case IEnumerable enumerable:
                    // Avoid treating string as IEnumerable here (already handled above)
                    return DumpEnumerable ( enumerable ) ;
            }

            if ( IsWindowsBluetoothInstance ( argument ) )
            {
                // Windows.Devices.Bluetooth WinRT types are complex; avoid deep serialization
                return $"[{argument.GetType ( ).FullName}]" ;
            }

            // For simple primitives and structs, ToString with invariant is fine
            if ( IsSimpleScalar ( argument ) )
            {
                return Truncate ( System.Convert.ToString ( argument ,
                                                            CultureInfo.InvariantCulture ) ??
                                  argument.GetType ( ).Name ,
                                  MaxStringLengthPerArg ) ;
            }

            // Try safe JSON for other objects
            try
            {
                var json = JsonSerializer.Serialize ( argument ,
                                                      SafeLogJsonOptions ) ;
                return Truncate ( json ,
                                  MaxStringLengthPerArg ) ;
            }
            catch ( Exception ex )
            {
                _logger.Debug ( ex ,
                                "Failed to serialize argument of type {Type}" ,
                                argument.GetType ( ).FullName ) ;
                // Do NOT call ToString() on unknown objects to avoid potential recursion/StackOverflow
                return $"[{argument.GetType ( ).FullName}]" ;
            }
        }
        catch ( Exception ex )
        {
            _logger.Debug ( ex ,
                            "Failed to dump argument of type {Type}" ,
                            argument.GetType ( ).FullName ) ;
            return $"[{argument.GetType ( ).FullName}]" ;
        }
    }

    private static bool IsSimpleScalar ( object value )
    {
        var t = value.GetType ( ) ;

        if ( t.IsEnum )
        {
            return true ;
        }

        // Handle common primitives and BCL value types
        return t.IsPrimitive ||
               t == typeof ( decimal ) ||
               t == typeof ( DateTime ) ||
               t == typeof ( DateTimeOffset ) ||
               t == typeof ( TimeSpan ) ||
               t == typeof ( Guid ) ;
    }

    private static string DumpByteArray ( byte [ ] bytes )
    {
        // Only dump length to avoid large logs
        return $"byte[{bytes.Length}]" ;
    }

    private string DumpEnumerable ( IEnumerable enumerable )
    {
        // Only dump the length for safety; avoid invoking arbitrary Count members
        if ( enumerable is Array arr )
        {
            return $"IEnumerable[{arr.Length}]" ;
        }

        if ( enumerable is ICollection nonGenericCollection )
        {
            return $"IEnumerable[{nonGenericCollection.Count}]" ;
        }

        return "IEnumerable[?]" ;
    }

    private string DumpDictionary ( IDictionary dictionary )
    {
        // Only dump the length (Count is available on IDictionary)
        return $"IDictionary[{dictionary.Count}]" ;
    }

    private static string Truncate ( string text , int maxLength )
    {
        if ( text.Length <= maxLength )
        {
            return text ;
        }

        // Keep closing bracket/quote visibility by using an ellipsis suffix
        return text.Substring ( 0 ,
                                Math.Max ( 0 ,
                                           maxLength - 1 ) ) +
               "…" ;
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

        return ns is "Idasen" ||
               ns?.StartsWith ( "Idasen." ,
                                StringComparison.Ordinal ) ==
               true ;
    }
}