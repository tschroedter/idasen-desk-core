using Autofac.Extras.DynamicProxy ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Characteristics.Interfaces.Characteristics ;
using Idasen.BluetoothLE.Core ;
using Idasen.BluetoothLE.Linak.Interfaces ;

namespace Idasen.BluetoothLE.Linak.Control
{
    [ Intercept ( typeof ( LogAspect ) ) ]
    public class DeskCommandExecutorFactory
        : IDeskCommandExecutorFactory
    {
        public DeskCommandExecutorFactory ( DeskCommandExecutor.Factory factory )
        {
            Guard.ArgumentNotNull ( factory ,
                                    nameof ( factory ) ) ;

            _factory = factory ;
        }

        public IDeskCommandExecutor Create ( IControl control )
        {
            Guard.ArgumentNotNull ( control ,
                                    nameof ( control ) ) ;

            return _factory ( control ) ;
        }

        private readonly DeskCommandExecutor.Factory _factory ;
    }
}