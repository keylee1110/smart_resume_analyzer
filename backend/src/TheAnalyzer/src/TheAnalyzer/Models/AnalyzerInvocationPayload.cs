namespace TheAnalyzer.Models;

/// <summary>
/// Payload received from TheParser Lambda function
/// </summary>
public class AnalyzerInvocationPayload
{
    public string? ResumeId { get; set; }
    public string ResumeText { get; set; } = string.Empty;
    public string SourceBucket { get; set; } = string.Empty;
    public string SourceKey { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public DateTime ExtractedAt { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public string? UserId { get; set; }
}
