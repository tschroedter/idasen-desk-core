using System ;
using System.Collections.Generic ;

namespace Idasen.BluetoothLE.Core.ServicesDiscovery.Wrappers
{
    public class GattCharacteristicValueChangedDetails
    {
        public GattCharacteristicValueChangedDetails (
            Guid                 uuid ,
            IEnumerable < byte > value ,
            DateTimeOffset       timestamp )
        {
            Guard.ArgumentNotNull ( value ,
                                    nameof ( value ) ) ;

            Uuid      = uuid ;
            Value     = value ;
            Timestamp = timestamp ;
        }

        public Guid                 Uuid      { get ; }
        public IEnumerable < byte > Value     { get ; }
        public DateTimeOffset       Timestamp { get ; }
    }
}