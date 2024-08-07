﻿using Idasen.BluetoothLE.Characteristics.Common ;
using Idasen.BluetoothLE.Core ;

namespace Idasen.BluetoothLE.Characteristics.Characteristics
{
    public class RawValueChangedDetails
    {
        public RawValueChangedDetails ( string               description ,
                                        IEnumerable < byte > value ,
                                        DateTimeOffset       timestamp ,
                                        Guid                 uuid )
        {
            Guard.ArgumentNotNull ( description ,
                                    nameof ( description ) ) ;
            Guard.ArgumentNotNull ( value ,
                                    nameof ( value ) ) ;

            Value       = value ;
            Timestamp   = timestamp ;
            Uuid        = uuid ;
            Description = description ;
        }

        public string               Description { get ; }
        public IEnumerable < byte > Value       { get ; }
        public DateTimeOffset       Timestamp   { get ; }
        public Guid                 Uuid        { get ; }

        public override string ToString ( )
        {
            return $"Description = {Description}, " +
                   $"Value =  {Value.ToHex ( )}, "  +
                   $"Timestamp = {Timestamp:O}, "   +
                   $"Uuid = {Uuid}" ;
        }
    }
}