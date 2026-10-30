using System ;
using System.Linq ;
using System.Reflection ;
using System.Runtime.Serialization ;
using NSubstitute ;

namespace Idasen.BluetoothLE.Common.Tests ;

public interface INotNullTester
{
    void Test< T > () where T : class ;
    bool HasPassed { get ; }
    int ConstructorsToTest { get ; }
    int ConstructorsFailed { get ; }
}

public class NotNullTester : INotNullTester
{
    public bool HasPassed { get ; private set ; } = true ;

    public int ConstructorsToTest { get ; private set ; }

    public int ConstructorsFailed { get ; private set ; }

    public void Test< T > ()
        where T : class
    {
        HasPassed = true ;
        ConstructorsToTest = 0 ;
        ConstructorsFailed = 0 ;

        var type = typeof ( T ) ;

        var ctors = type.GetConstructors ( BindingFlags.Public | BindingFlags.Instance ) ;

        foreach ( var ctor in ctors )
        {
            var parameters = ctor.GetParameters ( ) ;

            // nothing to test if no parameters
            if ( parameters.Length == 0 )
                continue ;

            ConstructorsToTest++ ;

            var ctorPassed = true ;

            for ( var i = 0 ; i < parameters.Length ; i++ )
            {
                var args = new object[ parameters.Length ] ;

                for ( var j = 0 ; j < parameters.Length ; j++ )
                {
                    if ( j == i )
                    {
                        // set the tested parameter to null when reference type
                        args[ j ] = parameters[ j ].ParameterType.IsValueType
                                       ? Activator.CreateInstance ( parameters[ j ].ParameterType )!
                                       : null! ;
                    }
                    else
                    {
                        args[ j ] = CreateNonNull ( parameters[ j ].ParameterType ) ;
                    }
                }

                try
                {
                    _ = ctor.Invoke ( args ) ;

                    // if invoke succeeded without throwing ArgumentNullException the test failed
                    ctorPassed = false ;
                    break ;
                }
                catch ( TargetInvocationException tie )
                {
                    if ( tie.InnerException is ArgumentNullException )
                    {
                        // expected
                        continue ;
                    }

                    ctorPassed = false ;
                    break ;
                }
                catch ( ArgumentNullException )
                {
                    // expected
                    continue ;
                }
                catch
                {
                    ctorPassed = false ;
                    break ;
                }
            }

            if ( !ctorPassed )
            {
                ConstructorsFailed++ ;
                HasPassed = false ;
            }
        }
    }

    private static object CreateNonNull ( Type t )
    {
        if ( t == typeof ( string ) )
            return "value" ;

        if ( t.IsValueType )
            return Activator.CreateInstance ( t )! ;

        if ( t.IsInterface || t.IsAbstract )
        {
            try
            {
                return Substitute.For ( new[] { t } , Array.Empty<object>() ) ;
            }
            catch
            {
                // fall through
            }
        }

        try
        {
            return Activator.CreateInstance ( t )! ;
        }
        catch
        {
            // as fallback return a substitute for the type if possible
            try
            {
                return Substitute.For ( new[] { t } , Array.Empty<object>() ) ;
            }
            catch
            {
                return new object ( ) ;
            }
        }
    }
}
