using System.Text ;
using System.Text.Json ;
using Castle.DynamicProxy ;
using Idasen.Aop.Interfaces ;
using JetBrains.Annotations ;
using Serilog ;

namespace Idasen.Aop ;

public class InvocationToTextConverter ( ILogger logger ) : IInvocationToTextConverter
{
    private readonly ILogger _logger = logger ?? throw new ArgumentNullException ( nameof ( logger ) ) ;

    public string Convert ( IInvocation?  invocation )
    {
        if (invocation == null)
            throw new ArgumentNullException(nameof(invocation));

        var arguments = ConvertArgumentsToString ( invocation.Arguments ) ;

        return $"{invocation.TargetType.FullName}.{invocation.Method.Name}({arguments})" ;
    }

    [ UsedImplicitly ]
    internal string ConvertArgumentsToString ( object [ ] arguments )
    {
        var builder = new StringBuilder ( ) ;

        foreach (var argument in arguments)
        {
            builder.Append ( DumpObject ( argument ) ).Append ( "," ) ;
        }

        if ( builder.Length > 0 )
            builder.Length-- ; // Remove the trailing comma

        return builder.ToString ( ) ;
    }

    private string DumpObject ( object? argument )
    {
        if (argument == null)
            return "null";

        try
        {
            if ( IsWindowsBluetoothInstance ( argument ) )
                return argument.ToString ( ) ?? "null" ;

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
        return argument.GetType ( ).Namespace?.StartsWith ( "Windows.Devices.Bluetooth" ) == true ;
    }
}