using Autofac.Extras.DynamicProxy ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Core ;
using Idasen.BluetoothLE.Linak.Interfaces ;

namespace Idasen.BluetoothLE.Linak.Control
{
    [ Intercept ( typeof ( LogAspect ) ) ]
    public class DeskLockerFactory
        : IDeskLockerFactory
    {
        public DeskLockerFactory ( DeskLocker.Factory factory )
        {
            Guard.ArgumentNotNull ( factory ,
                                    nameof ( factory ) ) ;

            _factory = factory ;
        }

        public IDeskLocker Create ( IDeskMover           deskMover ,
                                    IDeskCommandExecutor executor ,
                                    IDeskHeightAndSpeed  heightAndSpeed )
        {
            Guard.ArgumentNotNull ( deskMover ,
                                    nameof ( deskMover ) ) ;
            Guard.ArgumentNotNull ( executor ,
                                    nameof ( executor ) ) ;
            Guard.ArgumentNotNull ( heightAndSpeed ,
                                    nameof ( heightAndSpeed ) ) ;

            return _factory ( deskMover ,
                              executor ,
                              heightAndSpeed ) ;
        }

        private readonly DeskLocker.Factory _factory ;
    }
}