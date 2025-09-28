namespace Idasen.BluetoothLE.Tests ;

using FluentAssertions ;
using FluentAssertions.Primitives ;
using FluentAssertions.Specialized ;

public static class ExceptionAssertionsExtensions
{
    /// <summary>
    ///     This extension allows to check the parameter of an exception.
    /// </summary>
    /// <param name="assertions">The assertions.</param>
    /// <param name="parameter">The expected parameter name.</param>
    /// <returns></returns>
    public static AndConstraint < StringAssertions > WithParameter (
        this ExceptionAssertions < ArgumentNullException > assertions ,
        string parameter )
    {
        return assertions.And
                         .ParamName
                         .Should ( )
                         .Be ( parameter ) ;
    }
}
