namespace Idasen.Aop ;

using System.Diagnostics.CodeAnalysis ;
using Aspects ;
using Autofac ;
using Interfaces ;
using Serilog ;

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

        _ = builder.Register ( c => new LogAspect ( c.Resolve < ILogger > ( ) ,
                                                    c.Resolve < IInvocationToTextConverter > ( ) ) ) ;
    }
}
