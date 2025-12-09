using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using TheChatbot.Models;
using System.Threading.Tasks;
using System.Threading;

namespace TheChatbot.Data;

public class ProfileRepository : IProfileRepository
{
    private readonly IAmazonDynamoDB _dynamoDbClient;
    private readonly ILogger<ProfileRepository> _logger;
    private readonly string _tableName;

    public ProfileRepository(IAmazonDynamoDB dynamoDbClient, IConfiguration configuration, ILogger<ProfileRepository> logger)
    {
        _dynamoDbClient = dynamoDbClient;
        _logger = logger;
        _tableName = Environment.GetEnvironmentVariable("TABLE_NAME") ?? configuration["TABLE_NAME"] ?? throw new ArgumentNullException("TABLE_NAME environment variable is not set.");
    }

    public async Task<ProfileRecord?> GetProfileAsync(string resumeId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation($"Retrieving profile for resume ID: {resumeId}");
        
        var config = new DynamoDBContextConfig
        {
            DisableFetchingTableMetadata = true
        };
        var context = new DynamoDBContext(_dynamoDbClient, config);
        
        // DynamoDB PK for ProfileRecord is RESUME#{ResumeId}
        var pk = $"RESUME#{resumeId}";
        var sk = "PROFILE"; // SK for profile record is PROFILE

        try
        {
            var profile = await context.LoadAsync<ProfileRecord>(pk, sk, cancellationToken);
            if (profile != null)
            {
                _logger.LogInformation($"Successfully retrieved profile for resume ID: {resumeId}");
            }
            else
            {
                _logger.LogWarning($"Profile not found for resume ID: {resumeId}");
            }
            return profile;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving profile for resume ID {resumeId}: {ex.Message}");
            throw;
        }
    }
}
