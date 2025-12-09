using Amazon.DynamoDBv2.DataModel;

namespace TheAnalyzer.Models;

/// <summary>
/// DynamoDB entity representing an analysis operation
/// </summary>
[DynamoDBTable("ResumeAnalyzerTable")]
public class AnalysisRecord
{
    /// <summary>
    /// Partition key: RESUME#{ResumeId}
    /// </summary>
    [DynamoDBHashKey("PK")]
    public string PK { get; set; } = string.Empty;
    
    /// <summary>
    /// Sort key: ANALYSIS#{Timestamp}
    /// </summary>
    [DynamoDBRangeKey("SK")]
    public string SK { get; set; } = string.Empty;
    
    /// <summary>
    /// Entity type identifier
    /// </summary>
    [DynamoDBProperty]
    public string EntityType { get; set; } = "Analysis";
    
    /// <summary>
    /// Resume identifier
    /// </summary>
    [DynamoDBProperty]
    public string ResumeId { get; set; } = string.Empty;
    
    /// <summary>
    /// Analysis timestamp
    /// </summary>
    [DynamoDBProperty]
    public DateTime AnalysisTimestamp { get; set; }
    
    /// <summary>
    /// Extraction method used: "Comprehend" or "Regex"
    /// </summary>
    [DynamoDBProperty]
    public string ExtractionMethod { get; set; } = string.Empty;
    
    /// <summary>
    /// Number of entities found
    /// </summary>
    [DynamoDBProperty]
    public int EntityCount { get; set; }
    
    /// <summary>
    /// Creation timestamp
    /// </summary>
    [DynamoDBProperty]
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Helper method to initialize keys before saving
    /// </summary>
    public void InitializeKeys()
    {
        PK = $"RESUME#{ResumeId}";
        SK = $"ANALYSIS#{AnalysisTimestamp:O}";
        EntityType = "Analysis";
    }
}
