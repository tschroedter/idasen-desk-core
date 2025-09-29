using FluentAssertions ;
using Idasen.BluetoothLE.Characteristics.Characteristics.Unknowns ;
using Idasen.BluetoothLE.Characteristics.Common ;

namespace Idasen.BluetoothLE.Characteristics.Tests.Characteristics.Unknowns ;

[ TestClass ]
public class ReferenceOutputTests
{
    [ TestMethod ]
    public void GattServiceUuid_ForInvoked_Empty ( )
    {
        using var sut = CreateSut ( );

        sut.GattServiceUuid
           .Should ( )
           .Be ( Guid.Empty ) ;
    }

    [ TestMethod ]
    public void RawHeightSpeed_ForInvoked_Empty ( )
    {
        using var sut = CreateSut();

        sut.RawHeightSpeed
           .Should ( )
           .BeEmpty ( ) ;
    }

    [ TestMethod ]
    public void RawTwo_ForInvoked_Empty ( )
    {
        using var sut = CreateSut();

        sut.RawTwo
           .Should ( )
           .BeEmpty ( ) ;
    }

    [ TestMethod ]
    public void RawThree_ForInvoked_Empty ( )
    {
        using var sut = CreateSut();

        sut.RawThree
           .Should ( )
           .BeEmpty ( ) ;
    }

    [ TestMethod ]
    public void RawFour_ForInvoked_Empty ( )
    {
        using var sut = CreateSut();

        sut.RawFour
           .Should ( )
           .BeEmpty ( ) ;
    }

    [ TestMethod ]
    public void RawFive_ForInvoked_Empty ( )
    {
        using var sut = CreateSut();

        sut.RawFive
           .Should ( )
           .BeEmpty ( ) ;
    }

    [ TestMethod ]
    public void RawSix_ForInvoked_Empty ( )
    {
        using var sut = CreateSut();

        sut.RawSix
           .Should ( )
           .BeEmpty ( ) ;
    }

    [ TestMethod ]
    public void RawSeven_ForInvoked_Empty ( )
    {
        using var sut = CreateSut();

        sut.RawSeven
           .Should ( )
           .BeEmpty ( ) ;
    }

    [ TestMethod ]
    public void RawEight_ForInvoked_Empty ( )
    {
        using var sut = CreateSut();

        sut.RawEight
           .Should ( )
           .BeEmpty ( ) ;
    }

    [ TestMethod ]
    public void RawMask_ForInvoked_Empty ( )
    {
        using var sut = CreateSut();

        sut.RawMask
           .Should ( )
           .BeEmpty ( ) ;
    }

    [ TestMethod ]
    public void RawDetectMask_ForInvoked_Empty ( )
    {
        using var sut = CreateSut();

        sut.RawDetectMask
           .Should ( )
           .BeEmpty ( ) ;
    }

    [ TestMethod ]
    public void HeightSpeedChanged_ForInvoked_Throws ( )
    {
        Action action = ( ) => CreateSut ( ).HeightSpeedChanged
                                            .Subscribe ( ) ;

        action.Should ( )
              .Throw < NotInitializeException > ( ) ;
    }

    [ TestMethod ]
    public void Dispose_ForInvoked_DoesNothing ( )
    {
        var action = ( ) => CreateSut ( ).Dispose ( ) ;

        action.Should ( )
              .NotThrow < Exception > ( ) ;
    }

    private ReferenceOutput CreateSut ( ) => new ( ) ;
}
