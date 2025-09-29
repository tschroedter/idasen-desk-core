using FluentAssertions ;
using FluentAssertions.Execution ;
using Selkie.AutoMocking ;

namespace Idasen.BluetoothLE.Characteristics.Tests.Common ;

[ AutoDataTestClass ]
public class SimpleKeysAndValuesBaseTests
{
    [ AutoDataTestMethod ]
    public void Indexer_ForNewKeyAndValue_AddsKeyAndValue ( TestSimpleKeysAndValuesBase sut ,
                                                            string                      key ,
                                                            Guid                        id )
    {
        sut [ key ] = id ;

        sut [ key ]
           .Should ( )
           .Be ( id ) ;
    }

    [ AutoDataTestMethod ]
    public void Clear_ForInvoked_RemovesAllKeys ( TestSimpleKeysAndValuesBase sut ,
                                                  string                      key ,
                                                  Guid                        id )
    {
        sut [ key ] = id ;

        sut.Clear ( ) ;

        sut.Count
           .Should ( )
           .Be ( 0 ) ;
    }

    [ AutoDataTestMethod ]
    public void Count_ForInvoked_ReturnsCount ( TestSimpleKeysAndValuesBase sut ,
                                                string                      key ,
                                                Guid                        id )
    {
        sut [ key ] = id ;

        sut.Count
           .Should ( )
           .Be ( 1 ) ;
    }

    [ AutoDataTestMethod ]
    public void Keys_ForInvoked_ReturnsKeys ( TestSimpleKeysAndValuesBase sut ,
                                              string                      key1 ,
                                              Guid                        guid1 ,
                                              string                      key2 ,
                                              Guid                        guid2 )
    {
        sut [ key1 ] = guid1 ;
        sut [ key2 ] = guid2 ;

        sut.Keys
           .Should ( )
           .BeEquivalentTo ( key1 ,
                             key2 ) ;
    }

    [ AutoDataTestMethod ]
    public void ReadOnlyDictionary_ForInvoked_ReturnsClone ( TestSimpleKeysAndValuesBase sut ,
                                                             string                      key ,
                                                             Guid                        id ,
                                                             Guid                        newGuid )
    {
        sut [ key ] = id ;

        var actual = sut.ReadOnlyDictionary ;

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