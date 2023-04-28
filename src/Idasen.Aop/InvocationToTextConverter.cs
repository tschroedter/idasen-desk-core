using System ;
using System.Linq ;
using System.Text ;
using System.Text.Json ;
using Castle.DynamicProxy ;
using Idasen.Aop.Interfaces ;
using JetBrains.Annotations ;
using Serilog ;

namespace Idasen.Aop
{
    public class InvocationToTextConverter : IInvocationToTextConverter
    {
        public InvocationToTextConverter ( ILogger logger )
        {
            _logger = logger ?? throw new ArgumentNullException ( nameof ( logger ) ) ;
        }

        public string Convert ( IInvocation invocation )
        {
            var arguments = ConvertArgumentsToString ( invocation.Arguments ) ;

            var called = $"{invocation.TargetType.FullName}.{invocation.Method.Name}({arguments})" ;

            return called ;
        }

        [ UsedImplicitly ]
        internal string ConvertArgumentsToString ( object [ ] arguments )
        {
            var builder = new StringBuilder ( ) ;

            foreach ( var argument in arguments )
            {
                var argumentDescription = DumpObject ( argument ) ;

                builder.Append ( argumentDescription )
                       .Append ( "," ) ;
            }

            if ( arguments.Any ( ) ) builder.Length -- ;

            return builder.ToString ( ) ;
        }

        private string DumpObject ( object argument )
        {
            try
            {
                if ( IsWindowsBluetoothInstance ( argument ) )
                    return argument.ToString ( ) ?? "null" ;

                var json = JsonSerializer.Serialize ( argument ) ;

                return json ;
            }
            catch ( Exception e )
            {
                _logger.Debug ( "Failed to convert object "                     +
                                $"'{argument.GetType ( ).FullName}' to json - " +
                                $"Message: '{e.Message}'" ) ;

                return argument.ToString ( ) ?? "null" ;
            }
        }

        private static bool IsWindowsBluetoothInstance ( object argument )
        {
            var ns = argument.GetType ( ).Namespace ;

            return ns != null &&
                   ns.StartsWith ( "Windows.Devices.Bluetooth" ) ;
        }

        private readonly ILogger _logger ;
    }
}