namespace TheAnalyzer.Models;

/// <summary>
/// Structured validation error response
/// </summary>
public class ValidationErrorResponse
{
    /// <summary>
    /// Error message
    /// </summary>
    public string Error { get; set; } = string.Empty;
    
    /// <summary>
    /// List of validation errors
    /// </summary>
    public List<string> ValidationErrors { get; set; } = new();
    
    /// <summary>
    /// HTTP status code
    /// </summary>
    public int StatusCode { get; set; }
}
