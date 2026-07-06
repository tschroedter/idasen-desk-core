using Serilog.Events ;

namespace Idasen.TestLogger ;

/// <summary>
///     Examples for using InMemoryLogger
/// </summary>
public static class InMemoryLoggerExample
{
    public static void Example ( )
    {
        // Create an in-memory logger
        var logger = new InMemoryLogger ( ) ;

        // Log messages using the Logger property
        logger.Logger.Information ( "This is an information message" ) ;
        logger.Logger.Warning ( "This is a warning message" ) ;
        logger.Logger.Error ( "This is an error message" ) ;

        // Get all output as a single string
        _ = logger.Output ;

        // Get all lines as an array
        _ = logger.Lines ;

        // Clear all logged messages
        logger.Clear ( ) ;

        // Dispose the logger
        logger.Dispose ( ) ;
    }

    public static void ConstructorWithMinimumLevelExample ( )
    {
        // Create logger that only captures Warning and above
        using var logger = new InMemoryLogger ( LogEventLevel.Warning ) ;

        logger.Logger.Debug ( "This won't be captured" ) ;
        logger.Logger.Information ( "This won't be captured either" ) ;
        logger.Logger.Warning ( "This will be captured" ) ;
        logger.Logger.Error ( "This will also be captured" ) ;

        _ = logger.Lines ; // Only contains Warning and Error messages
    }

    public static void LoggerPropertyExample ( )
    {
        using var logger = new InMemoryLogger ( ) ;

        // The Logger property provides a real Serilog ILogger
        // that can be passed to classes under test
        var serilogLogger = logger.Logger ;

        serilogLogger.Information ( "User {UserId} logged in" ,
                                    123 ) ;
        serilogLogger.Warning ( "Connection timeout after {Milliseconds}ms" ,
                                5000 ) ;

        // Verify the logged messages
        _ = logger.Contains ( "User 123" ) ;
    }

    public static void OutputPropertyExample ( )
    {
        using var logger = new InMemoryLogger ( ) ;

        logger.Logger.Information ( "First message" ) ;
        logger.Logger.Warning ( "Second message" ) ;

        // Get all captured output as a single string
        _ = logger.Output ;
        // output contains all log lines joined by Environment.NewLine
    }

    public static void LinesPropertyExample ( )
    {
        using var logger = new InMemoryLogger ( ) ;

        logger.Logger.Information ( "Line 1" ) ;
        logger.Logger.Warning ( "Line 2" ) ;
        logger.Logger.Error ( "Line 3" ) ;

        // Get all lines as an array
        var lines = logger.Lines ;
        // lines[0] = "[Information] Line 1"
        // lines[1] = "[Warning] Line 2"
        // lines[2] = "[Error] Line 3"

        _ = lines.Length ; // 3
    }

    public static void WriteMethodExample ( )
    {
        using var logger = new InMemoryLogger ( ) ;

        // Create a LogEvent manually and write it
        var logEvent = new LogEvent ( DateTimeOffset.Now ,
                                      LogEventLevel.Information ,
                                      null ,
                                      new MessageTemplate ( "Manual log event" ,
                                                            [] ) ,
                                      [] ) ;

        logger.Write ( logEvent ) ;

        _ = logger.Contains ( "Manual log event" ) ;
    }

    public static void ContainsExample ( )
    {
        using var logger = new InMemoryLogger ( ) ;

        logger.Logger.Information ( "User logged in" ) ;
        logger.Logger.Warning ( "Connection timeout" ) ;

        // Case-insensitive search
        _ = logger.Contains ( "user" ) ;      // true
        _ = logger.Contains ( "LOGGED IN" ) ; // true
        _ = logger.Contains ( "error" ) ;     // false
    }

    public static void DoesNotContainsExample ( )
    {
        using var logger = new InMemoryLogger ( ) ;

        logger.Logger.Information ( "User logged in" ) ;
        logger.Logger.Warning ( "Connection established" ) ;

        // Verify that certain text is NOT in the logs
        _ = logger.DoesNotContains ( "error" ) ;  // true
        _ = logger.DoesNotContains ( "user" ) ;   // false
        _ = logger.DoesNotContains ( "failed" ) ; // true
    }

    public static void ContainsWithTimesExample ( )
    {
        using var logger = new InMemoryLogger ( ) ;

        logger.Logger.Information ( "Processing item" ) ;
        logger.Logger.Information ( "Processing item" ) ;
        logger.Logger.Information ( "Processing item" ) ;
        logger.Logger.Warning ( "Processing failed" ) ;

        // Check if text appears exactly N times
        _ = logger.Contains ( "Processing item" ,
                              3 ) ; // true
        _ = logger.Contains ( "Processing item" ,
                              2 ) ; // false
        _ = logger.Contains ( "Processing failed" ,
                              1 ) ; // true
    }

