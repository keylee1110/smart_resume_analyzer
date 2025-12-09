namespace TheParser.Models;

/// <summary>
/// Payload sent to the Analyzer Lambda function containing extracted resume text and metadata
/// </summary>
public class AnalyzerInvocationPayload
{
    /// <summary>
    /// Resume Identifier (matches S3 Key)
    /// </summary>
    public string? ResumeId { get; set; }

    /// <summary>
    /// The extracted and normalized resume text
    /// </summary>
    public string ResumeText { get; set; } = string.Empty;
    
    /// <summary>
    /// S3 bucket name where the original file is stored
    /// </summary>
    public string SourceBucket { get; set; } = string.Empty;
    
    /// <summary>
    /// S3 object key of the original file
    /// </summary>
    public string SourceKey { get; set; } = string.Empty;
    
    /// <summary>
    /// File type as a string (e.g., "Pdf", "Docx")
    /// </summary>
    public string FileType { get; set; } = string.Empty;
    
    /// <summary>
    /// Timestamp when extraction occurred
    /// </summary>
    public DateTime ExtractedAt { get; set; }
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string CorrelationId { get; set; } = string.Empty;

    /// <summary>
    /// ID of the user who uploaded the resume
    /// </summary>
    public string? UserId { get; set; }
}
