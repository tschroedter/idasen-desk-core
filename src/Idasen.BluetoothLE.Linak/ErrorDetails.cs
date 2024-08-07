﻿using Idasen.BluetoothLE.Core ;
using Idasen.BluetoothLE.Linak.Interfaces ;

namespace Idasen.BluetoothLE.Linak
{
    public class ErrorDetails // todo testing
        : IErrorDetails
    {
        public ErrorDetails (
            string      message ,
            string      caller ,
            Exception ? exception = null )
        {
            Guard.ArgumentNotNull ( message ,
                                    nameof ( message ) ) ;
            Guard.ArgumentNotNull ( caller ,
                                    nameof ( caller ) ) ;

            Message   = message ;
            Exception = exception ;
            Caller    = caller ;
        }

        public string      Message   { get ; }
        public Exception ? Exception { get ; }
        public string      Caller    { get ; }

        public override string ToString ( )
        {
            return Exception == null
                       ? $"[{Caller}] {Message}"
                       : $"[{Caller}] {Message} ({Exception.Message})" ;
        }
    }
}