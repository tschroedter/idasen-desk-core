using System ;
using System.Threading ;
using System.Threading.Tasks ;
using Microsoft.AspNetCore.Hosting ;
using Microsoft.Extensions.Configuration ;
using Serilog ;

namespace Idasen.RESTAPI
{
    public class IdasenRestApi
    {
        public IdasenRestApi ( )
        {
            _canceler = new CancellationTokenSource ( ) ;
        }

        private static Action < WebHostBuilderContext , IConfigurationBuilder > AddAppSettingsJson ( )
        {
            return ( context ,
                     builder ) =>
                   {
                       builder.AddJsonFile ( "appsettings.json" ,
                                             true ) ;
                   } ;
        }

        public void Start ( )
        {
            Log.Logger.Information("Starting service...");

            _serviceTask = ExecuteAsync ( ) ;

            Log.Logger.Information("...started service.");
        }

        public void Stop ( )
        {
            try
            {
                Log.Logger.Information ( "Stopping service..." ) ;

                _canceler.Cancel();
                _serviceTask.Wait(TimeSpan.FromSeconds(1));

                Log.Logger.Information("...service stopped.");
            }
            catch ( Exception e )
            {
                Log.Logger.Error( e,
                                  "Failed to stop service.");
            }
        }

        public async Task ExecuteAsync ( )
        {
            try
            {
                Log.Logger.Information("Creating WebHost...");
                _host = CreateWebHostBuilder ( ) ;

                Log.Logger.Information("Running WebHost...");
                await _host.RunAsync ( ) ;

                while ( ! _canceler.Token.IsCancellationRequested )
                {
                    Log.Logger.Information ( "Received request to stop service..." ) ;

                    Log.Logger.Information ( "Stopping web server..." ) ;
                    await _host.StopAsync ( TimeSpan.FromSeconds ( 5 ) ) ;

                    Log.Logger.Information ( "...stopped web server." ) ;
                }
            }
            catch ( Exception e )
            {
                Log.Logger.Error ( e, "Failed to stop service!" ) ;
            }
        }

        private IWebHost CreateWebHostBuilder ( )
        {
#pragma warning disable CS0618 // Type or member is obsolete
            IWebHostBuilder builder = new WebHostBuilder ( ).UseKestrel ( )
                                                            .UseUrls ( "http://*:5080" )
                                                            .UseSerilog ( )
                                                            .UseStartup < Startup > ( )
                                                            .ConfigureAppConfiguration ( AddAppSettingsJson ( ) ) ;
#pragma warning restore CS0618 // Type or member is obsolete

            return builder.Build ( );
        }

        private readonly CancellationTokenSource _canceler ;
        private          IWebHost                _host ;
        private          Task                    _serviceTask ;
    }
}