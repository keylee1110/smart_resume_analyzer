using TheAnalyzer.Models;

namespace TheAnalyzer.Interfaces;

/// <summary>
/// Repository interface for profile and analysis data persistence
/// </summary>
public interface IProfileRepository
{
    /// <summary>
    /// Saves a profile record to DynamoDB
    /// </summary>
    /// <param name="profile">The profile record to save</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SaveProfileAsync(ProfileRecord profile, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Retrieves a profile record by ID
    /// </summary>
    /// <param name="resumeId">The resume identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The profile record, or null if not found</returns>
    Task<ProfileRecord?> GetProfileAsync(string resumeId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Saves an analysis record to DynamoDB
    /// </summary>
    /// <param name="analysis">The analysis record to save</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SaveAnalysisAsync(AnalysisRecord analysis, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all profiles for a specific user
    /// </summary>
    /// <param name="userId">The user identifier</param>
    /// <param name="sortBy">Field to sort by (e.g., "createdAt", "fitScore")</param>
    /// <param name="order">Sort order ("asc" or "desc")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of profiles</returns>
    Task<IEnumerable<ProfileRecord>> GetProfilesByUserAsync(string userId, string sortBy = "createdAt", string order = "desc", CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a profile record by S3 object key
    /// </summary>
    /// <param name="s3Key">The S3 object key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The profile record, or null if not found</returns>
    Task<ProfileRecord?> GetProfileByS3KeyAsync(string s3Key, CancellationToken cancellationToken = default);
}
