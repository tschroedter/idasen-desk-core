using Autofac.Extras.DynamicProxy ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Core ;
using Idasen.BluetoothLE.Linak.Interfaces ;

namespace Idasen.BluetoothLE.Linak.Control ;

[ Intercept ( typeof ( LogAspect ) ) ]
public class DeskMovementMonitorFactory
    : IDeskMovementMonitorFactory
{
    private readonly DeskMovementMonitor.Factory _factory ;

    public DeskMovementMonitorFactory ( DeskMovementMonitor.Factory factory )
    {
        Guard.ArgumentNotNull ( factory ,
                                nameof ( factory ) ) ;

        _factory = factory ;
    }

    public IDeskMovementMonitor Create ( IDeskHeightAndSpeed heightAndSpeed )
    {
        Guard.ArgumentNotNull ( heightAndSpeed ,
                                nameof ( heightAndSpeed ) ) ;

        return _factory ( heightAndSpeed ) ;
    }
}