namespace TheChatbot.Models; // Namespace adjusted

/// <summary>
/// Detailed result of CV analysis and comparison
/// </summary>
public class AnalysisResult
{
    /// <summary>
    /// Calculated fit score (0-100)
    /// </summary>
    public double FitScore { get; set; }

    /// <summary>
    /// Skills found in both CV and Job Description
    /// </summary>
    public List<string> MatchedSkills { get; set; } = new();

    /// <summary>
    /// Skills found in Job Description but missing from CV
    /// </summary>
    public List<string> MissingSkills { get; set; } = new();

    /// <summary>
    /// Entities extracted from the Resume
    /// </summary>
    public ExtractedEntities ResumeEntities { get; set; } = new();

    /// <summary>
    /// AI recommendation or summary
    /// </summary>
    public string Recommendation { get; set; } = string.Empty;
    
    // Adding JobDescription to AnalysisResult for easier context passing
    public string? JobDescription { get; set; }
}
