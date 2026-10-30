using FluentAssertions ;
using Autofac ;
using Idasen.BluetoothLE.Characteristics.Characteristics ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery ;
using Microsoft.VisualStudio.TestTools.UnitTesting ;
using NSubstitute ;

namespace Idasen.BluetoothLE.Characteristics.Tests.Characteristics.Factories ;

[ TestClass ]
public class CharacteristicBaseFactoryTests
{
    private sealed class Dummy { public int Value { get; set; } = 42; }
    [ TestMethod ]
    public void Create_ForInvoked_Instance ( )
    {
        // Use a real Autofac lifetime scope and register a dummy type to resolve
        var builder = new ContainerBuilder ( ) ;

        builder.RegisterType<Dummy> ( ).As<Dummy> ( ) ;

        var container = builder.Build ( ) ;
        var scope = container.BeginLifetimeScope ( ) ;

        var sut = new CharacteristicBaseFactory ( scope ) ;
        var device = Substitute.For< IDevice > ( ) ;

        sut.Create<Dummy> ( device )
           .Should ( )
           .NotBeNull ( ) ;
    }
}
