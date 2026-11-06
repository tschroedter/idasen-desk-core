using System.Reflection ;
using FluentAssertions ;
using Serilog ;
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
        var sut      = CreateSut ( ) ;
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
    public void TryConvert_ForEnumerableBytes_ReturnsHeightAndSignedSpeed ( )
    {
        var sut = CreateSut ( ) ;
        IEnumerable < byte > bytes = new List < byte > { 0x34 , 0x12 , 0xFE , 0xFF , 0xAB } ;

        sut.TryConvert ( bytes ,
                         out var height ,
                         out var speed )
           .Should ( )
           .BeTrue ( ) ;

        height.Should ( )
              .Be ( RawValueToHeightAndSpeedConverter.HeightBaseInMicroMeter + 0x1234u ) ;
        speed.Should ( )
             .Be ( -2 ) ;
    }

    [ TestMethod ]
    public void TryConvert_ForShortEnumerable_ReturnsFalseAndZeroOutputs ( )
    {
        var sut = CreateSut ( ) ;
        IEnumerable < byte > bytes = new List < byte > { 1 , 0 , 2 } ;

        sut.TryConvert ( bytes ,
                         out var height ,
                         out var speed )
           .Should ( )
           .BeFalse ( ) ;

        height.Should ( )
              .Be ( 0 ) ;
        speed.Should ( )
             .Be ( 0 ) ;
    }

    [ TestMethod ]
    public void TryConvert_ForWarningLoggerFailure_ReturnsFalseAndZeroOutputs ( )
    {
        var logger = DispatchProxy.Create < ILogger , ThrowOnFirstWriteLogger > ( ) ;
        var sut = new RawValueToHeightAndSpeedConverter ( logger ) ;

        sut.TryConvert ( _invalidHeightAndSpeed ,
                         out var height ,
                         out var speed )
           .Should ( )
           .BeFalse ( ) ;

        height.Should ( )
              .Be ( 0 ) ;
        speed.Should ( )
             .Be ( 0 ) ;
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

    public class ThrowOnFirstWriteLogger : DispatchProxy
    {
        private bool _hasThrown ;

        protected override object? Invoke ( MethodInfo? targetMethod ,
                                            object? [ ]? args )
        {
            if ( targetMethod?.Name == nameof ( ILogger.IsEnabled ) )
                return true ;

            if ( ( targetMethod?.Name == "Warning" ||
                   targetMethod?.Name == nameof ( ILogger.Write ) ) && !_hasThrown )
            {
                _hasThrown = true ;
                throw new InvalidOperationException ( ) ;
            }

            return null ;
        }
    }
}
