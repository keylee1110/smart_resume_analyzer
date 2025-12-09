namespace TheAnalyzer.Models;

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

    /// <summary>
    /// The Job Description text used for this analysis.
    /// </summary>
    public string? JobDescription { get; set; }

    /// <summary>
    /// Job Title extracted from JD
    /// </summary>
    public string? JobTitle { get; set; }

    /// <summary>
    /// Company Name extracted from JD
    /// </summary>
    public string? Company { get; set; }

    /// <summary>
    /// Structured improvement plan from AI
    /// </summary>
    public List<ImprovementItem> ImprovementPlan { get; set; } = new();
}

/// <summary>
/// Represents a single improvement suggestion
/// </summary>
public class ImprovementItem
{
    public string Area { get; set; } = string.Empty;
    public string Advice { get; set; } = string.Empty;
}
