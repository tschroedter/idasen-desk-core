namespace Idasen.BluetoothLE.Core.DevicesDiscovery ;

using Aop.Aspects ;
using Autofac.Extras.DynamicProxy ;
using Interfaces.DevicesDiscovery ;

[ Intercept ( typeof ( LogAspect ) ) ]
public class DeviceComparer
    : IDeviceComparer
{
    /// <inheritdoc />
    public bool Equals ( IDevice? deviceA ,
                         IDevice? deviceB )
    {
        if ( deviceA == null ||
             deviceB == null )
        {
            return false ;
        }

        return deviceA.BroadcastTime == deviceB.BroadcastTime &&
               deviceA.Address == deviceB.Address &&
               deviceA.Name == deviceB.Name &&
               deviceA.RawSignalStrengthInDBm == deviceB.RawSignalStrengthInDBm ;
    }

    /// <inheritdoc />
    public bool IsEquivalentTo ( IDevice? deviceA ,
                                 IDevice? deviceB )
    {
        if ( deviceA == null ||
             deviceB == null )
        {
            return false ;
        }

        return deviceA.Address == deviceB.Address ;
    }
}
