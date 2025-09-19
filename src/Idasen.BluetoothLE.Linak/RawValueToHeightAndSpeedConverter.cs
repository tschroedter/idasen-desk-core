using Autofac.Extras.DynamicProxy ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Characteristics.Common ;
using Idasen.BluetoothLE.Core ;
using Idasen.BluetoothLE.Linak.Interfaces ;
using Serilog ;

namespace Idasen.BluetoothLE.Linak ;

[ Intercept ( typeof ( LogAspect ) ) ]
/// <summary>
///     Converts raw GATT bytes into height and speed values.
/// </summary>
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
        Guard.ArgumentNotNull ( logger ,
                                nameof ( logger ) ) ;

        _logger = logger ;
    }

    /// <inheritdoc />
    public bool TryConvert ( IEnumerable < byte > bytes ,
                             out uint height ,
                             out int speed )
    {
        var array = bytes as byte [ ] ?? bytes.ToArray ( ) ;

        try
        {
            var rawHeight = array.Take ( 2 )
                                 .ToArray ( ) ;

            var rawSpeed = array.Skip ( 2 )
                                .Take ( 2 )
                                .ToArray ( ) ;

            height = HeightBaseInMicroMeter + BitConverter.ToUInt16 ( rawHeight ) ;
            speed = BitConverter.ToInt16 ( rawSpeed ) ;

            return true ;
        }
        catch ( Exception e )
        {
            _logger.Warning ( $"Failed to convert raw value '{array.ToHex ( )}' " +
                              $"to height and speed! ({e.Message})" ) ;

            height = 0 ;
            speed = 0 ;

            return false ;
        }
    }
}