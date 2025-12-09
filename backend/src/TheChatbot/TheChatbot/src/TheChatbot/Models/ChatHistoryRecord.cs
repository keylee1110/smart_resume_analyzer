using Amazon.DynamoDBv2.DataModel;

namespace TheChatbot.Models;

/// <summary>
/// DynamoDB record for storing chat history
/// Uses composite key: PK = RESUME#{resumeId}, SK = CHAT#{timestamp}
/// </summary>
[DynamoDBTable("ResumeAnalyzerTable")]
public class ChatHistoryRecord
{
    [DynamoDBHashKey("PK")]
    public string PK { get; set; } = string.Empty; // RESUME#{resumeId}
    
    [DynamoDBRangeKey("SK")]
    public string SK { get; set; } = string.Empty; // CHAT#{timestamp}
    
    [DynamoDBProperty("resumeId")]
    public string ResumeId { get; set; } = string.Empty;
    
    [DynamoDBProperty("timestamp")]
    public string Timestamp { get; set; } = string.Empty;
    
    [DynamoDBProperty("role")]
    public string Role { get; set; } = string.Empty; // "user" or "assistant"
    
    [DynamoDBProperty("content")]
    public string Content { get; set; } = string.Empty;
    
    [DynamoDBProperty("userId")]
    public string UserId { get; set; } = string.Empty;
}
