namespace TheAnalyzer.Exceptions;

/// <summary>
/// Exception thrown when input validation fails
/// </summary>
public class ValidationException : Exception
{
    /// <summary>
    /// HTTP status code for the validation error
    /// </summary>
    public int StatusCode { get; }
    
    /// <summary>
    /// Detailed validation error messages
    /// </summary>
    public List<string> Errors { get; }
    
    public ValidationException(string message, int statusCode = 400) 
        : base(message)
    {
        StatusCode = statusCode;
        Errors = new List<string> { message };
    }
    
    public ValidationException(List<string> errors, int statusCode = 400) 
        : base(string.Join("; ", errors))
    {
        StatusCode = statusCode;
        Errors = errors;
    }
}
