using System.Reactive.Concurrency ;
using System.Reactive.Linq ;
using Autofac.Extras.DynamicProxy ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Core ;
using Idasen.BluetoothLE.Linak.Interfaces ;
using Serilog ;

namespace Idasen.BluetoothLE.Linak.Control
{
    [ Intercept ( typeof ( LogAspect ) ) ]
    public class DeskLocker
        : IDeskLocker
    {
        public DeskLocker ( ILogger              logger ,
                            IScheduler           scheduler ,
                            IDeskMover           deskMover ,
                            IDeskCommandExecutor executor ,
                            IDeskHeightAndSpeed  heightAndSpeed )
        {
            Guard.ArgumentNotNull ( logger ,
                                    nameof ( logger ) ) ;
            Guard.ArgumentNotNull ( scheduler ,
                                    nameof ( scheduler ) ) ;
            Guard.ArgumentNotNull ( deskMover ,
                                    nameof ( deskMover ) ) ;
            Guard.ArgumentNotNull ( executor ,
                                    nameof ( executor ) ) ;
            Guard.ArgumentNotNull ( heightAndSpeed ,
                                    nameof ( heightAndSpeed ) ) ;

            _logger         = logger ;
            _scheduler      = scheduler ;
            _deskMover      = deskMover ;
            _executor       = executor ;
            _heightAndSpeed = heightAndSpeed ;
        }

        public IDeskLocker Initialize ( )
        {
            _disposalHeightAndSpeed = _heightAndSpeed.HeightAndSpeedChanged
                                                     .ObserveOn ( _scheduler )
                                                     .Subscribe ( OnHeightAndSpeedChanged ) ;

            return this ;
        }

        public IDeskLocker Lock ( )
        {
            _logger.Information ( "Desk locked!" ) ;

            IsLocked = true ;

            return this ;
        }

        public bool IsLocked { get ; set ; }

        public IDeskLocker Unlock ( )
        {
            _logger.Information ( "Desk unlocked!" ) ;

            IsLocked = false ;

            return this ;
        }

        public void Dispose ( )
        {
            _deskMover.Dispose ( ) ;
            _heightAndSpeed.Dispose ( ) ;
            _disposalHeightAndSpeed?.Dispose ( ) ;
        }

        public delegate IDeskLocker Factory ( IDeskMover           deskMover ,
                                              IDeskCommandExecutor executor ,
                                              IDeskHeightAndSpeed  heightAndSpeed ) ;

        private void OnHeightAndSpeedChanged ( HeightSpeedDetails details )
        {
            if ( ! IsLocked )
                return ;

            if ( _deskMover.IsAllowedToMove )
                return ;

            _logger.Information ( "Manual move detected. Calling 'Stop'!" ) ;

            _logger.Debug ( $"{details}" ) ;

            _executor.Stop ( ) ;
        }

        private readonly IDeskMover           _deskMover ;
        private readonly IDeskCommandExecutor _executor ;
        private readonly IDeskHeightAndSpeed  _heightAndSpeed ;

        private readonly ILogger    _logger ;
        private readonly IScheduler _scheduler ;

        private IDisposable ? _disposalHeightAndSpeed ;
    }
}