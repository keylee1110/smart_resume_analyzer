namespace TheParser.Exceptions;

/// <summary>
/// Exception thrown when text extraction from a document fails
/// </summary>
public class TextExtractionException : ParserException
{
    /// <summary>
    /// Creates a new TextExtractionException with a message and inner exception
    /// </summary>
    /// <param name="message">Error message describing the extraction failure</param>
    /// <param name="innerException">The underlying exception that caused the failure</param>
    public TextExtractionException(string message, Exception innerException)
        : base($"Text extraction failed: {message}", "EXTRACTION_FAILED", innerException)
    {
    }
    
    /// <summary>
    /// Creates a new TextExtractionException with just a message
    /// </summary>
    /// <param name="message">Error message describing the extraction failure</param>
    public TextExtractionException(string message)
        : base($"Text extraction failed: {message}", "EXTRACTION_FAILED")
    {
    }
}
