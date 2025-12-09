namespace TheParser.Interfaces;

/// <summary>
/// Service for normalizing and cleaning extracted text
/// </summary>
public interface ITextNormalizer
{
    /// <summary>
    /// Normalizes extracted text by cleaning whitespace and formatting
    /// </summary>
    /// <param name="rawText">Raw extracted text</param>
    /// <returns>Normalized text</returns>
    string Normalize(string rawText);
    
    /// <summary>
    /// Validates that normalized text is not empty or whitespace-only
    /// </summary>
    /// <param name="text">Text to validate</param>
    /// <returns>True if text is valid</returns>
    bool IsValid(string text);
}
