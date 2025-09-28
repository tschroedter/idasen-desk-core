namespace Idasen.BluetoothLE.Characteristics.Characteristics.Unknowns ;

using Interfaces.Characteristics ;

public class Dpg
    : UnknownBase , IDpg
{
    public IEnumerable < byte > RawDpg { get ; } = RawArrayEmpty ;
}
