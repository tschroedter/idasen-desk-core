using System.Diagnostics.CodeAnalysis ;
using Autofac ;
using Idasen.BluetoothLE.Linak.Interfaces ;
using Idasen.Launcher ;
using JetBrains.Annotations ;
using Microsoft.Extensions.Configuration ;
using Serilog ;
using static System.Console ;

namespace Idasen.ConsoleApp ;

[ ExcludeFromCodeCoverage ]
internal static class Program
{
    private const string DefaultDeviceName              = "Desk" ;
    private const ulong  DefaultDeviceAddress           = 250635178951455u ;
    private const uint   DefaultDeviceMonitoringTimeout = 600u ;

    /// <summary>
    ///     Test Application
    /// </summary>
    [ UsedImplicitly ]
    private static async Task Main ( )
    {
        using var tokenSource = new CancellationTokenSource ( TimeSpan.FromSeconds ( 60 ) ) ;
        var       token       = tokenSource.Token ;

        var builder = new ConfigurationBuilder ( ).SetBasePath ( Directory.GetCurrentDirectory ( ) )
                                                  .AddJsonFile ( "appsettings.json" ) ;

        IContainer ? container = null ;

        try
        {
            container = ContainerProvider.Create ( builder.Build ( ) ) ;

            var logger   = container.Resolve < ILogger > ( ) ;
            var provider = container.Resolve < IDeskProvider > ( ) ;

            provider.Initialize ( DefaultDeviceName ,
                                  DefaultDeviceAddress ,
                                  DefaultDeviceMonitoringTimeout ) ;

            var (isSuccess , desk) = await provider.TryGetDesk ( token ) ;

            if ( isSuccess )
                desk!.MoveTo ( 7200u ) ;
            else
                logger.Error ( "Failed to detect desk" ) ;

            ReadLine ( ) ;

            desk?.Dispose ( ) ;
        }
        catch ( Exception ex )
        {
            WriteLine ( ex ) ;
        }
        finally
        {
            container?.Dispose ( ) ;
            LoggerProvider.Shutdown ( ) ;
        }
    }
}