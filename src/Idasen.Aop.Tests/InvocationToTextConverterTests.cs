using System.Collections ;
using System.Collections.ObjectModel ;
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
    public void Convert_Array_VeryLarge_IsClamped ( )
    {
        var target    = new Sample ( ) ;
        var proxy     = CreateProxy ( target ) ;
        var converter = CreateConverter ( ) ;

        var invocation = Intercept ( proxy ,
                                     p => p.WithArray ( new byte[ 1001 ] ) ) ;
        var text = converter.Convert ( invocation ) ;

        text.Should ( ).Contain ( "bytes=[Array too large to process]" ) ;
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

        // Optional: once you choose a cycle marker, assert it here
    }

    [ TestMethod ]
    public void Convert_SensitiveString_IsRedacted ( )
    {
        var target    = new Sample ( ) ;
        var proxy     = CreateProxy ( target ) ;
        var converter = CreateConverter ( ) ;

        var invocation = Intercept ( proxy ,
                                     p => p.WithSensitiveString ( "user-password-123" ) ) ;
        var text = converter.Convert ( invocation ) ;

        text.Should ( ).Contain ( "password=[REDACTED]" ) ;
    }

    [ TestMethod ]
    public void Convert_RefOut_Modifiers_AreIncluded ( )
    {
        var target    = new Sample ( ) ;
        var proxy     = CreateProxy ( target ) ;
        var converter = CreateConverter ( ) ;

        var x = 5 ;

        var invocation = Intercept ( proxy ,
                                     p => p.WithRefOut ( ref x ,
                                                         out _ ) ) ;
        var text = converter.Convert ( invocation ) ;

        text.Should ( ).Contain ( "ref x=5" ) ;
        text.Should ( ).Contain ( "out y=null" ) ;
    }

    [ TestMethod ]
    public void Convert_Params_Modifier_IsIncluded ( )
    {
        var target    = new Sample ( ) ;
        var proxy     = CreateProxy ( target ) ;
        var converter = CreateConverter ( ) ;

        var invocation = Intercept ( proxy ,
                                     p => p.WithParams ( 1 ,
                                                         2 ,
                                                         3 ) ) ;
        var text = converter.Convert ( invocation ) ;

        text.Should ( ).Contain ( "params values=[3]" ) ;
    }

    [ TestMethod ]
    public void Convert_NonGenericIDictionary_SummarizedAsCount ( )
    {
        var target    = new Sample ( ) ;
        var proxy     = CreateProxy ( target ) ;
        var converter = CreateConverter ( ) ;

        var table = new Hashtable { { "a" , 1 } , { "b" , 2 } } ;

        var invocation = Intercept ( proxy ,
                                     p => p.WithNonGenericDictionary ( table ) ) ;
        var text = converter.Convert ( invocation ) ;

        text.Should ( ).Contain ( "dict=[2]" ) ;
    }

    [ TestMethod ]
    public void Convert_GenericDictionary_SummarizedAsCount ( )
    {
        var target    = new Sample ( ) ;
        var proxy     = CreateProxy ( target ) ;
        var converter = CreateConverter ( ) ;

        var dict = new Dictionary < string , int > { { "a" , 1 } , { "b" , 2 } , { "c" , 3 } } ;

        var invocation = Intercept ( proxy ,
                                     p => p.WithGenericDictionary ( dict ) ) ;
        var text = converter.Convert ( invocation ) ;

        text.Should ( ).Contain ( "dict=[3]" ) ;
    }

    [ TestMethod ]
    public void Convert_Enumerable_SummarizedWithPrefix ( )
    {
        var target    = new Sample ( ) ;
        var proxy     = CreateProxy ( target ) ;
        var converter = CreateConverter ( ) ;

        var list = new List < int > { 1 , 2 , 3 } ;

        var invocation = Intercept ( proxy ,
                                     p => p.WithObject ( list ) ) ;
        var text = converter.Convert ( invocation ) ;

        text.Should ( ).Contain ( "o=enumerable[3]" ) ;
    }

    [ TestMethod ]
    public void Convert_NullInvocation_Throws ( )
    {
        var converter = CreateConverter ( ) ;

        FluentActions.Invoking ( ( ) => converter.Convert ( null! ) )
                     .Should ( ).Throw < ArgumentNullException > ( ) ;
    }

    [ TestMethod ]
    public void Convert_DictionaryTooLarge_IsClamped ( )
    {
        var target    = new Sample ( ) ;
        var proxy     = CreateProxy ( target ) ;
        var converter = CreateConverter ( ) ;

        var table = new Hashtable ( ) ;
        for ( var i = 0 ; i < 1001 ; i ++ )
            table [ i ] = i ;

        var invocation = Intercept ( proxy ,
                                     p => p.WithNonGenericDictionary ( table ) ) ;
        var text = converter.Convert ( invocation ) ;

        text.Should ( ).Contain ( "dict=[Dictionary too large to process]" ) ;
    }

    [ TestMethod ]
    public void Convert_GenericDictionaryTooLarge_IsClamped ( )
    {
        var target    = new Sample ( ) ;
        var proxy     = CreateProxy ( target ) ;
        var converter = CreateConverter ( ) ;

        var dict = new Dictionary < string , int > ( ) ;
        for ( var i = 0 ; i < 1001 ; i ++ )
            dict [ i.ToString ( ) ] = i ;

        var invocation = Intercept ( proxy ,
                                     p => p.WithGenericDictionary ( dict ) ) ;
        var text = converter.Convert ( invocation ) ;

        // Dictionary<TKey, TValue> also implements non-generic IDictionary, so it follows that branch
        text.Should ( ).Contain ( "dict=[Dictionary too large to process]" ) ;
    }

    [ TestMethod ]
    public void Convert_ReadOnlyDictionary_SummarizedAsCount_UsesGenericPath ( )
    {
        var target    = new Sample ( ) ;
        var proxy     = CreateProxy ( target ) ;
        var converter = CreateConverter ( ) ;

        var writable = new Dictionary < string , int > { { "a" , 1 } , { "b" , 2 } } ;
        var ro       = new ReadOnlyDictionary < string , int > ( writable ) ;

        var invocation = Intercept ( proxy ,
                                     p => p.WithReadOnlyDictionary ( ro ) ) ;
        var text = converter.Convert ( invocation ) ;

        text.Should ( ).Contain ( "dict=[2]" ) ;
    }

    [ TestMethod ]
    public void Convert_ReadOnlyDictionaryTooLarge_IsClamped ( )
    {
        var target    = new Sample ( ) ;
        var proxy     = CreateProxy ( target ) ;
        var converter = CreateConverter ( ) ;

        var dict = new Dictionary < string , int > ( ) ;
        for ( var i = 0 ; i < 1001 ; i ++ )
            dict [ i.ToString ( ) ] = i ;
        var ro = new ReadOnlyDictionary < string , int > ( dict ) ;

        var invocation = Intercept ( proxy ,
                                     p => p.WithReadOnlyDictionary ( ro ) ) ;
        var text = converter.Convert ( invocation ) ;

        text.Should ( ).Contain ( "dict=[Dictionary too large to process]" ) ;
    }

    private static T CreateProxy < T > ( T target )
        where T : class
    {
        var generator = new ProxyGenerator ( ) ;
        return generator.CreateClassProxyWithTarget ( target ) ;
    }

    private static IInvocation Intercept < T > ( T            proxy ,
                                                 Action < T > call )
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

        public virtual void Primitives ( int    x ,
                                         string s )
        {
        }

        public virtual void WithArray ( byte [ ] bytes )
        {
        }

        public virtual void WithObject ( object o )
        {
        }

        public virtual void WithSensitiveString ( string password )
        {
        }

        public virtual void WithRefOut ( ref int    x ,
                                         out string y )
        {
            y = "assigned" ;
        }

        public virtual void WithParams ( params int [ ] values )
        {
        }

        public virtual void WithNonGenericDictionary ( IDictionary dict )
        {
        }

        public virtual void WithGenericDictionary ( Dictionary < string , int > dict )
        {
        }

        public virtual void WithReadOnlyDictionary ( IReadOnlyDictionary < string , int > dict )
        {
        }
    }

    private sealed class CaptureInterceptor ( Action < IInvocation > onInvoke ) : IInterceptor
    {
#pragma warning disable S3218
        public void Intercept ( IInvocation invocation )
        {
            onInvoke ( invocation ) ;
            // do not proceed to avoid side effects
        }
#pragma warning restore S3218
    }

    private sealed class Node ( string name )
    {
        [ UsedImplicitly ] public string Name { get ; } = name ;

        [ UsedImplicitly ] public Node ? Next { get ; set ; }
    }
}