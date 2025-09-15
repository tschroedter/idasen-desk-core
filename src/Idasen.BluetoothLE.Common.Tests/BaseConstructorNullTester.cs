using Autofac ;
using AutofacSerilogIntegration ;
using FluentAssertions ;
using FluentAssertions.Execution ;
using Selkie.DefCon.One ;
using Selkie.DefCon.One.Common ;
using Selkie.DefCon.One.Interfaces ;
using Serilog ;
using Serilog.Events ;
using Serilog.Sinks.SystemConsole.Themes ;

namespace Idasen.BluetoothLE.Common.Tests ;

public abstract class BaseConstructorNullTester<T> where T : class
{
    private IContainer? _container ;
    public virtual int NumberOfConstructorsPassed { get ; } = 1 ;
    public virtual int NumberOfConstructorsFailed { get ; } = 0 ;

    protected IContainer Container
    {
        get
        {
            if ( _container == null )
            {
                throw new InvalidOperationException ( "Container not initialized. Ensure Initialize() ran before accessing the container." ) ;
            }

            return _container ;
        }
    }

    [ TestCleanup ]
    public virtual void Cleanup ( )
    {
        _container?.Dispose ( ) ;
        _container = null ;
    }


    [ TestInitialize ]
    public virtual void Initialize ( )
    {
        ConfigureLogger ( ) ;
        _container = BuildContainer ( ) ;
    }

    protected virtual void ConfigureLogger ( )
    {
        const string template = "[{Timestamp:HH:mm:ss.ffff} {Level:u3}] {Message:lj}{NewLine}{Exception}" ;

        Log.Logger = new LoggerConfiguration ( )
                    .Enrich.WithCaller ( )
                    .MinimumLevel.Information ( )
                    .WriteTo.Console ( LogEventLevel.Information ,
                                       template ,
                                       theme : AnsiConsoleTheme.Code )
                    .CreateLogger ( ) ;
    }

    protected virtual ContainerBuilder CreateContainerBuilder ( )
    {
        return new ContainerBuilder ( ) ;
    }

    protected virtual void RegisterModules ( ContainerBuilder builder )
    {
        builder.RegisterLogger ( ) ;
        builder.RegisterModule < DefConOneModule > ( ) ;
    }

    protected virtual IContainer BuildContainer ( )
    {
        var builder = CreateContainerBuilder ( ) ;
        RegisterModules ( builder ) ;
        return builder.Build ( ) ;
    }

    [ TestMethod ]
    public virtual void Constructor_ForAnyParameterNullThrows_AllPassing ( )
    {
        var tester = CreateTester ( ) ;

        tester.Test < T > ( ) ;

        using (new AssertionScope ( ))
        {
            tester.HasPassed
                  .Should ( )
                  .BeTrue ( "Has Passed" ) ;

            tester.ConstructorsToTest
                  .Should ( )
                  .Be ( NumberOfConstructorsPassed ,
                        "ConstructorsToTest" ) ;

            tester.ConstructorsFailed
                  .Should ( )
                  .Be ( NumberOfConstructorsFailed ,
                        "ConstructorsFailed" ) ;
        }
    }

    protected virtual INotNullTester CreateTester ( )
    {
        return Container.Resolve < INotNullTester > ( ) ;
    }
}