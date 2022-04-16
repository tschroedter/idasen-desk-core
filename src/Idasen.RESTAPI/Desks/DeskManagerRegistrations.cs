using System.Collections.Generic ;
using System.Threading.Tasks ;
using Autofac ;
using Autofac.Core ;
using Idasen.Launcher ;
using Idasen.RESTAPI.Interfaces ;
using JetBrains.Annotations ;

namespace Idasen.RESTAPI.Desks
{
    public static class DeskManagerRegistrations
    {
        [ UsedImplicitly ] public static Task < bool > DeskManager ;

        public static IDeskManager CreateFakeDeskManager ( )
        {
            return new FakeDeskManager ( ) ;
        }

        public static IDeskManager CreateRealDeskManager ( )
        {
            IEnumerable < IModule > otherModules = new List < IModule > { new IdasenRESTAPIModule ( ) } ;

            var container = ContainerProvider.Create ( "Idasen.Desk" ,
                                                       "idasen-desk.log",
                                                       otherModules) ;

            var manager = container.Resolve < IDeskManager > ( ) ;

            while ( ! ( DeskManager is { Result: true } ) )
            {
                DeskManager = Task.Run ( async ( ) => await manager.Initialise ( )
                                                                   .ConfigureAwait ( false ) ) ;
            }

            return manager ;
        }
    }
}