using Idasen.BluetoothLE.Linak.Control ;

namespace Idasen.BluetoothLE.Linak.Tests.Control ;

[ TestClass ]
public class StopDetailsTests
{
    [ TestMethod ]
    public void Constructor_SetsProperties ( )
    {
        var details = new StopDetails ( true ,
                                        Direction.Up ) ;

        Assert.IsTrue ( details.ShouldStop ) ;
        Assert.AreEqual ( Direction.Up ,
                          details.Desired ) ;
    }

    [ TestMethod ]
    public void Equality_WorksForSameValues ( )
    {
        var a = new StopDetails ( true ,
                                  Direction.Down ) ;
        var b = new StopDetails ( true ,
                                  Direction.Down ) ;

        Assert.AreEqual ( a ,
                          b ) ;
        Assert.IsTrue ( a  == b ) ;
        Assert.IsFalse ( a != b ) ;
    }

    [ TestMethod ]
    public void Equality_WorksForDifferentValues ( )
    {
        var a = new StopDetails ( true ,
                                  Direction.Up ) ;
        var b = new StopDetails ( false ,
                                  Direction.Down ) ;

        Assert.AreNotEqual ( a ,
                             b ) ;
        Assert.IsFalse ( a == b ) ;
        Assert.IsTrue ( a  != b ) ;
    }
}