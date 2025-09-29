using FluentAssertions ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery.Wrappers ;
using Idasen.BluetoothLE.Core.ServicesDiscovery ;
using NSubstitute ;

namespace Idasen.BluetoothLE.Core.Tests.ServicesDiscovery ;

[ TestClass ]
public class DeviceFactoryTests
{
    private Device.Factory                   _deviceFactory  = null! ;
    private IBluetoothLEDeviceProvider       _deviceProvider = null! ;
    private IBluetoothLeDeviceWrapperFactory _wrapperFactory = null! ;

    [ TestInitialize ]
    public void Initialize ( )
    {
        _deviceFactory  = Substitute.For < Device.Factory > ( ) ;
        _wrapperFactory = Substitute.For < IBluetoothLeDeviceWrapperFactory > ( ) ;
        _deviceProvider = Substitute.For < IBluetoothLEDeviceProvider > ( ) ;
    }

    [ TestMethod ]
    public void Constructor_ForInvoked_Instance ( )
    {
        CreateSut ( ).Should ( )
                     .NotBeNull ( ) ;
    }

    private DeviceFactory CreateSut ( )
    {
        return new DeviceFactory ( _deviceFactory ,
                                   _wrapperFactory ,
                                   _deviceProvider ) ;
    }
}