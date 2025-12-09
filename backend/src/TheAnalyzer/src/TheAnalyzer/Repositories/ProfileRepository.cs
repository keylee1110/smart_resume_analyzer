using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.Extensions.Logging;
using TheAnalyzer.Interfaces;
using TheAnalyzer.Models;

namespace TheAnalyzer.Repositories;

/// <summary>
/// Repository implementation for profile and analysis data persistence in DynamoDB
/// </summary>
public class ProfileRepository : IProfileRepository
{
    private readonly DynamoDBContext _context;
    private readonly ILogger<ProfileRepository> _logger;
    private readonly string _tableName;

    /// <summary>
    /// Initializes a new instance of the ProfileRepository
    /// </summary>
    /// <param name="dynamoDbClient">DynamoDB client</param>
    /// <param name="logger">Logger instance</param>
    public ProfileRepository(IAmazonDynamoDB dynamoDbClient, ILogger<ProfileRepository> logger)
    {
        _logger = logger;
        _tableName = Environment.GetEnvironmentVariable("TABLE_NAME") ?? "ResumeAnalyzerTable";
        
        var config = new DynamoDBContextConfig
        {
            TableNamePrefix = string.Empty,
            ConsistentRead = false // GSI queries only support eventual consistency
        };
        
        _context = new DynamoDBContext(dynamoDbClient, config);
        
        _logger.LogInformation("ProfileRepository initialized with table: {TableName}", _tableName);
    }

    /// <summary>
    /// Saves a profile record to DynamoDB
    /// </summary>
    public async Task SaveProfileAsync(ProfileRecord profile, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Saving profile for resume ID: {ResumeId}", profile.ResumeId);
            
            await _context.SaveAsync(profile, cancellationToken);
            
            _logger.LogInformation("Successfully saved profile for resume ID: {ResumeId}", profile.ResumeId);
        }
        catch (AmazonDynamoDBException ex)
        {
            _logger.LogError(ex, "DynamoDB error saving profile for resume ID: {ResumeId}", profile.ResumeId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error saving profile for resume ID: {ResumeId}", profile.ResumeId);
            throw;
        }
    }

    /// <summary>
    /// Retrieves a profile record by ID
    /// </summary>
    public async Task<ProfileRecord?> GetProfileAsync(string resumeId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving profile for resume ID: {ResumeId}", resumeId);
            
            var pk = $"RESUME#{resumeId}";
            var sk = "PROFILE";
            
            var profile = await _context.LoadAsync<ProfileRecord>(pk, sk, cancellationToken);
            
            if (profile == null)
            {
                _logger.LogInformation("Profile not found for resume ID: {ResumeId}", resumeId);
            }
            else
            {
                // Ensure ResumeId is set from PK if not already set
                if (string.IsNullOrEmpty(profile.ResumeId) && !string.IsNullOrEmpty(profile.PK))
                {
                    profile.ResumeId = profile.PK.Replace("RESUME#", "");
                }
                _logger.LogInformation("Successfully retrieved profile for resume ID: {ResumeId}", resumeId);
            }
            
            return profile;
        }
        catch (AmazonDynamoDBException ex)
        {
            _logger.LogError(ex, "DynamoDB error retrieving profile for resume ID: {ResumeId}", resumeId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving profile for resume ID: {ResumeId}", resumeId);
            throw;
        }
    }

    /// <summary>
    /// Saves an analysis record to DynamoDB
    /// </summary>
    public async Task SaveAnalysisAsync(AnalysisRecord analysis, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Saving analysis for resume ID: {ResumeId}", analysis.ResumeId);
            
            await _context.SaveAsync(analysis, cancellationToken);
            
            _logger.LogInformation("Successfully saved analysis for resume ID: {ResumeId}", analysis.ResumeId);
        }
        catch (AmazonDynamoDBException ex)
        {
            _logger.LogError(ex, "DynamoDB error saving analysis for resume ID: {ResumeId}", analysis.ResumeId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error saving analysis for resume ID: {ResumeId}", analysis.ResumeId);
            throw;
        }
    }

