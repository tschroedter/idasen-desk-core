﻿using System ;
using System.Collections.Generic ;
using System.Reactive.Concurrency ;
using Idasen.BluetoothLE.Characteristics.Interfaces.Characteristics ;
using Idasen.BluetoothLE.Characteristics.Interfaces.Characteristics.Customs ;
using Idasen.BluetoothLE.Characteristics.Interfaces.Common ;
using Idasen.BluetoothLE.Core ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery ;
using Serilog ;

namespace Idasen.BluetoothLE.Characteristics.Characteristics
{
    public class GenericAttribute
        : CharacteristicBase ,
          IGenericAttribute
    {
        public GenericAttribute (
            ILogger                              logger ,
            IScheduler                           scheduler ,
            IDevice                              device ,
            IGattCharacteristicsProviderFactory  providerFactory ,
            IRawValueReader                      rawValueReader ,
            IRawValueWriter                      rawValueWriter ,
            ICharacteristicBaseToStringConverter toStringConverter ,
            IDescriptionToUuid                   descriptionToUuid ,
            IAllGattCharacteristicsProvider      allGattCharacteristicsProvider )
            : base ( logger ,
                     scheduler ,
                     device ,
                     providerFactory ,
                     rawValueReader ,
                     rawValueWriter ,
                     toStringConverter ,
                     descriptionToUuid )
        {
            Guard.ArgumentNotNull ( allGattCharacteristicsProvider ,
                                    nameof ( allGattCharacteristicsProvider ) ) ;

            _allGattCharacteristicsProvider = allGattCharacteristicsProvider ;
        }

        public IEnumerable < byte > RawServiceChanged => GetValueOrEmpty ( CharacteristicServiceChanged ) ;

        public override Guid GattServiceUuid { get ; } = Guid.Parse ( "00001801-0000-1000-8000-00805f9b34fb" ) ;

        public delegate IGenericAttribute Factory ( IDevice device ) ;

        protected override T WithMapping < T > ( ) where T : class
        {
            if ( _allGattCharacteristicsProvider.TryGetUuid ( CharacteristicServiceChanged ,
                                                              out var uuid ) )
                DescriptionToUuid [ CharacteristicServiceChanged ] = uuid ;

            return this as T ?? throw new Exception ( $"Can't cast {this} to {typeof ( T )}" ) ;
        }

        internal const string CharacteristicServiceChanged = "Service Changed" ;

        private readonly IAllGattCharacteristicsProvider _allGattCharacteristicsProvider ;
    }
}