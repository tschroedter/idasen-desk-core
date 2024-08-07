﻿using System.Diagnostics.CodeAnalysis ;
using Idasen.BluetoothLE.Core.Interfaces ;

namespace Idasen.BluetoothLE.Core
{
    /// <inheritdoc />
    [ ExcludeFromCodeCoverage ]
    public class DateTimeOffsetWrapper ( DateTimeOffset dateTimeOffset )
        : IDateTimeOffset
    {
        public DateTimeOffsetWrapper ( )
            : this ( new DateTimeOffset ( ) )
        {
        }

        /// <inheritdoc />
        public IDateTimeOffset Now => new DateTimeOffsetWrapper ( DateTimeOffset.Now ) ;

        /// <inheritdoc />
        public long Ticks => dateTimeOffset.Ticks ;

        /// <inheritdoc />
        public string ToString ( string          format ,
                                 IFormatProvider formatProvider )
        {
            Guard.ArgumentNotNull ( format ,
                                    nameof ( format ) ) ;
            Guard.ArgumentNotNull ( formatProvider ,
                                    nameof ( formatProvider ) ) ;

            return dateTimeOffset.ToString ( format ,
                                              formatProvider ) ;
        }

        /// <inheritdoc />
        public override string ToString ( )
        {
            return dateTimeOffset.ToString ( ) ;
        }
    }
}