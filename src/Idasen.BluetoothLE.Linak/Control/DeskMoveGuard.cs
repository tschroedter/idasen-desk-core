using System.Reactive.Linq ;
using System.Reactive.Subjects ;
using Idasen.BluetoothLE.Linak.Interfaces ;
using Serilog ;

namespace Idasen.BluetoothLE.Linak.Control ;

public class DeskMoveGuard
    : IDeskMoveGuard
{
    private readonly IStoppingHeightCalculator _calculator ;
    private readonly IDeskHeightAndSpeed       _heightAndSpeed ;
    private readonly ILogger                   _logger ;
    private readonly Subject < uint >          _targetHeightReached = new( ) ;
    private          Direction                 _direction ;
    private          bool                      _isGuarding ;
    private          IDisposable ?             _subscription ;
    private          uint                      _targetHeight ;

    public DeskMoveGuard ( ILogger                   logger ,
                           IDeskHeightAndSpeed       heightAndSpeed ,
                           IStoppingHeightCalculator calculator )
    {
        ArgumentNullException.ThrowIfNull ( logger ) ;
        ArgumentNullException.ThrowIfNull ( heightAndSpeed ) ;
        ArgumentNullException.ThrowIfNull ( calculator ) ;

        _logger         = logger ;
        _heightAndSpeed = heightAndSpeed ;
        _calculator     = calculator ;
    }

    public IObservable < uint > TargetHeightReached => _targetHeightReached.AsObservable ( ) ;

    public void Dispose ( )
    {
        Dispose ( true ) ;

        GC.SuppressFinalize ( this ) ;
    }

    public void StartGuarding ( Direction         direction ,
                                uint              targetHeight ,
                                CancellationToken none )
    {
        StopGuarding ( ) ;

        _direction    = direction ;
        _targetHeight = targetHeight ;
        _isGuarding   = true ;

        _subscription = _heightAndSpeed.HeightAndSpeedChanged.Subscribe ( DoGuarding ) ;
    }

    public void StopGuarding ( )
    {
        _isGuarding = false ;
        _subscription?.Dispose ( ) ;
        _subscription = null ;
    }

    protected virtual void Dispose ( bool disposing )
    {
        if ( disposing )
        {
            StopGuarding ( ) ;

            _targetHeightReached.OnCompleted ( ) ;
            _targetHeightReached.Dispose ( ) ;
        }
    }

    ~DeskMoveGuard ( )
    {
        Dispose ( false ) ;
    }

    private void DoGuarding ( HeightSpeedDetails details )
    {
        if ( ! _isGuarding )
            return ;

        _calculator.Height                   = details.Height ;
        _calculator.Speed                    = details.Speed ;
        _calculator.TargetHeight             = _targetHeight ;
        _calculator.StartMovingIntoDirection = _direction ;
        _calculator.Calculate ( ) ;

        var currentHeight           = details.Height ;
        var estimatedStoppingHeight = _calculator.StoppingHeight ;

        if ( _direction                 == Direction.Up &&
             _calculator.StoppingHeight >= _targetHeight )
        {
            _logger.Debug ( "Stop Up Movement- current height {Current} (~{Stopping}) >= target {Target}" ,
                            currentHeight ,
                            estimatedStoppingHeight ,
                            _targetHeight ) ;

            _targetHeightReached.OnNext ( estimatedStoppingHeight ) ;

            StopGuarding ( ) ;
        }
        else if ( _direction              == Direction.Down &&
                  estimatedStoppingHeight <= _targetHeight )
        {
            _logger.Debug ( "Stop Down Movement - current height {Current} (~{Stopping}) >= target {Target}" ,
                            currentHeight ,
                            estimatedStoppingHeight ,
                            _targetHeight ) ;

            _targetHeightReached.OnNext ( estimatedStoppingHeight ) ;

            StopGuarding ( ) ;
        }

        if ( _direction == Direction.Up )
            _logger.Debug ( "Allow movement {Direction} - current height {Current} (~{Stopping}) >= target {Target}" ,
                            _direction ,
                            currentHeight ,
                            estimatedStoppingHeight ,
                            _targetHeight ) ;

        else if ( _direction == Direction.Down )
            _logger.Debug ( "Allow movement {Direction} - current height {Current} > target {Target}" ,
                            _direction ,
                            estimatedStoppingHeight ,
                            _targetHeight ) ;
    }
}