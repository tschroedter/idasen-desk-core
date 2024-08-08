using Idasen.BluetoothLE.Core ;
using Serilog ;

namespace Idasen.BluetoothLE.Characteristics.Common
{
    public static class ExceptionExtensions
    {
        public static bool IsBluetoothDisabledException ( this Exception e )
        {
            return ( uint )e.HResult == 0x8007048F ||
                   ( uint )e.HResult == 0x800710DF ||
                   ( uint )e.HResult == 0x8000FFFF ;
        }

        public static void LogBluetoothStatusException ( this Exception exception ,
                                                         ILogger        log ,
                                                         string ?       message )
        {
            Guard.ArgumentNotNull ( log ,
                                    nameof ( log ) ) ;

            var text = Constants.CheckAndEnableBluetooth +
                       $" (0x{exception.HResult:X}) "    +
                       message ;

            log.Information ( text ) ;
        }
    }
}