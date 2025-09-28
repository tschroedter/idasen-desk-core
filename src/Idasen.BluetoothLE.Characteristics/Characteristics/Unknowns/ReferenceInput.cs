namespace Idasen.BluetoothLE.Characteristics.Characteristics.Unknowns ;

using Interfaces.Characteristics ;

public class ReferenceInput
    : UnknownBase , IReferenceInput
{
    public IEnumerable < byte > Ctrl1 { get ; } = RawArrayEmpty ;
}
