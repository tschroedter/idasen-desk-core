using System.Diagnostics.CodeAnalysis ;
using Autofac ;
using Idasen.Aop.Aspects ;
using Idasen.Aop.Interfaces ;
using Serilog ;

namespace Idasen.Aop ;

// ReSharper disable once InconsistentNaming
/// <summary>
///     Autofac module that registers AOP-related components for BluetoothLE, including
///     invocation-to-text conversion and logging aspects.
/// </summary>
[ ExcludeFromCodeCoverage ]
public sealed class BluetoothLEAop
    : Module
{
    /// <summary>
    ///     Adds the AOP services to the provided container builder.
    /// </summary>
    /// <param name="builder">The Autofac container builder.</param>
    protected override void Load ( ContainerBuilder builder )
    {
        ArgumentNullException.ThrowIfNull ( builder ) ;

        _ = builder.RegisterType < InvocationToTextConverter > ( )
                   .As < IInvocationToTextConverter > ( ) ;

        _ = builder.Register ( c => new LogAspect (
                                                   c.Resolve < ILogger > ( ) ,
                                                   c.Resolve < IInvocationToTextConverter > ( ) ) ) ;
    }
}
