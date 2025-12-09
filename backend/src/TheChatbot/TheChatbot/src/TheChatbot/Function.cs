using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TheChatbot.Models;
using TheChatbot.Services; // Placeholder for Bedrock service
using TheChatbot.Data;     // Placeholder for data access

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace TheChatbot;

public class Function
{
    private readonly ILogger<Function> _logger;
    private readonly IBedrockChatService _chatService; // Interface for Bedrock chat service
    private readonly IProfileRepository _profileRepository; // Interface for profile data access

    public Function()
    {
        var serviceProvider = new ServiceCollection()
            .ConfigureServices()
            .BuildServiceProvider();

        _logger = serviceProvider.GetRequiredService<ILogger<Function>>();
        _chatService = serviceProvider.GetRequiredService<IBedrockChatService>();
        _profileRepository = serviceProvider.GetRequiredService<IProfileRepository>();
    }

    public Function(ILogger<Function> logger, IBedrockChatService chatService, IProfileRepository profileRepository)
    {
        _logger = logger;
        _chatService = chatService;
        _profileRepository = profileRepository;
    }

    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        _logger.LogInformation("Received API Gateway request for ChatFunction");

        if (request?.Body == null)
        {
            _logger.LogWarning("Request body is empty.");
            return new APIGatewayProxyResponse
            {
                StatusCode = 400,
                Body = JsonSerializer.Serialize(new { message = "Request body cannot be empty" })
            };
        }

        ChatRequest? chatRequest;
        try
        {
            chatRequest = JsonSerializer.Deserialize<ChatRequest>(request.Body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error deserializing chat request body.");
            return new APIGatewayProxyResponse
            {
                StatusCode = 400,
                Body = JsonSerializer.Serialize(new { message = "Invalid JSON in request body" })
            };
        }

        try
        {
            if (chatRequest == null || string.IsNullOrWhiteSpace(chatRequest.ResumeId) || string.IsNullOrWhiteSpace(chatRequest.UserMessage))
            {
                _logger.LogWarning("Invalid chat request: Missing ResumeId or UserMessage.");
                return new APIGatewayProxyResponse
                {
                    StatusCode = 400,
                    Body = JsonSerializer.Serialize(new { message = "ResumeId and UserMessage are required" })
                };
            }

            // Authentication/Authorization - get userId from claims
            string? userId = null;
            if (request.RequestContext?.Authorizer?.Claims != null &&
                request.RequestContext.Authorizer.Claims.TryGetValue("sub", out var cognitoUserId))
            {
                userId = cognitoUserId;
                _logger.LogInformation($"User authenticated via Cognito: {userId}");
            }
            else
            {
                _logger.LogWarning("Unauthorized: No user ID found in Cognito context");
                return new APIGatewayProxyResponse
                {
                    StatusCode = 401,
                    Body = JsonSerializer.Serialize(new { message = "Unauthorized - No valid authentication" })
                };
            }

            // Fetch CV and JD from storage (DynamoDB/S3)
            _logger.LogInformation($"Fetching profile for ResumeId: {chatRequest.ResumeId}");
            var profile = await _profileRepository.GetProfileAsync(chatRequest.ResumeId); // Assuming GetProfileAsync exists and gets all needed data

            if (profile == null || string.IsNullOrWhiteSpace(profile.ResumeText))
            {
                _logger.LogWarning($"Profile or ResumeText not found for ResumeId: {chatRequest.ResumeId}");
                return new APIGatewayProxyResponse
                {
                    StatusCode = 404,
                    Body = JsonSerializer.Serialize(new { message = "Resume profile not found or is still processing." })
                };
            }

            // Construct context for the chat service
            var chatContext = new ChatContext
            {
                CvText = profile.ResumeText,
                JobDescription = chatRequest.JobDescription ?? profile.LastAnalysis?.JobDescription, // Use JD from request or stored analysis
                LastAnalysis = profile.LastAnalysis,
                UserMessage = chatRequest.UserMessage,
                ChatHistory = chatRequest.ChatHistory // Pass previous messages for context
            };

            // Invoke Bedrock service (placeholder)
            _logger.LogInformation($"Invoking chat service for ResumeId: {chatRequest.ResumeId}");
            var aiResponse = await _chatService.GetChatResponseAsync(chatContext);

            _logger.LogInformation("Chat function executed successfully.");

            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
                Body = JsonSerializer.Serialize(new ChatResponse { AiMessage = aiResponse })
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Internal Server Error in ChatFunction");
            return new APIGatewayProxyResponse
            {
                StatusCode = 500,
                Body = JsonSerializer.Serialize(new { message = "Internal Server Error", details = ex.Message })
            };
        }
    }
}