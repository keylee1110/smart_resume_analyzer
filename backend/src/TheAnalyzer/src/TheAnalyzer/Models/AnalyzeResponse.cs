namespace TheAnalyzer.Models;

/// <summary>
/// Response model for CV analysis
/// </summary>
public class AnalyzeResponse
{
    /// <summary>
    /// Resume identifier
    /// </summary>
    public string ResumeId { get; set; } = string.Empty;
    
    /// <summary>
    /// Success or error message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Detailed analysis result
    /// </summary>
    public AnalysisResult? Analysis { get; set; }
}
