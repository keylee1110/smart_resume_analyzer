using Amazon.DynamoDBv2.DataModel;

namespace TheChatbot.Models; // Namespace adjusted

/// <summary>
/// DynamoDB entity representing a candidate profile
/// </summary>
[DynamoDBTable("ResumeAnalyzerTable")]
public class ProfileRecord
{
    /// <summary>
    /// Partition key: RESUME#{ResumeId}
    /// </summary>
    [DynamoDBHashKey("PK")]
    public string PK { get; set; } = string.Empty;
    
    /// <summary>
    /// Sort key: PROFILE
    /// </summary>
    [DynamoDBRangeKey("SK")]
    public string SK { get; set; } = "PROFILE";
    
    /// <summary>
    /// Entity type identifier
    /// </summary>
    [DynamoDBProperty]
    public string EntityType => "Profile";
    
    /// <summary>
    /// Resume identifier
    /// </summary>
    [DynamoDBProperty]
    public string ResumeId { get; set; } = string.Empty;
    
    /// <summary>
    /// Candidate name
    /// </summary>
    [DynamoDBProperty]
    public string? Name { get; set; }
    
    /// <summary>
    /// Email address
    /// </summary>
    [DynamoDBProperty]
    public string? Email { get; set; }
    
    /// <summary>
    /// Phone number
    /// </summary>
    [DynamoDBProperty]
    public string? Phone { get; set; }
    
    /// <summary>
    /// List of skills
    /// </summary>
    [DynamoDBProperty]
    public List<string> Skills { get; set; } = new();
    
    /// <summary>
    /// Creation timestamp
    /// </summary>
    [DynamoDBProperty]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Full Resume Text for re-analysis
    /// </summary>
    [DynamoDBProperty]
    public string? ResumeText { get; set; }

    /// <summary>
    /// Result of the most recent analysis/comparison
    /// </summary>
    [DynamoDBProperty]
    public AnalysisResult? LastAnalysis { get; set; }

    /// <summary>
    /// User identifier (GSI Partition Key)
    /// </summary>
    [DynamoDBGlobalSecondaryIndexHashKey("GSI-User")]
    public string? UserId { get; set; }

    /// <summary>
    /// GSI Sort Key: User specific timestamp
    /// </summary>
    [DynamoDBGlobalSecondaryIndexRangeKey("GSI-User")]
    public string? UserTimestamp { get; set; }
    
    /// <summary>
    /// S3 object key for the resume file
    /// </summary>
    [DynamoDBProperty]
    public string? S3Key { get; set; }
    
    /// <summary>
    /// Helper method to initialize keys before saving
    /// </summary>
    public void InitializeKeys()
    {
        PK = $"RESUME#{ResumeId}";
        SK = "PROFILE";
        UserTimestamp = CreatedAt.ToString("o");
    }
}
