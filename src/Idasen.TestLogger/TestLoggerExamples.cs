using FluentAssertions ;
using Serilog.Events ;

namespace Idasen.TestLogger ;

/// <summary>
///     Example tests demonstrating how to use the LoggerForTests class.
/// </summary>
[ TestClass ]
public class LoggerForTestsExamples 
{
    [ TestMethod ]
    public void Example_BasicUsage_VerifyLogContainsText ( )
    {
        // Arrange
        using var testLogger = new LoggerForTests ( ) ;

        // Act - Use the logger
        testLogger.Logger.Information ( "Processing started" ) ;
        testLogger.Logger.Debug ( "Found GattDeviceService with UUID {Uuid}" ,
                                  Guid.NewGuid ( ) ) ;
        testLogger.Logger.Warning ( "Processing completed" ) ;

        // Assert
        testLogger.Contains ( "Processing started" )
                  .Should ( )
                  .BeTrue ( ) ;

        testLogger.Contains ( "Found GattDeviceService with UUID" )
                  .Should ( )
                  .BeTrue ( ) ;

        testLogger.CountLinesContaining ( "Processing" )
                  .Should ( )
                  .Be ( 2 ) ;
    }

    [ TestMethod ]
    public void Example_WithMinimumLevel_OnlyCapturesSpecifiedLevel ( )
    {
        // Arrange - Only capture Warning and above
        using var testLogger = new LoggerForTests ( LogEventLevel.Warning ) ;

        // Act
        testLogger.Logger.Debug ( "This will not be captured" ) ;
        testLogger.Logger.Information ( "This will not be captured either" ) ;
        testLogger.Logger.Warning ( "This will be captured" ) ;
        testLogger.Logger.Error ( "This will also be captured" ) ;

        // Assert
        testLogger.Contains ( "This will not be captured" )
                  .Should ( )
                  .BeFalse ( ) ;

        testLogger.Contains ( "This will be captured" )
                  .Should ( )
                  .BeTrue ( ) ;

        testLogger.Lines.Length
                  .Should ( )
                  .Be ( 2 ) ;
    }

    [ TestMethod ]
    public void Example_InRealTest_WithSystemUnderTest ( )
    {
        // Arrange
        using var testLogger = new LoggerForTests ( ) ;

        // Example: Pass testLogger.Logger to your class under test
        // Then call methods on that class

        // Act
        testLogger.Logger.Information ( "Expected log message from system under test" ) ;

        // Assert - Verify specific log messages
        testLogger.Contains ( "expected log message" )
                  .Should ( )
                  .BeTrue ( "because the operation should log this message" ) ;

        // Or verify using LINQ on Lines
        testLogger.AnyLine ( line => line.Contains ( "Error" ) )
                  .Should ( )
                  .BeFalse ( "because no errors should occur" ) ;
    }

    [ TestMethod ]
    public void Example_ClearAndReuse_TestLogger ( )
    {
        // Arrange
        using var testLogger = new LoggerForTests ( ) ;

        // Act - First operation
        testLogger.Logger.Information ( "First operation" ) ;
        testLogger.Contains ( "First operation" )
                  .Should ( )
                  .BeTrue ( ) ;

        // Clear for next operation
        testLogger.Clear ( ) ;

        // Act - Second operation
        testLogger.Logger.Information ( "Second operation" ) ;

        // Assert - Only second operation should be in logs
        testLogger.Contains ( "First operation" )
                  .Should ( )
                  .BeFalse ( ) ;

        testLogger.Contains ( "Second operation" )
                  .Should ( )
                  .BeTrue ( ) ;
    }

    [ TestMethod ]
    public void Example_ExactMatching_CaseSensitive ( )
    {
        // Arrange
        using var testLogger = new LoggerForTests ( ) ;

        // Act
        testLogger.Logger.Information ( "Processing STARTED" ) ;

        // Assert
        testLogger.Contains ( "processing started" )
                  .Should ( )
                  .BeTrue ( "Contains is case-insensitive" ) ;

        testLogger.ContainsExact ( "processing started" )
                  .Should ( )
                  .BeFalse ( "ContainsExact is case-sensitive" ) ;

        testLogger.ContainsExact ( "Processing STARTED" )
                  .Should ( )
                  .BeTrue ( ) ;
    }

    [ TestMethod ]
    public void Example_CountOccurrences_OfLogMessage ( )
    {
        // Arrange
        using var testLogger = new LoggerForTests ( ) ;

        // Act
        for ( var i = 0 ; i < 5 ; i++ )
            testLogger.Logger.Debug ( "Processing item {Item}" ,
                                      i ) ;

        // Assert
        testLogger.CountLinesContaining ( "Processing item" )
                  .Should ( )
                  .Be ( 5 ) ;
    }
}
