namespace Idasen.BluetoothLE.Linak ;

using Aop.Aspects ;
using Autofac.Extras.DynamicProxy ;
using Characteristics.Interfaces.Characteristics ;
using Interfaces ;

/// <inheritdoc />
[ Intercept ( typeof ( LogAspect ) ) ]
public class DeskHeightAndSpeedFactory
    : IDeskHeightAndSpeedFactory
{
    private readonly DeskHeightAndSpeed.Factory _factory ;

    public DeskHeightAndSpeedFactory ( DeskHeightAndSpeed.Factory factory )
    {
        ArgumentNullException.ThrowIfNull ( factory ) ;

        _factory = factory ;
    }

    public IDeskHeightAndSpeed Create ( IReferenceOutput referenceOutput )
    {
        ArgumentNullException.ThrowIfNull ( referenceOutput ) ;

        return _factory ( referenceOutput ) ;
    }
}
