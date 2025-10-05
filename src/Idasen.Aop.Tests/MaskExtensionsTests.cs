using FluentAssertions ;

namespace Idasen.Aop.Tests ;

[ TestClass ]
public class MaskExtensionsTests
{
    [ TestMethod ]
    public void MaskMacAddress_ForShortString_ReturnsOriginal ( )
    {
        // Arrange
        var input = "1234" ;

        // Act
        var result = input.MaskMacAddress ( ) ;

        // Assert
        result.Should ( ).Be ( "1234" ) ;
    }

    [ TestMethod ]
    public void MaskMacAddress_ForValidMacAddress_MasksCorrectly ( )
    {
        // Arrange
        var input = "AA:BB:CC:DD:EE:FF" ;

        // Act
        var result = input.MaskMacAddress ( ) ;

        // Assert
        result.Should ( ).Be ( "***-EE:FF" ) ;
    }

    [ TestMethod ]
    public void MaskAddress_ForShortAddress_ReturnsOriginal ( )
    {
        // Arrange
        const ulong input = 1234 ;

        // Act
        var result = input.MaskAddress ( ) ;

        // Assert
        result.Should ( ).Be ( "***01234" ) ; // Hexadecimal representation of 1234
    }

    [ TestMethod ]
    public void MaskAddress_ForValidAddress_MasksCorrectly ( )
    {
        // Arrange
        const ulong input = 0x123456789ABC ;

        // Act
        var result = input.MaskAddress ( ) ;

        // Assert
        result.Should ( ).Be ( "***43868" ) ;
    }

    [ TestMethod ]
    public void MaskAddress_ForZeroAddress_ReturnsMasked ( )
    {
        // Arrange
        const ulong input = 0 ;

        // Act
        var result = input.MaskAddress ( ) ;

        // Assert
        result.Should ( ).Be ( "***00000" ) ;
    }
}
