using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TheChatbot.Models;
using TheChatbot.Data;
using TheChatbot.Middleware;

namespace TheChatbot.Handlers;

/// <summary>
/// Lambda function handler for GET /chat/history/{resumeId} endpoint
/// Retrieves chat history for a specific resume analysis session
/// </summary>
public class GetChatHistoryLambdaFunction
{
    private readonly ILogger<GetChatHistoryLambdaFunction>? _logger;
    private readonly IChatHistoryRepository? _chatHistoryRepository;
    private readonly IProfileRepository? _profileRepository;
    private static Exception? _initException;

    public GetChatHistoryLambdaFunction()
    {
        try
        {
            var serviceProvider = new ServiceCollection()
                .ConfigureServices()
                .BuildServiceProvider();

            _logger = serviceProvider.GetRequiredService<ILogger<GetChatHistoryLambdaFunction>>();
            _chatHistoryRepository = serviceProvider.GetRequiredService<IChatHistoryRepository>();
            _profileRepository = serviceProvider.GetRequiredService<IProfileRepository>();
        }
        catch (Exception ex)
        {
            _initException = ex;
            Console.WriteLine($"[CRITICAL] ChatHistory Constructor Initialization Failed: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }

    public GetChatHistoryLambdaFunction(
        ILogger<GetChatHistoryLambdaFunction> logger,
        IChatHistoryRepository chatHistoryRepository,
        IProfileRepository profileRepository)
    {
        _logger = logger;
        _chatHistoryRepository = chatHistoryRepository;
        _profileRepository = profileRepository;
    }

    /// <summary>
    /// Handles GET /chat/history/{resumeId} requests
    /// Requirements: 3.3, 3.5, 5.2, 12.4
    /// </summary>
    public async Task<APIGatewayProxyResponse> FunctionHandler(
        APIGatewayProxyRequest request,
        ILambdaContext context)
    {
        if (_initException != null)
        {
            context.Logger.LogError($"[CRITICAL] Function initialization failed previously: {_initException.Message}");
            context.Logger.LogError(_initException.StackTrace);
            
            return CreateChatErrorResponse(500, $"Internal Server Error (Init): {_initException.Message}");
        }
        
        // If _initException is null, then _logger, _chatHistoryRepository, and _profileRepository must have been initialized
        _logger!.LogInformation("GetChatHistoryLambdaFunction: Processing GET /chat/history request");

        try
        {
            // Extract resumeId from path parameters
            if (request.PathParameters == null || 
                !request.PathParameters.TryGetValue("resumeId", out var resumeId) ||
                string.IsNullOrWhiteSpace(resumeId))
            {
                _logger.LogWarning("ResumeId not found in path parameters");
                return CreateChatErrorResponse(400, "ResumeId is required in path");
            }

            // Extract and validate user ID from Cognito JWT using authentication middleware
            var (isAuthenticated, userId) = AuthenticationHelper.ValidateAuthentication(request, _logger);
            if (!isAuthenticated || string.IsNullOrWhiteSpace(userId))
            {
                return CreateChatErrorResponse(401, "Unauthorized - No valid authentication");
            }

            _logger.LogInformation($"User {userId} requesting chat history for resume {resumeId}");

            // Verify user owns the resume
            var profile = await _profileRepository!.GetProfileAsync(resumeId);
            if (profile == null)
            {
                _logger.LogWarning($"Profile not found for ResumeId: {resumeId}");
                return CreateChatErrorResponse(404, "Resume profile not found");
            }

            // Verify user owns the resume using authentication middleware
            if (!AuthenticationHelper.ValidateOwnership(userId, profile.UserId))
            {
                _logger.LogWarning($"User {userId} attempted to access resume {resumeId} owned by {profile.UserId}");
                return CreateChatErrorResponse(403, "Forbidden - You do not own this resume");
            }

            // Retrieve chat history from DynamoDB
            var messages = await _chatHistoryRepository!.GetHistoryAsync(resumeId);

            _logger.LogInformation($"Retrieved {messages.Count} messages for resume {resumeId}");

            // Return chat history in chronological order
            var response = new
            {
                resumeId = resumeId,
                messages = messages
            };

            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" },
                    { "Access-Control-Allow-Origin", "*" },
                    { "Access-Control-Allow-Headers", "Content-Type,Authorization" },
                    { "Access-Control-Allow-Methods", "GET,OPTIONS" }
                },
                Body = JsonSerializer.Serialize(response)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving chat history");
            return CreateChatErrorResponse(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// Creates a standardized error response
    /// </summary>
    private APIGatewayProxyResponse CreateChatErrorResponse(int statusCode, string message)
    {
        return new APIGatewayProxyResponse
        {
            StatusCode = statusCode,
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" },
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Headers", "Content-Type,Authorization" },
                { "Access-Control-Allow-Methods", "GET,OPTIONS" }
            },
            Body = JsonSerializer.Serialize(new { message })
        };
    }
}