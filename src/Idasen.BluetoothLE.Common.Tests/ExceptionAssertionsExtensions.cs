using FluentAssertions ;
using FluentAssertions.Primitives ;
using FluentAssertions.Specialized ;
using JetBrains.Annotations ;

namespace Idasen.BluetoothLE.Common.Tests ;

public static class ExceptionAssertionsExtensions
{
    /// <summary>
    ///     This extension allows to check the parameter of an exception.
    /// </summary>
    /// <param name="assertions">The assertions.</param>
    /// <param name="parameter">The expected parameter name.</param>
    /// <returns></returns>
    [ UsedImplicitly ]
    public async static Task < AndConstraint < StringAssertions > > WithParameter (
        this Task < ExceptionAssertions < ArgumentNullException > > assertions ,
        string parameter )
    {
        ArgumentNullException.ThrowIfNull ( assertions ) ;
        ArgumentNullException.ThrowIfNull ( parameter ) ;

        var a = await assertions.ConfigureAwait ( false ) ;
        return a.And.ParamName.Should ( ).Be ( parameter ) ;
    }

    /// <summary>
    ///     This extension allows to check the parameter of an exception.
    /// </summary>
    /// <param name="assertions">The assertions.</param>
    /// <param name="parameter">The expected parameter name.</param>
    /// <returns></returns>
    [ UsedImplicitly ]
    public static AndConstraint < StringAssertions > WithParameter (
        this ExceptionAssertions < ArgumentNullException > assertions ,
        string parameter )
    {
        ArgumentNullException.ThrowIfNull ( assertions ) ;
        ArgumentNullException.ThrowIfNull ( parameter ) ;

        return assertions.And
                         .ParamName
                         .Should ( )
                         .Be ( parameter ) ;
    }

    /// <summary>
    ///     This extension allows to check the parameter of an exception.
    /// </summary>
    /// <param name="assertions">The assertions.</param>
    /// <param name="parameter">The expected parameter name.</param>
    /// <returns></returns>
    [ UsedImplicitly ]
    public static AndConstraint < StringAssertions > WithParameter (
        this ExceptionAssertions < ArgumentException > assertions ,
        string parameter )
    {
        ArgumentNullException.ThrowIfNull ( assertions ) ;
        ArgumentNullException.ThrowIfNull ( parameter ) ;

        return assertions.And
                         .ParamName
                         .Should ( )
                         .Be ( parameter ) ;
    }
}