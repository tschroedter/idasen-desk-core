namespace Idasen.BluetoothLE.Characteristics.Characteristics.Unknowns ;

using Interfaces.Characteristics ;

public class GenericAttribute
    : UnknownBase , IGenericAttribute
{
    public IEnumerable < byte > RawServiceChanged { get ; } = RawArrayEmpty ;
}
