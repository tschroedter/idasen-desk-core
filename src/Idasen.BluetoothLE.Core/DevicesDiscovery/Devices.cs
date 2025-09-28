namespace Idasen.BluetoothLE.Core.DevicesDiscovery ;

using Aop.Aspects ;
using Autofac.Extras.DynamicProxy ;
using Interfaces.DevicesDiscovery ;
using Serilog ;

/// <inheritdoc />
[ Intercept ( typeof ( LogAspect ) ) ]
public class Devices
    : IDevices
{
    private readonly Dictionary < ulong , Device > _discoveredDevices = new ( ) ;
    private readonly ILogger _logger ;
    private readonly object _padLock = new ( ) ;

    public Devices ( ILogger logger )
    {
        Guard.ArgumentNotNull ( logger ,
                                nameof ( logger ) ) ;

        _logger = logger ;
    }

    /// <inheritdoc />
    public IReadOnlyCollection < IDevice > DiscoveredDevices
    {
        get
        {
            lock (_padLock)
            {
                return _discoveredDevices.Values
                                         .ToList ( )
                                         .AsReadOnly ( ) ;
            }
        }
    }

    /// <inheritdoc />
    public void AddOrUpdateDevice ( IDevice device )
    {
        Guard.ArgumentNotNull ( device ,
                                nameof ( device ) ) ;

        lock (_padLock)
        {
            if ( _discoveredDevices.TryGetValue ( device.Address ,
                                                  out Device? storedDevice ) )
            {
                UpdateDevice ( device ,
                               storedDevice ) ;
            }
            else
            {
                AddDevice ( device ) ;
            }
        }
    }

    /// <inheritdoc />
    public void RemoveDevice ( IDevice device )
    {
        Guard.ArgumentNotNull ( device ,
                                nameof ( device ) ) ;

        lock (_padLock)
        {
            _discoveredDevices.Remove ( device.Address ) ;

            _logger.Information ( "[{Mac}] Device removed" ,
                                  device.MacAddress ) ;
        }
    }

    /// <inheritdoc />
    public void Clear ( )
    {
        lock (_padLock)
        {
            _discoveredDevices.Clear ( ) ;
        }
    }

    /// <inheritdoc />
    public bool ContainsDevice ( IDevice device )
    {
        Guard.ArgumentNotNull ( device ,
                                nameof ( device ) ) ;

        lock (_padLock)
        {
            return _discoveredDevices.ContainsKey ( device.Address ) ;
        }
    }

    /// <inheritdoc />
    public bool TryGetDevice ( ulong address ,
                               out IDevice? device )
    {
        lock (_padLock)
        {
            var result = _discoveredDevices.TryGetValue ( address ,
                                                          out Device? storedDevice ) ;

            device = storedDevice ;

            return result ;
        }
    }

    private void AddDevice ( IDevice device )
    {
        var newDevice = new Device ( device ) ;

        _discoveredDevices[device.Address] = newDevice ;

        _logger.Information ( "[{Mac}] Device added" ,
                              device.MacAddress ) ;
    }

    private void UpdateDevice ( IDevice device ,
                                Device storedDevice )
    {
        if ( string.IsNullOrWhiteSpace ( storedDevice.Name ) &&
             ! string.IsNullOrWhiteSpace ( device.Name ) )
        {
            storedDevice.Name = device.Name ;
        }

        storedDevice.RawSignalStrengthInDBm = device.RawSignalStrengthInDBm ;
        storedDevice.BroadcastTime = device.BroadcastTime ;

        _logger.Information ( "[{Mac}] Device updated" ,
                              device.MacAddress ) ;
    }
}
