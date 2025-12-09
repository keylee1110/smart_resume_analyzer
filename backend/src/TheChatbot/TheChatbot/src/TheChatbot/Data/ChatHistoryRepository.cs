using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Microsoft.Extensions.Logging;
using TheChatbot.Models;

namespace TheChatbot.Data;

public class ChatHistoryRepository : IChatHistoryRepository
{
    private readonly DynamoDBContext _context;
    private readonly ILogger<ChatHistoryRepository> _logger;
    private readonly string _tableName;

    public ChatHistoryRepository(IAmazonDynamoDB dynamoDbClient, ILogger<ChatHistoryRepository> logger)
    {
        _context = new DynamoDBContext(dynamoDbClient);
        _logger = logger;
        _tableName = Environment.GetEnvironmentVariable("TABLE_NAME") ?? "ProfilesTable";
    }

    /// <summary>
    /// Saves a chat message to DynamoDB
    /// </summary>
    public async Task SaveMessageAsync(
        string resumeId, 
        ChatMessage message, 
        string userId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(resumeId))
        {
            throw new ArgumentException("ResumeId cannot be null or empty", nameof(resumeId));
        }

        if (message == null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("UserId cannot be null or empty", nameof(userId));
        }

        try
        {
            // Generate timestamp if not provided
            var timestamp = string.IsNullOrWhiteSpace(message.Timestamp) 
                ? DateTime.UtcNow.ToString("o") 
                : message.Timestamp;

            var record = new ChatHistoryRecord
            {
                PK = $"RESUME#{resumeId}",
                SK = $"CHAT#{timestamp}",
                ResumeId = resumeId,
                Timestamp = timestamp,
                Role = message.Role,
                Content = message.Content,
                UserId = userId
            };

            _logger.LogInformation($"Saving chat message for resume {resumeId}, role: {message.Role}");
            await _context.SaveAsync(record, cancellationToken);
            _logger.LogInformation($"Successfully saved chat message for resume {resumeId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error saving chat message for resume {resumeId}");
            throw;
        }
    }

    /// <summary>
    /// Retrieves all chat messages for a specific resume in chronological order
    /// </summary>
    public async Task<List<ChatMessage>> GetHistoryAsync(
        string resumeId, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(resumeId))
        {
            throw new ArgumentException("ResumeId cannot be null or empty", nameof(resumeId));
        }

        try
        {
            _logger.LogInformation($"Retrieving chat history for resume {resumeId}");

            // Query all items with PK = RESUME#{resumeId} and SK starting with CHAT#
            var queryConfig = new QueryOperationConfig
            {
                KeyExpression = new Expression
                {
                    ExpressionStatement = "PK = :pk AND begins_with(SK, :sk)",
                    ExpressionAttributeValues = new Dictionary<string, DynamoDBEntry>
                    {
                        { ":pk", $"RESUME#{resumeId}" },
                        { ":sk", "CHAT#" }
                    }
                },
                ConsistentRead = false // Eventually consistent is fine for chat history
            };

            var table = _context.GetTargetTable<ChatHistoryRecord>();
            var search = table.Query(queryConfig);

            var records = new List<ChatHistoryRecord>();
            do
            {
                var documents = await search.GetNextSetAsync(cancellationToken);
                var batch = _context.FromDocuments<ChatHistoryRecord>(documents);
                records.AddRange(batch);
            } while (!search.IsDone);

            // Convert to ChatMessage and sort by timestamp (SK already ensures chronological order)
            var messages = records
                .OrderBy(r => r.Timestamp)
                .Select(r => new ChatMessage
                {
                    Role = r.Role,
                    Content = r.Content,
                    Timestamp = r.Timestamp
                })
                .ToList();

            _logger.LogInformation($"Retrieved {messages.Count} chat messages for resume {resumeId}");
            return messages;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving chat history for resume {resumeId}");
            throw;
        }
    }
}
