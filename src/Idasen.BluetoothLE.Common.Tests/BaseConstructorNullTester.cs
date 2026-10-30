using System ;
using Autofac ;
using AutofacSerilogIntegration ;
using FluentAssertions ;
using FluentAssertions.Execution ;
using System.Linq.Expressions ;
using System.Reflection ;
using NSubstitute ;
using Serilog ;
using Serilog.Events ;
using Serilog.Sinks.SystemConsole.Themes ;

namespace Idasen.BluetoothLE.Common.Tests ;

[ TestClass ]
public abstract class BaseConstructorNullTester < T >
    where T : class
{
    private IContainer ? _container ;
    // Lock to serialize creation of non-null argument instances which uses
    // reflection, expression compilation and NSubstitute. Running these
    // concurrently from parallel test runners caused intermittent failures.
    // The actual lock is defined in a non-generic helper to avoid static
    // field behavior on generic types which analyzers warn about.

    public abstract int NumberOfConstructorsPassed { get ; }
    public abstract int NumberOfConstructorsFailed { get ; }

    protected IContainer Container =>
        _container ??
        throw new
            InvalidOperationException ( "Container not initialized. Ensure Initialize() ran before accessing the container." ) ;

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
                    // Enrichment intentionally omitted in tests
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
        var result = RunConstructorNullTests ( ) ;

        using ( new AssertionScope ( ) )
        {
            var failedList = result.FailedConstructors != null && result.FailedConstructors.Length > 0
                                 ? string.Join ( " | ", result.FailedConstructors )
                                 : string.Empty ;

            result.HasPassed
                  .Should ( )
                  .BeTrue ( $"Has Passed. Failed constructors: {failedList}" ) ;

            result.ConstructorsToTest
                  .Should ( )
                  .Be ( NumberOfConstructorsPassed ,
                        "ConstructorsToTest" ) ;

            result.ConstructorsFailed
                  .Should ( )
                  .Be ( NumberOfConstructorsFailed ,
                        "ConstructorsFailed" ) ;
        }
    }

    private static ( bool HasPassed , int ConstructorsToTest , int ConstructorsFailed , string[] FailedConstructors )
        RunConstructorNullTests ()
    {
        // Diagnostic: capture which constructors fail to help debugging intermittent failures
        var failedCtors = new List<string> ( ) ;

        var hasPassed = true ;
        var constructorsToTest = 0 ;
        var constructorsFailed = 0 ;

        var type = typeof ( T ) ;

        var ctors = type.GetConstructors ( BindingFlags.Public | BindingFlags.Instance ) ;

        foreach ( var ctor in ctors )
        {
            var parameters = ctor.GetParameters ( ) ;

            if ( parameters.Length == 0 )
                continue ;

            // If every parameter is a value type or intentionally ignored by
            // GuardIgnore, there is nothing to test for null-reference guarding.
            var allSkippable = parameters.All ( p =>
            {
                if ( p.ParameterType.IsValueType )
                    return true ;

                var name = p.GetCustomAttributes ( false )
                            .Select ( a => a?.GetType ( )?.Name )
                            .FirstOrDefault ( ) ;

                if ( name == "GuardIgnoreAttribute" || name == "GuardIgnore" )
                    return true ;

                return false ;
            } ) ;

            if ( allSkippable )
                continue ;

            constructorsToTest++ ;

            var ctorPassed = true ;

            for ( var i = 0 ; i < parameters.Length ; i++ )
            {
                // Skip value type parameters since they cannot be null and
                // the null-constructor tester only verifies reference-type
                // parameter null checks.
                if (parameters[i].ParameterType.IsValueType)
                    continue;

                // Skip parameters marked with GuardIgnore attribute since the
                // constructor intentionally does not guard them against null.
                var hasGuardIgnore = parameters[i].GetCustomAttributes ( false )
                                           .Any ( a =>
                                                  {
                                                      var name = a?.GetType ( )?.Name ;
                                                      return name == "GuardIgnoreAttribute" || name == "GuardIgnore" ;
                                                  } ) ;

                if ( hasGuardIgnore )
                    continue ;
                var args = new object[ parameters.Length ] ;

                for ( var j = 0 ; j < parameters.Length ; j++ )
                {
                    if ( j == i )
                    {
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
                    ctorPassed = false ;
                    var paramName = parameters[i].Name ?? i.ToString();
                    var paramType = parameters[i].ParameterType.Name;
                    failedCtors.Add ( $"{ctor} - param[{i}] {paramName}:{paramType}" ) ;
                    break ;
                }
                catch ( TargetInvocationException tie ) when ( tie.InnerException is ArgumentNullException )
                {
                    // expected
                    continue ;
                }
                catch ( TargetInvocationException tie )
                {
                    // Constructor threw an exception different from ArgumentNullException
                    ctorPassed = false ;
                    var inner = tie.InnerException ;
                    failedCtors.Add ( $"{ctor} -> {inner?.GetType().Name}: {inner?.Message}" ) ;
                    break ;
                }
                catch ( ArgumentNullException )
                {
                    // expected
                    continue ;
                }
                catch ( Exception ex )
                {
                    ctorPassed = false ;
                    failedCtors.Add ( $"{ctor} -> {ex.GetType().Name}: {ex.Message}" ) ;
                    break ;
                }
            }

            if ( !ctorPassed )
            {
                constructorsFailed++ ;
                hasPassed = false ;
            }
        }

        if ( failedCtors.Count > 0 )
            Console.WriteLine ( $"Constructor null tester failed for: {string.Join ( " | ", failedCtors )}" ) ;

        return ( hasPassed , constructorsToTest , constructorsFailed , failedCtors.ToArray ( ) ) ;
    }

    private static object CreateNonNull ( Type t )
    {
        // Serialize creation of instances that rely on reflection, expression
        // compilation or NSubstitute. These operations touch shared runtime
        // state and caused race conditions when tests ran in parallel.
        lock ( TestLocks.CreateNonNullLock )
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
                return new object ( ) ;
            }
        }

        // Try to create a concrete instance. If the type has a parameterless constructor use it.
        try
        {
            var instance = Activator.CreateInstance ( t ) ;
            if ( instance != null )
                return instance ;
        }
        catch
        {
            // ignored - fallback below
        }

        // Try to find an assignable concrete type with parameterless ctor in loaded assemblies
        try
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies ( ) ;

            foreach ( var asm in assemblies )
            {
                Type[] types ;

                try
                {
                    types = asm.GetTypes ( ) ;
                }
                catch
                {
                    continue ;
                }

                var candidate = types.FirstOrDefault ( x => t.IsAssignableFrom ( x ) && !x.IsAbstract && x.GetConstructor ( Type.EmptyTypes ) != null ) ;

                if ( candidate != null )
                {
                    try
                    {
                        var inst = Activator.CreateInstance ( candidate ) ;
                        if ( inst != null )
                            return inst ;
                    }
                    catch
                    {
                        // continue searching
                    }
                }
            }
        }
        catch
        {
            // ignore
        }

        // Special handling for delegate types (Func<...> / Action<...>) used in many factories
        if ( typeof ( Delegate ).IsAssignableFrom ( t ) )
        {
            try
            {
                // Create a simple delegate that returns default(TResult) or does nothing for Action
                var invokeMethod = t.GetMethod ( "Invoke" ) ;
                if ( invokeMethod != null )
                {
                    var parameters = invokeMethod.GetParameters ( ).Select ( p => Expression.Parameter ( p.ParameterType , p.Name ) ).ToArray ( ) ;
                    Expression body ;

                    if ( invokeMethod.ReturnType == typeof ( void ) )
                    {
                        body = Expression.Empty ( ) ;
                    }
                    else
                    {
                        var defaultValue = Expression.Default ( invokeMethod.ReturnType ) ;
                        body = defaultValue ;
                    }

                    var lambda = Expression.Lambda ( t , body , parameters ) ;
                    return lambda.Compile ( ) ;
                }
            }
            catch
            {
                // ignore
            }
        }

        // As a last resort, try to create an NSubstitute for the concrete type so it is assignable
        try
        {
            return Substitute.For ( new[] { t } , Array.Empty<object>() ) ;
        }
        catch
        {
            // fallback to plain object when substitute/creation fails
            return new object ( ) ;
        }
        }
    }
}
