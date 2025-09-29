using System.Diagnostics ;
using System.Globalization ;
using Autofac.Extras.DynamicProxy ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Core.Interfaces ;
using Idasen.BluetoothLE.Core.Interfaces.DevicesDiscovery ;
using Selkie.DefCon.One.Common ;

namespace Idasen.BluetoothLE.Core.DevicesDiscovery ;

/// <inheritdoc />
[ DebuggerDisplay ( "{Name} {MacAddress}" ) ]
[ Intercept ( typeof ( LogAspect ) ) ]
public class Device
    : IDevice
{
    public delegate IDevice Factory ( IDateTimeOffset broadcastTime ,
                                      ulong           address ,
                                      string ?        name ,
                                      short           rawSignalStrengthInDBm ) ;

    public Device ( IDevice device )
    {
        Guard.ArgumentNotNull ( device ,
                                nameof ( device ) ) ;

        BroadcastTime          = device.BroadcastTime ;
        Address                = device.Address ;
        MacAddress             = device.MacAddress ;
        Name                   = device.Name ;
        RawSignalStrengthInDBm = device.RawSignalStrengthInDBm ;
    }

    public Device ( IDateTimeOffset          broadcastTime ,
                    ulong                    address ,
                    [ GuardIgnore ] string ? name ,
                    short                    rawSignalStrengthInDBm )
    {
        Guard.ArgumentNotNull ( broadcastTime ,
                                nameof ( broadcastTime ) ) ;

        BroadcastTime          = broadcastTime ;
        Address                = address ;
        MacAddress             = address.ToMacAddress ( ) ;
        Name                   = name ;
        RawSignalStrengthInDBm = rawSignalStrengthInDBm ;
    }

    /// <inheritdoc />
    public IDateTimeOffset BroadcastTime { get ; set ; }

    /// <inheritdoc />
    public ulong Address { get ; }

    /// <inheritdoc />
    public string MacAddress { get ; }

    /// <inheritdoc />
    public string ? Name { get ; set ; }

    /// <inheritdoc />
    public short RawSignalStrengthInDBm { get ; set ; }

    /// <inheritdoc />
    public string Details => ToString ( ) ;

    /// <inheritdoc />
    public override string ToString ( )
    {
        var name = string.IsNullOrWhiteSpace ( Name )
                       ? "[No Name]"
                       : Name ;

        return
            $"Name = {name}, "                                                                   +
            $"MacAddress = {MacAddress}, "                                                       +
            $"Address = {Address}, "                                                             +
            $"BroadcastTime = {BroadcastTime.ToString ( "O" , CultureInfo.InvariantCulture )}, " +
            $"RawSignalStrengthInDBm = {RawSignalStrengthInDBm}dB" ;
    }
}