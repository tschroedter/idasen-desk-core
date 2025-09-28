namespace Idasen.BluetoothLE.Linak.Tests ;

using Core.Interfaces.ServicesDiscovery ;
using FluentAssertions ;
using Interfaces ;
using NSubstitute ;

[ TestClass ]
public class DeskFactoryTests
{
    private Func < IDevice , IDeskConnector > _deskConnectorFactory = null! ;
    private Func < IDeskConnector , IDesk > _deskFactory = null! ;
    private IDeviceFactory _deviceFactory = null! ;

    [ TestInitialize ]
    public void Initialize ( )
    {
        _deviceFactory = Substitute.For < IDeviceFactory > ( ) ;
        _deskConnectorFactory = Substitute.For < Func < IDevice , IDeskConnector > > ( ) ;
        _deskFactory = Substitute.For < Func < IDeskConnector , IDesk > > ( ) ;
    }

    [ TestMethod ]
    public async Task CreateAsync_ForInvoked_ReturnsInstance ( )
    {
        IDesk actual = await CreateSut ( ).CreateAsync ( 1u ) ;

        actual.Should ( )
              .NotBeNull ( ) ;
    }

    private DeskFactory CreateSut ( )
    {
        return new DeskFactory ( _deviceFactory ,
                                 _deskConnectorFactory ,
                                 _deskFactory ) ;
    }
}