    public static void ContainsWithLevelExample ( )
    {
        using var logger = new InMemoryLogger ( ) ;

        logger.Logger.Information ( "Connection established" ) ;
        logger.Logger.Warning ( "Connection timeout" ) ;
        logger.Logger.Error ( "Connection failed" ) ;

        // Check if text appears with specific log level
        _ = logger.Contains ( "Connection" ,
                              LogEventLevel.Warning ) ; // true
        _ = logger.Contains ( "Connection" ,
                              LogEventLevel.Error ) ; // true
        _ = logger.Contains ( "Connection" ,
                              LogEventLevel.Debug ) ; // false
        _ = logger.Contains ( "timeout" ,
                              LogEventLevel.Warning ) ; // true
        _ = logger.Contains ( "timeout" ,
                              LogEventLevel.Error ) ; // false
    }

    public static void ContainsExactExample ( )
    {
        using var logger = new InMemoryLogger ( ) ;

        logger.Logger.Information ( "User logged in" ) ;
        logger.Logger.Warning ( "user session started" ) ;

        // Case-sensitive search
        _ = logger.ContainsExact ( "User" ) ;   // true
        _ = logger.ContainsExact ( "user" ) ;   // true
        _ = logger.ContainsExact ( "USER" ) ;   // false (case sensitive!)
        _ = logger.ContainsExact ( "logged" ) ; // true
    }

    public static void AnyLineExample ( )
    {
        using var logger = new InMemoryLogger ( ) ;

        logger.Logger.Information ( "User 123 logged in" ) ;
        logger.Logger.Warning ( "User 456 login failed" ) ;
        logger.Logger.Error ( "Database error" ) ;

        // Check if any line matches a custom predicate
        _ = logger.AnyLine ( line => line.Length > 50 ) ;
        _ = logger.AnyLine ( line => line.Contains ( "User" ) ) ;      // true
        _ = logger.AnyLine ( line => line.Contains ( "123" ) ) ;       // true
        _ = logger.AnyLine ( line => line.StartsWith ( "[Error]" ) ) ; // true
    }

    public static void CountLinesContainingExample ( )
    {
        using var logger = new InMemoryLogger ( ) ;

        logger.Logger.Information ( "Processing item 1" ) ;
        logger.Logger.Information ( "Processing item 2" ) ;
        logger.Logger.Information ( "Processing item 3" ) ;
        logger.Logger.Warning ( "Processing failed" ) ;
        logger.Logger.Error ( "Database error" ) ;

        // Count how many lines contain specific text (case-insensitive)
        _ = logger.CountLinesContaining ( "Processing" ) ; // 4
        _ = logger.CountLinesContaining ( "item" ) ;       // 3
        _ = logger.CountLinesContaining ( "error" ) ;      // 1
        _ = logger.CountLinesContaining ( "user" ) ;       // 0
    }

    public static void ToStringExample ( )
    {
        using var logger = new InMemoryLogger ( ) ;

        logger.Logger.Information ( "First message" ) ;
        logger.Logger.Warning ( "Second message" ) ;
        logger.Logger.Error ( "Third message" ) ;

        // ToString() returns the same as Output property
        _ = logger.ToString ( ) ;
        // Contains all log lines as a single string

        // Useful for debugging or displaying in test output
        Console.WriteLine ( logger.ToString ( ) ) ;
    }

    public static void ContainsLevelExample ( )
    {
        using var logger = new InMemoryLogger ( ) ;

        logger.Logger.Information ( "Info message" ) ;
        logger.Logger.Warning ( "Warning message" ) ;

        // Check if any log entry has a specific level
        _ = logger.ContainsLevel ( LogEventLevel.Warning ) ;     // true
        _ = logger.ContainsLevel ( LogEventLevel.Error ) ;       // false
        _ = logger.ContainsLevel ( LogEventLevel.Information ) ; // true
        _ = logger.ContainsLevel ( LogEventLevel.Debug ) ;       // false
    }

    public static void ClearExample ( )
    {
        using var logger = new InMemoryLogger ( ) ;

        logger.Logger.Information ( "First message" ) ;
        logger.Logger.Warning ( "Second message" ) ;

        _ = logger.Lines.Length ; // 2

        // Clear all captured logs
        logger.Clear ( ) ;

        _ = logger.Lines.Length ; // 0
        _ = logger.Output ;       // empty string
    }

    public static void ComprehensiveExample ( )
    {
        // Create logger with minimum level
        using var logger = new InMemoryLogger ( LogEventLevel.Information ) ;

        // Log various messages
        logger.Logger.Debug ( "This won't be captured" ) ;
        logger.Logger.Information ( "User {UserId} logged in" ,
                                    123 ) ;
        logger.Logger.Information ( "Processing started" ) ;
        logger.Logger.Warning ( "Connection slow" ) ;
        logger.Logger.Error ( "Operation failed" ) ;

        // Verify using different methods
        _ = logger.Contains ( "User" ) ;
        _ = logger.Contains ( "User" ,
                              LogEventLevel.Information ) ;
        _ = logger.CountLinesContaining ( "Processing" ) ;
        _ = logger.Lines.Length == 4 ;
        _ = logger.ContainsLevel ( LogEventLevel.Error ) ;
        _ = logger.DoesNotContains ( "Debug" ) ;

        // Access output
        _ = logger.Output ;
        _ = logger.Lines ;

        // Clean up happens automatically with 'using'
    }
}
