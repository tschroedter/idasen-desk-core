using FluentAssertions ;
using Serilog.Core ;

namespace Idasen.BluetoothLE.Linak.Tests ;

[ TestClass ]
public class RawValueToHeightAndSpeedConverterTests
{
    private readonly byte [ ] _invalidHeightAndSpeed = [1 , 0 , 2] ;
    private readonly byte [ ] _validHeightAndSpeed   = [1 , 0 , 2 , 0] ;

    private static RawValueToHeightAndSpeedConverter CreateSut ( )
    {
        return new RawValueToHeightAndSpeedConverter ( Logger.None ) ;
    }

    [ TestMethod ]
    public void TryConvert_ForValidBytes_ReturnsTrue ( )
    {
        var sut = CreateSut ( ) ;

        sut.TryConvert ( _validHeightAndSpeed ,
                         out _ ,
                         out _ )
           .Should ( )
           .BeTrue ( ) ;
    }

    [ TestMethod ]
    public void TryConvert_ForValidBytes_ReturnsHeight ( )
    {
        var sut = CreateSut ( ) ;
        var expected = RawValueToHeightAndSpeedConverter.HeightBaseInMicroMeter + 1u ;

        sut.TryConvert ( _validHeightAndSpeed ,
                         out var height ,
                         out _ ) ;

        height.Should ( )
              .Be ( expected ) ;
    }

    [ TestMethod ]
    public void TryConvert_ForValidBytes_ReturnsSpeed ( )
    {
        var sut = CreateSut ( ) ;

        sut.TryConvert ( _validHeightAndSpeed ,
                         out _ ,
                         out var speed ) ;

        speed.Should ( )
             .Be ( 2 ) ;
    }

    [ TestMethod ]
    public void TryConvert_ForInvalidBytes_ReturnsFalse ( )
    {
        var sut = CreateSut ( ) ;

        sut.TryConvert ( _invalidHeightAndSpeed ,
                         out _ ,
                         out _ )
           .Should ( )
           .BeFalse ( ) ;
    }
}
