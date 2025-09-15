using FluentAssertions ;

namespace Idasen.BluetoothLE.Core.Tests ;

[ TestClass ]
public class ULongExtensionsTests
{
    [ TestMethod ]
    public void ToMacAddress_ForValidValue_ReturnsMacAddress ( )
    {
        const ulong value = 254682828386071 ;

        value.ToMacAddress ( )
             .Should ( )
             .Be ( "E7:A1:F7:84:2F:17" ) ;
    }

    [ TestMethod ]
    public void ToMacAddress_ForValueIsZero_ReturnsZeroPaddedMac ( )
    {
        const ulong value = 0 ;

        value.ToMacAddress ( )
             .Should ( )
             .Be ( "00:00:00:00:00:00" ) ;
    }

    [ TestMethod ]
    public void ToMacAddress_ForValueNotLargeEnough_PadsOnTheLeft ( )
    {
        const ulong value = 2546828283860 ;

        value.ToMacAddress ( )
             .Should ( )
             .Be ( "02:50:FA:CB:8F:D4" ) ;
    }

    [ TestMethod ]
    public void ToMacAddress_ForValueTooLarge_UsesLower48Bits ( )
    {
        const ulong value = 25468282838607111 ;

        value.ToMacAddress ( )
             .Should ( )
             .Be ( "7B:44:AF:A2:65:07" ) ;
    }
}