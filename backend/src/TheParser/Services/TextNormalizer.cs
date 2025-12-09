using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using TheParser.Interfaces;

namespace TheParser.Services;

/// <summary>
/// Service for normalizing and cleaning extracted text from resumes
/// </summary>
public class TextNormalizer : ITextNormalizer
{
    private readonly ILogger<TextNormalizer> _logger;
    
    // Regex to match multiple consecutive whitespace characters (spaces, tabs)
    private static readonly Regex ExcessiveWhitespaceRegex = new Regex(@"[ \t]+", RegexOptions.Compiled);
    
    // Regex to match multiple consecutive newlines (more than 2)
    private static readonly Regex ExcessiveNewlinesRegex = new Regex(@"\n{3,}", RegexOptions.Compiled);

    /// <summary>
    /// Creates a new TextNormalizer instance
    /// </summary>
    /// <param name="logger">Logger instance</param>
    public TextNormalizer(ILogger<TextNormalizer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Normalizes extracted text by cleaning whitespace and formatting
    /// Requirements: 5.1, 5.2, 5.3, 5.4
    /// </summary>
    /// <param name="rawText">Raw extracted text</param>
    /// <returns>Normalized text</returns>
    public string Normalize(string rawText)
    {
        if (string.IsNullOrEmpty(rawText))
        {
            _logger.LogWarning("Received null or empty text for normalization");
            return string.Empty;
        }

        _logger.LogInformation(
            "Starting text normalization. RawTextLength: {RawLength} characters",
            rawText.Length);

        // Step 1: Normalize line endings to Unix format (\n)
        // Handle Windows (\r\n) and old Mac (\r) line endings
        string normalized = rawText.Replace("\r\n", "\n").Replace("\r", "\n");

        // Step 2: Remove excessive whitespace (multiple spaces/tabs -> single space)
        // This preserves meaningful formatting like bullet points
        normalized = ExcessiveWhitespaceRegex.Replace(normalized, " ");

        // Step 3: Reduce excessive newlines (more than 2 consecutive -> 2)
        // This preserves section breaks (double newlines) but removes excessive spacing
        normalized = ExcessiveNewlinesRegex.Replace(normalized, "\n\n");

        // Step 4: Trim leading and trailing whitespace from the entire text
        normalized = normalized.Trim();

        // Step 5: Clean up lines - trim each line but preserve structure
        var lines = normalized.Split('\n');
        var cleanedLines = lines.Select(line => line.TrimEnd()).ToArray();
        normalized = string.Join('\n', cleanedLines);

        _logger.LogInformation(
            "Text normalization completed. RawLength: {RawLength}, NormalizedLength: {NormalizedLength}, LineCount: {LineCount}",
            rawText.Length,
            normalized.Length,
            cleanedLines.Length);

        return normalized;
    }

    /// <summary>
    /// Validates that normalized text is not empty or whitespace-only
    /// Requirement: 5.5
    /// </summary>
    /// <param name="text">Text to validate</param>
    /// <returns>True if text is valid (not empty or whitespace-only)</returns>
    public bool IsValid(string text)
    {
        var isValid = !string.IsNullOrWhiteSpace(text);
        
        if (!isValid)
        {
            _logger.LogWarning("Text validation failed: text is null, empty, or whitespace-only");
        }
        
        return isValid;
    }
}
