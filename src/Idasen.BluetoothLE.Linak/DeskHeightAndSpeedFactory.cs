using Autofac.Extras.DynamicProxy ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Characteristics.Interfaces.Characteristics ;
using Idasen.BluetoothLE.Linak.Interfaces ;

namespace Idasen.BluetoothLE.Linak ;

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