namespace Idasen.BluetoothLE.Characteristics.Common ;

using System.Diagnostics.CodeAnalysis ;
using Windows.Storage.Streams ;
using Aop.Aspects ;
using Autofac.Extras.DynamicProxy ;
using Core ;
using Interfaces.Common ;
using Serilog ;

/// <inheritdoc />
[ ExcludeFromCodeCoverage ]
[ Intercept ( typeof ( LogAspect ) ) ]
public class BufferReader
    : IBufferReader
{
    private readonly ILogger _logger ;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BufferReader" /> class.
    /// </summary>
    /// <param name="logger">Logger used for error reporting.</param>
    public BufferReader ( ILogger logger )
    {
        Guard.ArgumentNotNull ( logger ,
                                nameof ( logger ) ) ;

        _logger = logger ;
    }

    /// <inheritdoc />
    public bool TryReadValue (
        IBuffer buffer ,
        out byte [ ] bytes )
    {
        Guard.ArgumentNotNull ( buffer ,
                                nameof ( buffer ) ) ;

        try
        {
            var reader = DataReader.FromBuffer ( buffer ) ;
            bytes = new byte[reader.UnconsumedBufferLength] ;
            reader.ReadBytes ( bytes ) ;

            return true ;
        }
        catch ( Exception e )
        {
            const string message = "Failed to read from buffer" ;

            _logger.Error ( e ,
                            message ) ;
        }

        bytes = [] ;

        return false ;
    }
}
