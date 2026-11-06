using FluentAssertions ;

namespace Idasen.BluetoothLE.Linak.Tests ;

[ TestClass ]
public class HeightSpeedDetailsTests
{
    [ TestMethod ]
    public void Timestamp_ForInvoked_Instance ( )
    {
        var timestamp = DateTimeOffset.UtcNow ;
        var sut = new HeightSpeedDetails ( timestamp , 0u , 0 ) ;

        sut.Timestamp
           .Should ( )
           .Be ( timestamp ) ;
    }

    [ TestMethod ]
    public void Height_ForInvoked_Instance ( )
    {
        var height = 100u ;
        var sut = new HeightSpeedDetails ( DateTimeOffset.UtcNow , height , 0 ) ;

        sut.Height
           .Should ( )
           .Be ( height ) ;
    }

    [ TestMethod ]
    public void Speed_ForInvoked_Instance ( )
    {
        var speed = 5 ;
        var sut = new HeightSpeedDetails ( DateTimeOffset.UtcNow , 0u , speed ) ;

        sut.Speed
           .Should ( )
           .Be ( speed ) ;
    }

    [ TestMethod ]
    public void ToString_ForInvoked_Instance ( )
    {
        var timestamp = DateTimeOffset.UtcNow ;
        var height = 100u ;
        var speed = 5 ;
        var sut = new HeightSpeedDetails ( timestamp , height , speed ) ;

        var expected = $"Timestamp = {timestamp:O}, " +
                       $"Height = {height}, "         +
                       $"Speed = {speed}" ;

        sut.ToString ( )
           .Should ( )
           .Be ( expected ) ;
    }
}
