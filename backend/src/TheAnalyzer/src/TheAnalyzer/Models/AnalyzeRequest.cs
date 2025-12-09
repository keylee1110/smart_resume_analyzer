namespace TheAnalyzer.Models;

/// <summary>
/// Request model for CV analysis
/// </summary>
public class AnalyzeRequest
{
    /// <summary>
    /// Resume identifier (optional, will be generated if not provided)
    /// </summary>
    public string? ResumeId { get; set; }
    
    /// <summary>
    /// Raw CV text to analyze
    /// </summary>
    public string ResumeText { get; set; } = string.Empty;

    /// <summary>
    /// Job description text for comparison (optional)
    /// </summary>
    public string? JobDescription { get; set; }

    /// <summary>
    /// User identifier from Cognito
    /// </summary>
    public string? UserId { get; set; }
}
