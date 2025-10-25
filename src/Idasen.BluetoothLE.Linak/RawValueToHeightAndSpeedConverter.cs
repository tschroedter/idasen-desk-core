using System.Buffers.Binary ;
using Autofac.Extras.DynamicProxy ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Characteristics.Common ;
using Idasen.BluetoothLE.Linak.Interfaces ;
using Serilog ;

namespace Idasen.BluetoothLE.Linak ;

/// <inheritdoc />
[ Intercept ( typeof ( LogAspect ) ) ]
public class RawValueToHeightAndSpeedConverter
    : IRawValueToHeightAndSpeedConverter
{
    // Height of the desk at it's lowest 620 mm and max. is 1270mm.
    internal const uint HeightBaseInMicroMeter = 6200 ; // = 6200 / 10 = 620mm

    private readonly ILogger _logger ;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RawValueToHeightAndSpeedConverter" /> class.
    /// </summary>
    public RawValueToHeightAndSpeedConverter ( ILogger logger )
    {
        ArgumentNullException.ThrowIfNull ( logger ) ;

        _logger = logger ;
    }

    /// <inheritdoc />
    public bool TryConvert ( IEnumerable < byte > bytes ,
                             out uint             height ,
                             out int              speed )
    {
        ArgumentNullException.ThrowIfNull ( bytes ) ;

        height = 0 ;
        speed  = 0 ;

        var enumerable = bytes as byte [ ] ?? bytes.ToArray ( ) ;

        try
        {
            // Fast path if we already have a byte[]
            if ( bytes is byte [ ] arr )
            {
                if ( arr.Length < 4 )
                {
                    _logger.Warning ( "Failed to convert raw value {Hex} to height and speed. Payload too short ({Length})" ,
                                      arr.ToHex ( ) ,
                                      arr.Length ) ;
                    return false ;
                }

                var span = arr.AsSpan ( ) ;
                var rawHeight = BinaryPrimitives.ReadUInt16LittleEndian ( span.Slice ( 0 ,
                                                                                       2 ) ) ;
                var rawSpeed = BinaryPrimitives.ReadInt16LittleEndian ( span.Slice ( 2 ,
                                                                                     2 ) ) ;

                height = HeightBaseInMicroMeter + rawHeight ;
                speed  = rawSpeed ;
                return true ;
            }

            // Fallback: copy first 4 bytes without allocating a full array
            Span < byte > buffer = stackalloc byte [ 4 ] ;
            var           i      = 0 ;

            foreach ( var b in enumerable )
                if ( i < 4 )
                {
                    buffer [ i ++ ] = b ;

                    if ( i == 4 )
                        break ;
                }
                else
                {
                    break ;
                }

            if ( i < 4 )
            {
                // Only allocate for logging on failure
                var hex = enumerable.ToHex ( ) ;
                _logger.Warning ( "Failed to convert raw value {Hex} to height and speed. Payload too short ({Length})" ,
                                  hex ,
                                  i ) ;
                return false ;
            }

            var h = BinaryPrimitives.ReadUInt16LittleEndian ( buffer.Slice ( 0 ,
                                                                             2 ) ) ;
            var s = BinaryPrimitives.ReadInt16LittleEndian ( buffer.Slice ( 2 ,
                                                                            2 ) ) ;

            height = HeightBaseInMicroMeter + h ;
            speed  = s ;
            return true ;
        }
        catch ( Exception e )
        {
            // Allocate only for logging
            var hex = bytes is byte [ ] a
                          ? a.ToHex ( )
                          : enumerable.ToHex ( ) ;
            _logger.Warning ( e ,
                              "Failed to convert raw value {Hex} to height and speed" ,
                              hex ) ;
            height = 0 ;
            speed  = 0 ;
            return false ;
        }
    }
}