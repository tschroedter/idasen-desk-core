using System ;
using System.Threading.Tasks ;
using FluentAssertions ;
using FluentAssertions.Primitives ;
using FluentAssertions.Specialized ;
using JetBrains.Annotations ;

namespace Idasen.BluetoothLE.Common.Tests
{
    public static class ExceptionAssertionsExtensions
    {
        /// <summary>
        ///     This extension allows to check the parameter of an exception.
        /// </summary>
        /// <param name="assertions">The assertions.</param>
        /// <param name="parameter">The expected parameter name.</param>
        /// <returns></returns>
        [ UsedImplicitly ]
        public static Task < AndConstraint < StringAssertions > > WithParameter (
            this Task < ExceptionAssertions < ArgumentNullException > > assertions ,
            string                                                      parameter )
        {
            // todo there must be a better way, return await assertions... something
            var test = assertions.GetAwaiter ( ).GetResult ( ).And.ParamName.Should ( ).Be ( parameter ) ;

            return Task.FromResult ( test ) ;
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
            string                                             parameter )
        {
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
            string                                         parameter )
        {
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
        public static AndConstraint < StringAssertions > WithParameterAsync (
            this Task < ExceptionAssertions < ArgumentNullException > > assertions ,
            string                                                      parameter )
        {
            return assertions.Result
                             .And
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
        public static AndConstraint < StringAssertions > WithParameterAsync (
            this Task < ExceptionAssertions < ArgumentException > > assertions ,
            string                                                  parameter )
        {
            return assertions.Result
                             .And
                             .ParamName
                             .Should ( )
                             .Be ( parameter ) ;
        }
    }
}