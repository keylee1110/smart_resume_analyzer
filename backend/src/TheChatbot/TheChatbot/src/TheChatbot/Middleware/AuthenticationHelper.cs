using Amazon.Lambda.APIGatewayEvents;
using Microsoft.Extensions.Logging;

namespace TheChatbot.Middleware;

/// <summary>
/// Helper class for authentication and authorization operations
/// Extracts and validates Cognito JWT tokens from API Gateway requests
/// Requirements: 1.4, 12.4, 13.3
/// </summary>
public static class AuthenticationHelper
{
    /// <summary>
    /// Extracts user ID from Cognito JWT token claims
    /// </summary>
    /// <param name="request">API Gateway proxy request</param>
    /// <returns>User ID (sub claim) if found, null otherwise</returns>
    public static string? ExtractUserId(APIGatewayProxyRequest request)
    {
        if (request?.RequestContext?.Authorizer?.Claims != null &&
            request.RequestContext.Authorizer.Claims.TryGetValue("sub", out var cognitoUserId))
        {
            return cognitoUserId;
        }

        return null;
    }

    /// <summary>
    /// Validates that the authenticated user owns the specified resume
    /// </summary>
    /// <param name="userId">Authenticated user ID from JWT</param>
    /// <param name="profileUserId">User ID from the profile record</param>
    /// <returns>True if user owns the resume, false otherwise</returns>
    public static bool ValidateOwnership(string? userId, string? profileUserId)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(profileUserId))
        {
            return false;
        }

        return userId == profileUserId;
    }

    /// <summary>
    /// Validates authentication and logs appropriate messages
    /// </summary>
    /// <param name="request">API Gateway proxy request</param>
    /// <param name="logger">Logger instance</param>
    /// <returns>Tuple of (isValid, userId)</returns>
    public static (bool IsValid, string? UserId) ValidateAuthentication(
        APIGatewayProxyRequest request,
        ILogger logger)
    {
        var userId = ExtractUserId(request);
        
        if (string.IsNullOrWhiteSpace(userId))
        {
            logger.LogWarning("Authentication failed: No user ID found in Cognito JWT claims");
            return (false, null);
        }

        logger.LogInformation($"User authenticated: {userId}");
        return (true, userId);
    }

    /// <summary>
    /// Validates that the Authorization header is present
    /// </summary>
    /// <param name="request">API Gateway proxy request</param>
    /// <returns>True if Authorization header exists, false otherwise</returns>
    public static bool HasAuthorizationHeader(APIGatewayProxyRequest request)
    {
        if (request?.Headers == null)
        {
            return false;
        }

        // Check for Authorization header (case-insensitive)
        return request.Headers.Any(h => 
            string.Equals(h.Key, "Authorization", StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrWhiteSpace(h.Value));
    }

    /// <summary>
    /// Extracts the JWT token from the Authorization header
    /// </summary>
    /// <param name="request">API Gateway proxy request</param>
    /// <returns>JWT token string if found, null otherwise</returns>
    public static string? ExtractToken(APIGatewayProxyRequest request)
    {
        if (request?.Headers == null)
        {
            return null;
        }

        var authHeader = request.Headers.FirstOrDefault(h =>
            string.Equals(h.Key, "Authorization", StringComparison.OrdinalIgnoreCase));

        if (string.IsNullOrWhiteSpace(authHeader.Value))
        {
            return null;
        }

        // Remove "Bearer " prefix if present
        var token = authHeader.Value;
        if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            token = token.Substring(7);
        }

        return token;
    }
}
