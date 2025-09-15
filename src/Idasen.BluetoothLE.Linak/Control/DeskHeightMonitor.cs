using Idasen.BluetoothLE.Core ;
using Idasen.BluetoothLE.Linak.Interfaces ;
using Serilog ;

namespace Idasen.BluetoothLE.Linak.Control ;

public class DeskHeightMonitor
    : IDeskHeightMonitor
{
    public const int MinimumNumberOfItems = 5 ;
    private readonly ILogger _logger ;

    private CircularBuffer < ulong > _history = new ( MinimumNumberOfItems ) ;

    public DeskHeightMonitor ( ILogger logger )
    {
        Guard.ArgumentNotNull ( logger ,
                                nameof ( logger ) ) ;

        _logger = logger ;
    }

    public bool IsHeightChanging ( )
    {
        if ( _history.Count ( ) < MinimumNumberOfItems )
        {
            return true ;
        }

        var lastNValues = _history.Skip ( _history.Count ( ) - MinimumNumberOfItems )
                                  .ToArray ( ) ;

        var differentValues = lastNValues.Distinct ( )
                                         .Count ( ) ;

        _logger.Debug ( $"History: {string.Join ( "," , lastNValues )}; DifferentValues = {differentValues}" ) ;

        return differentValues > 1 ;
    }

    public void Reset ( )
    {
        _history = new CircularBuffer < ulong > ( MinimumNumberOfItems ) ;
    }

    public void AddHeight ( uint height )
    {
        _history.PushBack ( height ) ;
    }
}