    /// <summary>
    /// Retrieves all profiles for a specific user using GSI-User index, with optional sorting
    /// </summary>
    public async Task<IEnumerable<ProfileRecord>> GetProfilesByUserAsync(
        string userId,
        string sortBy = "createdAt", // Default sort by creation date
        string order = "desc",      // Default descending order
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving profiles for user ID: {UserId} with sortBy: {SortBy}, order: {Order}", userId, sortBy, order);
            
            var queryConfig = new DynamoDBOperationConfig
            {
                IndexName = "GSI-User",
                ConsistentRead = false // GSIs only support eventual consistency
            };
            
            var search = _context.QueryAsync<ProfileRecord>(userId, queryConfig);
            var profiles = await search.GetRemainingAsync(cancellationToken);
            
            // Ensure ResumeId is set from PK for each profile
            foreach (var profile in profiles)
            {
                if (string.IsNullOrEmpty(profile.ResumeId) && !string.IsNullOrEmpty(profile.PK))
                {
                    profile.ResumeId = profile.PK.Replace("RESUME#", "");
                }
            }

            // Apply in-memory sorting
            IOrderedEnumerable<ProfileRecord> orderedProfiles;
            switch (sortBy.ToLowerInvariant())
            {
                case "fitscore":
                    orderedProfiles = order.ToLowerInvariant() == "asc"
                        ? profiles.OrderBy(p => p.LastAnalysis?.FitScore ?? 0)
                        : profiles.OrderByDescending(p => p.LastAnalysis?.FitScore ?? 0);
                    break;
                case "createdat": // Assuming createdAt is available in ProfileRecord
                default:
                    orderedProfiles = order.ToLowerInvariant() == "asc"
                        ? profiles.OrderBy(p => p.CreatedAt)
                        : profiles.OrderByDescending(p => p.CreatedAt);
                    break;
            }
            
            _logger.LogInformation("Successfully retrieved and sorted {Count} profiles for user ID: {UserId}", profiles.Count, userId);
            
            return orderedProfiles;
        }
        catch (AmazonDynamoDBException ex)
        {
            _logger.LogError(ex, "DynamoDB error retrieving profiles for user ID: {UserId}", userId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving profiles for user ID: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Retrieves a profile record by S3 object key
    /// </summary>
    public async Task<ProfileRecord?> GetProfileByS3KeyAsync(string s3Key, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving profile for S3 key: {S3Key}", s3Key);
            
            // Use ScanCondition to filter on DynamoDB side instead of client side
            // This is still a SCAN operation but transfers less data
            var scanConditions = new List<ScanCondition>
            {
                new ScanCondition("S3Key", ScanOperator.Equal, s3Key)
            };
            
            var search = _context.ScanAsync<ProfileRecord>(scanConditions);
            var profiles = await search.GetRemainingAsync(cancellationToken);
            var profile = profiles.FirstOrDefault();
            
            // Fallback: If strict match fails, try decoding/encoding or partial match
            if (profile == null)
            {
                _logger.LogWarning("Strict match failed for S3Key: {S3Key}. Attempting fallback search.", s3Key);
                
                // Try searching with URL decoded key if the original input might be encoded
                var decodedKey = System.Net.WebUtility.UrlDecode(s3Key);
                if (decodedKey != s3Key)
                {
                    _logger.LogInformation("Trying decoded key: {DecodedKey}", decodedKey);
                    var decodedConditions = new List<ScanCondition>
                    {
                        new ScanCondition("S3Key", ScanOperator.Equal, decodedKey)
                    };
                    var decodedSearch = _context.ScanAsync<ProfileRecord>(decodedConditions);
                    var decodedProfiles = await decodedSearch.GetRemainingAsync(cancellationToken);
                    profile = decodedProfiles.FirstOrDefault();
                }
            }

            if (profile == null)
            {
                _logger.LogInformation("Profile not found for S3 key: {S3Key}", s3Key);
            }
            else
            {
                // Ensure ResumeId is set from PK if not already set
                if (string.IsNullOrEmpty(profile.ResumeId) && !string.IsNullOrEmpty(profile.PK))
                {
                    profile.ResumeId = profile.PK.Replace("RESUME#", "");
                }
                _logger.LogInformation("Successfully retrieved profile for S3 key: {S3Key}. ResumeID: {ResumeId}", s3Key, profile.ResumeId);
            }
            
            return profile;
        }
        catch (AmazonDynamoDBException ex)
        {
            _logger.LogError(ex, "DynamoDB error retrieving profile for S3 key: {S3Key}", s3Key);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving profile for S3 key: {S3Key}", s3Key);
            throw;
        }
    }
}
