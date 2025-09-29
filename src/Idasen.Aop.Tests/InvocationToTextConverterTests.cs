using Castle.DynamicProxy ;
using FluentAssertions ;
using JetBrains.Annotations ;

namespace Idasen.Aop.Tests ;

[ TestClass ]
public class InvocationToTextConverterTests
{
    private static InvocationToTextConverter CreateConverter ( )
    {
        return new InvocationToTextConverter ( ) ;
    }

    [ TestMethod ]
    public void Convert_NoArgs_PrintsMethod ( )
    {
        var target    = new Sample ( ) ;
        var proxy     = CreateProxy ( target ) ;
        var converter = CreateConverter ( ) ;

        var invocation = Intercept ( proxy ,
                                     p => p.NoArgs ( ) ) ;
        var text = converter.Convert ( invocation ) ;

        text.Should ( ).Contain ( "Sample.NoArgs(" ) ;
        text.EndsWith ( ')' ).Should ( ).BeTrue ( ) ;
    }

    [ TestMethod ]
    public void Convert_Primitives_IncludesNamesAndValues ( )
    {
        var target    = new Sample ( ) ;
        var proxy     = CreateProxy ( target ) ;
        var converter = CreateConverter ( ) ;

        var invocation = Intercept ( proxy ,
                                     p => p.Primitives ( 42 ,
                                                         "abc" ) ) ;
        var text = converter.Convert ( invocation ) ;

        text.Should ( ).Contain ( "x=42" ) ;
        text.Should ( ).Contain ( "s=abc" ) ;
    }

    [ TestMethod ]
    public void Convert_Array_SummarizedAsLength ( )
    {
        var target    = new Sample ( ) ;
        var proxy     = CreateProxy ( target ) ;
        var converter = CreateConverter ( ) ;

        var invocation = Intercept ( proxy ,
                                     p => p.WithArray ( new byte[ 100 ] ) ) ;
        var text = converter.Convert ( invocation ) ;

        text.Should ( ).Contain ( "bytes=[100]" ) ;
    }

    [ TestMethod ]
    public void Convert_PropertySetter_UsesNameValue ( )
    {
        var target    = new Sample ( ) ;
        var proxy     = CreateProxy ( target ) ;
        var converter = CreateConverter ( ) ;

        var invocation = Intercept ( proxy ,
                                     p => p.SampleProperty = 123 ) ;
        var text = converter.Convert ( invocation ) ;

        text.Should ( )
            .Contain ( "Idasen.Aop.Tests.InvocationToTextConverterTests+Sample.set_SampleProperty(value=123)" ) ;
    }

    [ TestMethod ]
    public void Convert_SelfReferencingParameter_IsCycleSafe ( )
    {
        var target    = new Sample ( ) ;
        var proxy     = CreateProxy ( target ) ;
        var converter = CreateConverter ( ) ;

        var node = new Node ( "root" ) ;
        node.Next = node ; // direct cycle

        var invocation = Intercept ( proxy ,
                                     p => p.WithObject ( node ) ) ;

        // Should not recurse infinitely or throw
        FluentActions.Invoking ( ( ) => converter.Convert ( invocation ) )
                     .Should ( ).NotThrow ( ) ;

        var text = converter.Convert ( invocation ) ;
        text.Should ( ).Contain ( "WithObject(" ) ;
        text.Should ( ).Contain ( "Node" ) ;

        // Optional: once you choose a cycle marker, assert it here, e.g.:
        // text.Should().Contain("<cycle>");
    }

    private static T CreateProxy < T > ( T target )
        where T : class
    {
        var generator = new ProxyGenerator ( ) ;
        return generator.CreateClassProxyWithTarget ( target ) ;
    }

    private static IInvocation Intercept < T > ( T proxy , Action < T > call )
        where T : class
    {
        IInvocation ? captured = null ;

        var generator   = new ProxyGenerator ( ) ;
        var interceptor = new CaptureInterceptor ( i => captured = i ) ;
        var proxied = generator.CreateClassProxyWithTarget ( proxy ,
                                                             interceptor ) ;
        call ( proxied ) ;
        return captured ?? throw new AssertFailedException ( "Invocation was not captured" ) ;
    }

    // Make proxy-able by Castle: public, non-sealed, virtual members
    public class Sample
    {
        public virtual int SampleProperty { get ; set ; }

        public virtual void NoArgs ( )
        {
        }

        public virtual void Primitives ( int x , string s )
        {
        }

        public virtual void WithArray ( byte [ ] bytes )
        {
        }

        public virtual void WithObject ( object o )
        {
        }
    }

    private sealed class CaptureInterceptor ( Action < IInvocation > onInvoke ) : IInterceptor
    {
        public void Intercept ( IInvocation invocation )
        {
            onInvoke ( invocation ) ;
            // do not proceed to avoid side effects
        }
    }

    private sealed class Node ( string name )
    {
        [ UsedImplicitly ] public string Name { get ; } = name ;

        [ UsedImplicitly ] public Node ? Next { get ; set ; }
    }
}