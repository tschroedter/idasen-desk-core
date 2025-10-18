using FluentAssertions ;

namespace Idasen.BluetoothLE.Linak.Tests ;

[ TestClass ]
public class ErrorDetailsTests
{
    [ TestMethod ]
    public void Constructor_ShouldInitializeProperties_WhenValidArgumentsProvided ( )
    {
        // Arrange
        var message   = "Test message" ;
        var caller    = "TestCaller" ;
        var exception = new ArgumentException ( "Test exception" ) ;

        // Act
        var errorDetails = new ErrorDetails ( message ,
                                              caller ,
                                              exception ) ;

        // Assert
        errorDetails.Message.Should ( ).Be ( message ) ;
        errorDetails.Caller.Should ( ).Be ( caller ) ;
        errorDetails.Exception.Should ( ).Be ( exception ) ;
    }

    [ TestMethod ]
    public void Constructor_ShouldThrowArgumentNullException_WhenMessageIsNull ( )
    {
        // Arrange
        var caller = "TestCaller" ;

        // Act
        Action act = ( ) => _ = new ErrorDetails ( null! ,
                                                   caller ) ;

        // Assert
        act.Should ( ).Throw < ArgumentNullException > ( ).WithMessage ( "*message*" ) ;
    }

    [ TestMethod ]
    public void Constructor_ShouldThrowArgumentNullException_WhenCallerIsNull ( )
    {
        // Arrange
        var message = "Test message" ;

        // Act
        Action act = ( ) => _ = new ErrorDetails ( message ,
                                                   null! ) ;

        // Assert
        act.Should ( ).Throw < ArgumentNullException > ( ).WithMessage ( "*caller*" ) ;
    }

    [ TestMethod ]
    public void ToString_ShouldReturnFormattedMessage_WhenExceptionIsNull ( )
    {
        // Arrange
        var message = "Test message" ;
        var caller  = "TestCaller" ;
        var errorDetails = new ErrorDetails ( message ,
                                              caller ) ;

        // Act
        var result = errorDetails.ToString ( ) ;

        // Assert
        result.Should ( ).Be ( "[TestCaller] Test message" ) ;
    }

    [ TestMethod ]
    public void ToString_ShouldReturnFormattedMessageWithException_WhenExceptionIsNotNull ( )
    {
        // Arrange
        var message   = "Test message" ;
        var caller    = "TestCaller" ;
        var exception = new ArgumentException ( "Test exception" ) ;
        var errorDetails = new ErrorDetails ( message ,
                                              caller ,
                                              exception ) ;

        // Act
        var result = errorDetails.ToString ( ) ;

        // Assert
        result.Should ( ).Be ( "[TestCaller] Test message (Test exception)" ) ;
    }
}
