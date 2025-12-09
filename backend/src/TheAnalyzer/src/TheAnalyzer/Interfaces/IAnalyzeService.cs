using TheAnalyzer.Models;

namespace TheAnalyzer.Interfaces;

/// <summary>
/// Service interface for analyzing CV text and extracting entities
/// </summary>
public interface IAnalyzeService
{
    /// <summary>
    /// Extracts entities from CV text using AWS Comprehend or regex fallback
    /// </summary>
    /// <param name="cvText">The raw CV text to analyze</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Extracted entities including name, email, phone, and skills</returns>
    Task<ExtractedEntities> ExtractEntitiesAsync(string cvText, CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyzes CV text against an optional job description
    /// </summary>
    /// <param name="cvText">The raw CV text</param>
    /// <param name="jobDescription">The optional job description text</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Comprehensive analysis result including fit score</returns>
    Task<AnalysisResult> AnalyzeCvAsync(string cvText, string? jobDescription, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Validates that CV text is suitable for processing
    /// </summary>
    /// <param name="cvText">The CV text to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    bool ValidateInput(string cvText);
}
