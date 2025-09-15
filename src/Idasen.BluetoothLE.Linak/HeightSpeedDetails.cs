namespace Idasen.BluetoothLE.Linak ;

public class HeightSpeedDetails ( DateTimeOffset timestamp ,
                                  uint height ,
                                  int speed )
{
    public DateTimeOffset Timestamp { get ; } = timestamp ;
    public uint Height { get ; } = height ;
    public int Speed { get ; } = speed ;

    public override string ToString ( )
    {
        return $"Timestamp = {Timestamp:O}, " +
               $"Height = {Height}, " +
               $"Speed = {Speed}" ;
    }
}