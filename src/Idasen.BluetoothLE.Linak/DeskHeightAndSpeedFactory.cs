using Autofac.Extras.DynamicProxy ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Characteristics.Interfaces.Characteristics ;
using Idasen.BluetoothLE.Core ;
using Idasen.BluetoothLE.Linak.Interfaces ;

namespace Idasen.BluetoothLE.Linak ;

[ Intercept ( typeof ( LogAspect ) ) ]
public class DeskHeightAndSpeedFactory
    : IDeskHeightAndSpeedFactory
{
    private readonly DeskHeightAndSpeed.Factory _factory ;

    public DeskHeightAndSpeedFactory ( DeskHeightAndSpeed.Factory factory )
    {
        Guard.ArgumentNotNull ( factory ,
                                nameof ( factory ) ) ;

        _factory = factory ;
    }

    public IDeskHeightAndSpeed Create ( IReferenceOutput referenceOutput )
    {
        Guard.ArgumentNotNull ( referenceOutput ,
                                nameof ( referenceOutput ) ) ;

        return _factory ( referenceOutput ) ;
    }
}