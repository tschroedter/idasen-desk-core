namespace Idasen.BluetoothLE.Characteristics.Characteristics.Unknowns ;

using Interfaces.Characteristics ;

public class Control
    : UnknownBase , IControl
{
    public IEnumerable < byte > RawControl2 { get ; } = RawArrayEmpty ;
    public IEnumerable < byte > RawControl3 { get ; } = RawArrayEmpty ;

    public Task < bool > TryWriteRawControl2 ( IEnumerable < byte > bytes ) => Task.FromResult ( false ) ;
}
