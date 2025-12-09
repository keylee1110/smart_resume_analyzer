namespace TheParser.Exceptions;

/// <summary>
/// Base exception class for all Parser module errors
/// </summary>
public class ParserException : Exception
{
    /// <summary>
    /// Error code identifying the type of error
    /// </summary>
    public string ErrorCode { get; set; }
    
    /// <summary>
    /// Correlation ID for request tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Creates a new ParserException with a message and error code
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="errorCode">Error code</param>
    public ParserException(string message, string errorCode) 
        : base(message)
    {
        ErrorCode = errorCode;
    }
    
    /// <summary>
    /// Creates a new ParserException with a message, error code, and inner exception
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="errorCode">Error code</param>
    /// <param name="innerException">Inner exception</param>
    public ParserException(string message, string errorCode, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}

/// <summary>
/// Exception thrown when invoking the Analyzer Lambda function fails
/// </summary>
public class AnalyzerInvocationException : ParserException
{
    /// <summary>
    /// Creates a new AnalyzerInvocationException
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="innerException">Inner exception</param>
    public AnalyzerInvocationException(string message, Exception innerException)
        : base(message, "ANALYZER_INVOCATION_FAILED", innerException)
    {
    }
}
