using FluentAssertions ;
using FluentAssertions.Execution ;

namespace Idasen.BluetoothLE.Characteristics.Tests.Common ;

[ TestClass ]
public class SimpleKeysAndValuesBaseTests
{
    [ TestMethod ]
    public void Indexer_ForNewKeyAndValue_AddsKeyAndValue ( )
    {
        var sut = new TestSimpleKeysAndValuesBase ( ) ;
        var key = "key" ;
        var id = Guid.NewGuid ( ) ;

        sut [ key ] = id ;

        sut [ key ]
           .Should ( )
           .Be ( id ) ;
    }

    [ TestMethod ]
    public void Clear_ForInvoked_RemovesAllKeys ( )
    {
        var sut = new TestSimpleKeysAndValuesBase ( ) ;
        var key = "key" ;
        var id = Guid.NewGuid ( ) ;

        sut [ key ] = id ;

        sut.Clear ( ) ;

        sut.Count
           .Should ( )
           .Be ( 0 ) ;
    }

    [ TestMethod ]
    public void Count_ForInvoked_ReturnsCount ( )
    {
        var sut = new TestSimpleKeysAndValuesBase ( ) ;
        var key = "key" ;
        var id = Guid.NewGuid ( ) ;

        sut [ key ] = id ;

        sut.Count
           .Should ( )
           .Be ( 1 ) ;
    }

    [ TestMethod ]
    public void Keys_ForInvoked_ReturnsKeys ( )
    {
        var sut = new TestSimpleKeysAndValuesBase ( ) ;
        var key1 = "k1" ;
        var guid1 = Guid.NewGuid ( ) ;
        var key2 = "k2" ;
        var guid2 = Guid.NewGuid ( ) ;

        sut [ key1 ] = guid1 ;
        sut [ key2 ] = guid2 ;

        sut.Keys
           .Should ( )
           .BeEquivalentTo ( key1 ,
                             key2 ) ;
    }

    [ TestMethod ]
    public void ReadOnlyDictionary_ForInvoked_ReturnsClone ( )
    {
        var sut = new TestSimpleKeysAndValuesBase ( ) ;
        var key = "key" ;
        var id = Guid.NewGuid ( ) ;
        var newGuid = Guid.NewGuid ( ) ;

        sut [ key ] = id ;

        var actual = sut.GetReadOnlyDictionary ( ) ;

        using var scope = new AssertionScope ( ) ;

        // modify original dictionary
        sut [ key ] = newGuid ;

        // check original dictionary
        sut [ key ]
           .Should ( )
           .Be ( newGuid ) ;

        // check cloned dictionary
        actual [ key ]
           .Should ( )
           .Be ( id ) ;
    }
}
