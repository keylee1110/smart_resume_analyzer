using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TheChatbot.Models;
using TheChatbot.Services;
using TheChatbot.Data;
using TheChatbot.Middleware;

namespace TheChatbot.Handlers;

/// <summary>
/// Lambda function handler for POST /chat endpoint
/// Handles chat requests with context-aware AI responses
/// </summary>
public class ChatLambdaFunction
{
    private readonly ILogger<ChatLambdaFunction> _logger;
    private readonly IBedrockChatService _chatService;
    private readonly IProfileRepository _profileRepository;
    private readonly IChatHistoryRepository _chatHistoryRepository;

    public ChatLambdaFunction()
    {
        try
        {
            var serviceProvider = new ServiceCollection()
                .ConfigureServices()
                .BuildServiceProvider();

            _logger = serviceProvider.GetRequiredService<ILogger<ChatLambdaFunction>>();
            _chatService = serviceProvider.GetRequiredService<IBedrockChatService>();
            _profileRepository = serviceProvider.GetRequiredService<IProfileRepository>();
            _chatHistoryRepository = serviceProvider.GetRequiredService<IChatHistoryRepository>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CRITICAL] Constructor initialization failed: {ex}");
            throw;
        }
    }

    public ChatLambdaFunction(
        ILogger<ChatLambdaFunction> logger, 
        IBedrockChatService chatService, 
        IProfileRepository profileRepository,
        IChatHistoryRepository chatHistoryRepository)
    {
        _logger = logger;
        _chatService = chatService;
        _profileRepository = profileRepository;
        _chatHistoryRepository = chatHistoryRepository;
    }

    /// <summary>
    /// Handles POST /chat requests
    /// Requirements: 1.3, 1.4, 1.5, 2.1, 2.2, 2.3
    /// </summary>
    public async Task<APIGatewayProxyResponse> FunctionHandler(
        APIGatewayProxyRequest request, 
        ILambdaContext context)
    {
        _logger.LogInformation("ChatLambdaFunction: Processing POST /chat request");

        try
        {
            // Validate request body
            if (string.IsNullOrWhiteSpace(request?.Body))
            {
                _logger.LogWarning("Request body is empty");
                return CreateErrorResponse(400, "Request body cannot be empty");
            }

            // Deserialize request
            ChatRequest? chatRequest;
            try
            {
                chatRequest = JsonSerializer.Deserialize<ChatRequest>(
                    request.Body, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing chat request");
                return CreateErrorResponse(400, "Invalid JSON in request body");
            }

            // Validate required fields
            if (chatRequest == null || 
                string.IsNullOrWhiteSpace(chatRequest.ResumeId) || 
                string.IsNullOrWhiteSpace(chatRequest.UserMessage))
            {
                _logger.LogWarning("Invalid chat request: Missing ResumeId or UserMessage");
                return CreateErrorResponse(400, "ResumeId and UserMessage are required");
            }

            // Extract and validate user ID from Cognito JWT using authentication middleware
            var (isAuthenticated, userId) = AuthenticationHelper.ValidateAuthentication(request, _logger);
            if (!isAuthenticated || string.IsNullOrWhiteSpace(userId))
            {
                return CreateErrorResponse(401, "Unauthorized - No valid authentication");
            }

            _logger.LogInformation($"User {userId} sending message for resume {chatRequest.ResumeId}");

            // Retrieve resume profile from DynamoDB
            var profile = await _profileRepository.GetProfileAsync(chatRequest.ResumeId);
            if (profile == null)
            {
                _logger.LogWarning($"Profile not found for ResumeId: {chatRequest.ResumeId}");
                return CreateErrorResponse(404, "Resume profile not found");
            }

            // Verify user owns the resume using authentication middleware
            if (!AuthenticationHelper.ValidateOwnership(userId, profile.UserId))
            {
                _logger.LogWarning($"User {userId} attempted to access resume {chatRequest.ResumeId} owned by {profile.UserId}");
                return CreateErrorResponse(403, "Forbidden - You do not own this resume");
            }

            if (string.IsNullOrWhiteSpace(profile.ResumeText))
            {
                _logger.LogWarning($"ResumeText not found for ResumeId: {chatRequest.ResumeId}");
                return CreateErrorResponse(404, "Resume text not available - profile may still be processing");
            }

            // Construct context for Bedrock
            var chatContext = new ChatContext
            {
                CvText = profile.ResumeText,
                JobDescription = chatRequest.JobDescription ?? profile.LastAnalysis?.JobDescription,
                LastAnalysis = profile.LastAnalysis,
                UserMessage = chatRequest.UserMessage,
                ChatHistory = chatRequest.ChatHistory ?? new List<ChatMessage>()
            };

            // Call BedrockChatService to get AI response
            _logger.LogInformation($"Invoking Bedrock chat service for resume {chatRequest.ResumeId}");
            var aiResponse = await _chatService.GetChatResponseAsync(chatContext);

            // Generate timestamp for messages
            var timestamp = DateTime.UtcNow.ToString("o");

            // Store user message in DynamoDB
            var userMessage = new ChatMessage
            {
                Role = "user",
                Content = chatRequest.UserMessage,
                Timestamp = timestamp
            };
            await _chatHistoryRepository.SaveMessageAsync(chatRequest.ResumeId, userMessage, userId);

            // Store AI response in DynamoDB
            var assistantMessage = new ChatMessage
            {
                Role = "assistant",
                Content = aiResponse,
                Timestamp = DateTime.UtcNow.ToString("o") // Slightly later timestamp
            };
            await _chatHistoryRepository.SaveMessageAsync(chatRequest.ResumeId, assistantMessage, userId);

            _logger.LogInformation($"Successfully processed chat request for resume {chatRequest.ResumeId}");

            // Return AI response
            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Headers = new Dictionary<string, string> 
                { 
                    { "Content-Type", "application/json" },
                    { "Access-Control-Allow-Origin", "*" },
                    { "Access-Control-Allow-Headers", "Content-Type,Authorization" },
                    { "Access-Control-Allow-Methods", "POST,OPTIONS" }
                },
                Body = JsonSerializer.Serialize(new ChatResponse 
                { 
                    AiMessage = aiResponse,
                    Timestamp = assistantMessage.Timestamp
                }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing chat request");
            return CreateErrorResponse(500, $"Internal server error: {ex.Message} \nStack: {ex.StackTrace}");
        }
    }

    /// <summary>
    /// Creates a standardized error response
    /// </summary>
    private APIGatewayProxyResponse CreateErrorResponse(int statusCode, string message)
    {
        return new APIGatewayProxyResponse
        {
            StatusCode = statusCode,
            Headers = new Dictionary<string, string> 
            { 
                { "Content-Type", "application/json" },
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Headers", "Content-Type,Authorization" },
                { "Access-Control-Allow-Methods", "POST,OPTIONS" }
            },
            Body = JsonSerializer.Serialize(new { message })
        };
    }
}
