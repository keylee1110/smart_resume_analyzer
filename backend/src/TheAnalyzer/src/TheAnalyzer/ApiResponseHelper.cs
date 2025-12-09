using Amazon.Lambda.APIGatewayEvents;
using System.Text.Json;

namespace TheAnalyzer;

/// <summary>
/// Utility class for creating standardized API Gateway responses with CORS headers
/// </summary>
public static class ApiResponseHelper
{
    /// <summary>
    /// Creates a standardized API Gateway response with CORS headers
    /// </summary>
    /// <param name="statusCode">HTTP status code</param>
    /// <param name="body">Response body object to be serialized to JSON</param>
    /// <param name="additionalHeaders">Optional additional headers to include</param>
    /// <returns>APIGatewayProxyResponse with CORS headers</returns>
    public static APIGatewayProxyResponse CreateResponse(
        int statusCode,
        object body,
        Dictionary<string, string>? additionalHeaders = null)
    {
        var headers = new Dictionary<string, string>
        {
            { "Content-Type", "application/json" },
            { "Access-Control-Allow-Origin", "*" },
            { "Access-Control-Allow-Headers", "Content-Type,X-Amz-Date,Authorization,X-Api-Key,X-Amz-Security-Token,X-Amz-User-Agent" },
            { "Access-Control-Allow-Methods", "GET,POST,PUT,DELETE,OPTIONS" }
        };

        // Add any additional headers
        if (additionalHeaders != null)
        {
            foreach (var header in additionalHeaders)
            {
                headers[header.Key] = header.Value;
            }
        }

        return new APIGatewayProxyResponse
        {
            StatusCode = statusCode,
            Body = JsonSerializer.Serialize(body, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            }),
            Headers = headers
        };
    }

    /// <summary>
    /// Creates a standardized error response with CORS headers
    /// </summary>
    /// <param name="statusCode">HTTP status code (typically 4xx or 5xx)</param>
    /// <param name="message">Error message to return to the client</param>
    /// <param name="errorType">Type of error (e.g., "ValidationError", "NotFoundError", "InternalError")</param>
    /// <returns>APIGatewayProxyResponse with error structure and CORS headers</returns>
    public static APIGatewayProxyResponse CreateErrorResponse(
        int statusCode,
        string message,
        string errorType = "Error")
    {
        var errorBody = new
        {
            ErrorType = errorType,
            Message = message,
            StatusCode = statusCode
        };

        return CreateResponse(statusCode, errorBody);
    }
}
