﻿using System.Reactive.Concurrency ;
using System.Reactive.Linq ;
using Autofac.Extras.DynamicProxy ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Core ;
using Idasen.BluetoothLE.Linak.Interfaces ;
using Serilog ;

namespace Idasen.BluetoothLE.Linak.Control
{
    [ Intercept ( typeof ( LogAspect ) ) ]
    public class DeskMovementMonitor
        : IDeskMovementMonitor
    {
        public DeskMovementMonitor ( ILogger             logger ,
                                     IScheduler          scheduler ,
                                     IDeskHeightAndSpeed heightAndSpeed )
        {
            Guard.ArgumentNotNull ( scheduler ,
                                    nameof ( scheduler ) ) ;
            Guard.ArgumentNotNull ( heightAndSpeed ,
                                    nameof ( heightAndSpeed ) ) ;
            Guard.ArgumentNotNull ( logger ,
                                    nameof ( logger ) ) ;

            _logger         = logger ;
            _scheduler      = scheduler ;
            _heightAndSpeed = heightAndSpeed ;
        }

        /// <inheritdoc />
        public void Dispose ( )
        {
            _disposalHeightAndSpeed?.Dispose ( ) ;
        }

        public void Initialize ( int capacity = DefaultCapacity )
        {
            History = new CircularBuffer < HeightSpeedDetails > ( capacity ) ;

            _disposalHeightAndSpeed = _heightAndSpeed.HeightAndSpeedChanged
                                                     .ObserveOn ( _scheduler )
                                                     .Subscribe ( OnHeightAndSpeedChanged ) ;
        }

        public delegate IDeskMovementMonitor Factory ( IDeskHeightAndSpeed heightAndSpeed ) ;

        private void OnHeightAndSpeedChanged ( HeightSpeedDetails details )
        {
            History.PushBack ( details ) ;

            _logger.Debug ( $"History: {string.Join ( ',' , History )}" ) ;

            if ( History.Size < History.Capacity )
                return ;

            var height        = History [ 0 ].Height ;
            var allSameHeight = History.All ( x => x.Height == height ) ;

            if ( allSameHeight )
                throw new ApplicationException ( HeightDidNotChange ) ;

            _logger.Debug ( "Good, height changed" ) ;

            if ( History.Count ( ) >= MinimumNumberOfItems &&
                 History.All ( x => x.Speed == 0 ) )
                throw new ApplicationException ( SpeedWasZero ) ;

            _logger.Debug ( "Good, speed changed" ) ;
        }

        internal const int MinimumNumberOfItems = 3 ;

        internal const int    DefaultCapacity    = 5 ;
        internal const string HeightDidNotChange = "Height didn't change when moving desk" ;
        internal const string SpeedWasZero       = "Speed was zero when moving desk" ;

        private readonly IDeskHeightAndSpeed _heightAndSpeed ;
        private readonly ILogger             _logger ;
        private readonly IScheduler          _scheduler ;

        private IDisposable ? _disposalHeightAndSpeed ;

        internal CircularBuffer < HeightSpeedDetails > History = // todo interface and test
            new(5) ;
    }
}