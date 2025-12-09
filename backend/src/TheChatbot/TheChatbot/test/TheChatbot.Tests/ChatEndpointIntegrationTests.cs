using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using Microsoft.Extensions.Logging;
using Moq;
using TheChatbot.Data;
using TheChatbot.Handlers;
using TheChatbot.Models;
using TheChatbot.Services;
using Xunit;

namespace TheChatbot.Tests;

/// <summary>
/// Integration tests for chat endpoints
/// Requirements: 1.3, 1.4, 1.5, 3.3, 13.3
/// </summary>
public class ChatEndpointIntegrationTests
{
    private readonly Mock<ILogger<ChatLambdaFunction>> _chatLogger;
    private readonly Mock<ILogger<GetChatHistoryLambdaFunction>> _historyLogger;
    private readonly Mock<IBedrockChatService> _mockChatService;
    private readonly Mock<IProfileRepository> _mockProfileRepository;
    private readonly Mock<IChatHistoryRepository> _mockChatHistoryRepository;

    public ChatEndpointIntegrationTests()
    {
        _chatLogger = new Mock<ILogger<ChatLambdaFunction>>();
        _historyLogger = new Mock<ILogger<GetChatHistoryLambdaFunction>>();
        _mockChatService = new Mock<IBedrockChatService>();
        _mockProfileRepository = new Mock<IProfileRepository>();
        _mockChatHistoryRepository = new Mock<IChatHistoryRepository>();
    }

    [Fact]
    public async Task PostChat_WithValidRequest_ReturnsSuccess()
    {
        // Arrange
        var resumeId = "test-resume-123";
        var userId = "test-user-456";
        var userMessage = "What skills should I focus on?";
        var aiResponse = "Based on your resume, you should focus on cloud technologies.";

        var profile = new ProfileRecord
        {
            ResumeId = resumeId,
            UserId = userId,
            ResumeText = "Sample resume text",
            LastAnalysis = new AnalysisResult
            {
                JobDescription = "Sample job description",
                FitScore = 85
            }
        };

        _mockProfileRepository
            .Setup(r => r.GetProfileAsync(resumeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        _mockChatService
            .Setup(s => s.GetChatResponseAsync(It.IsAny<ChatContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(aiResponse);

        _mockChatHistoryRepository
            .Setup(r => r.SaveMessageAsync(resumeId, It.IsAny<ChatMessage>(), userId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var function = new ChatLambdaFunction(
            _chatLogger.Object,
            _mockChatService.Object,
            _mockProfileRepository.Object,
            _mockChatHistoryRepository.Object
        );

        var request = new APIGatewayProxyRequest
        {
            Body = JsonSerializer.Serialize(new ChatRequest
            {
                ResumeId = resumeId,
                UserMessage = userMessage,
                ChatHistory = new List<ChatMessage>()
            }),
            RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
            {
                Authorizer = new APIGatewayCustomAuthorizerContext
                {
                    Claims = new Dictionary<string, string>
                    {
                        { "sub", userId }
                    }
                }
            }
        };

        var context = new TestLambdaContext();

        // Act
        var response = await function.FunctionHandler(request, context);

        // Assert
        Assert.Equal(200, response.StatusCode);
        
        var chatResponse = JsonSerializer.Deserialize<ChatResponse>(response.Body);
        Assert.NotNull(chatResponse);
        Assert.Equal(aiResponse, chatResponse.AiMessage);
        Assert.False(string.IsNullOrWhiteSpace(chatResponse.Timestamp));

        // Verify chat service was called with correct context
        _mockChatService.Verify(
            s => s.GetChatResponseAsync(
                It.Is<ChatContext>(c => 
                    c.CvText == profile.ResumeText &&
                    c.UserMessage == userMessage
                ),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );

        // Verify messages were saved (user message + AI response)
        _mockChatHistoryRepository.Verify(
            r => r.SaveMessageAsync(resumeId, It.IsAny<ChatMessage>(), userId, It.IsAny<CancellationToken>()),
            Times.Exactly(2)
        );
    }

    [Fact]
    public async Task GetChatHistory_WithValidRequest_ReturnsMessages()
    {
        // Arrange
        var resumeId = "test-resume-123";
        var userId = "test-user-456";

        var profile = new ProfileRecord
        {
            ResumeId = resumeId,
            UserId = userId
        };

        var messages = new List<ChatMessage>
        {
            new ChatMessage
            {
                Role = "user",
                Content = "First message",
                Timestamp = "2024-01-01T12:00:00Z"
            },
            new ChatMessage
            {
                Role = "assistant",
                Content = "First response",
                Timestamp = "2024-01-01T12:00:05Z"
            }
        };

        _mockProfileRepository
            .Setup(r => r.GetProfileAsync(resumeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        _mockChatHistoryRepository
            .Setup(r => r.GetHistoryAsync(resumeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(messages);

        var function = new GetChatHistoryLambdaFunction(
            _historyLogger.Object,
            _mockChatHistoryRepository.Object,
            _mockProfileRepository.Object
        );

        var request = new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string>
            {
                { "resumeId", resumeId }
            },
            RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
            {
                Authorizer = new APIGatewayCustomAuthorizerContext
                {
                    Claims = new Dictionary<string, string>
                    {
                        { "sub", userId }
                    }
                }
            }
        };

        var context = new TestLambdaContext();

        // Act
        var response = await function.FunctionHandler(request, context);

        // Assert
        Assert.Equal(200, response.StatusCode);

        var result = JsonSerializer.Deserialize<JsonElement>(response.Body);
        Assert.Equal(resumeId, result.GetProperty("resumeId").GetString());
        
        var returnedMessages = result.GetProperty("messages").EnumerateArray().ToList();
        Assert.Equal(2, returnedMessages.Count);
        Assert.Equal("user", returnedMessages[0].GetProperty("Role").GetString());
        Assert.Equal("First message", returnedMessages[0].GetProperty("Content").GetString());
    }

    [Fact]
    public async Task PostChat_WithMissingAuthentication_Returns401()
    {
        // Arrange
        var function = new ChatLambdaFunction(
            _chatLogger.Object,
            _mockChatService.Object,
            _mockProfileRepository.Object,
            _mockChatHistoryRepository.Object
        );

        var request = new APIGatewayProxyRequest
        {
            Body = JsonSerializer.Serialize(new ChatRequest
            {
                ResumeId = "test-resume-123",
                UserMessage = "Test message"
            }),
            RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
            {
                Authorizer = new APIGatewayCustomAuthorizerContext
                {
                    Claims = new Dictionary<string, string>() // Empty claims
                }
            }
        };

        var context = new TestLambdaContext();

        // Act
        var response = await function.FunctionHandler(request, context);

        // Assert
        Assert.Equal(401, response.StatusCode);
        Assert.Contains("Unauthorized", response.Body);
    }

    [Fact]
    public async Task GetChatHistory_WithMissingAuthentication_Returns401()
    {
        // Arrange
        var function = new GetChatHistoryLambdaFunction(
            _historyLogger.Object,
            _mockChatHistoryRepository.Object,
            _mockProfileRepository.Object
        );

        var request = new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string>
            {
                { "resumeId", "test-resume-123" }
            },
            RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
            {
                Authorizer = new APIGatewayCustomAuthorizerContext
                {
                    Claims = new Dictionary<string, string>() // Empty claims
                }
            }
        };

        var context = new TestLambdaContext();

        // Act
        var response = await function.FunctionHandler(request, context);

        // Assert
        Assert.Equal(401, response.StatusCode);
        Assert.Contains("Unauthorized", response.Body);
    }

    [Fact]
    public async Task PostChat_WithInvalidResumeId_Returns404()
    {
        // Arrange
        var resumeId = "invalid-resume-id";
        var userId = "test-user-456";

        _mockProfileRepository
            .Setup(r => r.GetProfileAsync(resumeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProfileRecord?)null);

        var function = new ChatLambdaFunction(
            _chatLogger.Object,
            _mockChatService.Object,
            _mockProfileRepository.Object,
            _mockChatHistoryRepository.Object
        );

        var request = new APIGatewayProxyRequest
        {
            Body = JsonSerializer.Serialize(new ChatRequest
            {
                ResumeId = resumeId,
                UserMessage = "Test message"
            }),
            RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
            {
                Authorizer = new APIGatewayCustomAuthorizerContext
                {
                    Claims = new Dictionary<string, string>
                    {
                        { "sub", userId }
                    }
                }
            }
        };

        var context = new TestLambdaContext();

        // Act
        var response = await function.FunctionHandler(request, context);

        // Assert
        Assert.Equal(404, response.StatusCode);
        Assert.Contains("not found", response.Body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetChatHistory_WithInvalidResumeId_Returns404()
    {
        // Arrange
        var resumeId = "invalid-resume-id";
        var userId = "test-user-456";

        _mockProfileRepository
            .Setup(r => r.GetProfileAsync(resumeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProfileRecord?)null);

        var function = new GetChatHistoryLambdaFunction(
            _historyLogger.Object,
            _mockChatHistoryRepository.Object,
            _mockProfileRepository.Object
        );

        var request = new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string>
            {
                { "resumeId", resumeId }
            },
            RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
            {
                Authorizer = new APIGatewayCustomAuthorizerContext
                {
                    Claims = new Dictionary<string, string>
                    {
                        { "sub", userId }
                    }
                }
            }
        };

        var context = new TestLambdaContext();

        // Act
        var response = await function.FunctionHandler(request, context);

        // Assert
        Assert.Equal(404, response.StatusCode);
        Assert.Contains("not found", response.Body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task PostChat_WithUnauthorizedUser_Returns403()
    {
        // Arrange
        var resumeId = "test-resume-123";
        var userId = "test-user-456";
        var differentUserId = "different-user-789";

        var profile = new ProfileRecord
        {
            ResumeId = resumeId,
            UserId = differentUserId, // Different user owns this resume
            ResumeText = "Sample resume text"
        };

        _mockProfileRepository
            .Setup(r => r.GetProfileAsync(resumeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        var function = new ChatLambdaFunction(
            _chatLogger.Object,
            _mockChatService.Object,
            _mockProfileRepository.Object,
            _mockChatHistoryRepository.Object
        );

        var request = new APIGatewayProxyRequest
        {
            Body = JsonSerializer.Serialize(new ChatRequest
            {
                ResumeId = resumeId,
                UserMessage = "Test message"
            }),
            RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
            {
                Authorizer = new APIGatewayCustomAuthorizerContext
                {
                    Claims = new Dictionary<string, string>
                    {
                        { "sub", userId }
                    }
                }
            }
        };

        var context = new TestLambdaContext();

        // Act
        var response = await function.FunctionHandler(request, context);

        // Assert
        Assert.Equal(403, response.StatusCode);
        Assert.Contains("Forbidden", response.Body);
    }

    [Fact]
    public async Task GetChatHistory_WithUnauthorizedUser_Returns403()
    {
        // Arrange
        var resumeId = "test-resume-123";
        var userId = "test-user-456";
        var differentUserId = "different-user-789";

        var profile = new ProfileRecord
        {
            ResumeId = resumeId,
            UserId = differentUserId // Different user owns this resume
        };

        _mockProfileRepository
            .Setup(r => r.GetProfileAsync(resumeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        var function = new GetChatHistoryLambdaFunction(
            _historyLogger.Object,
            _mockChatHistoryRepository.Object,
            _mockProfileRepository.Object
        );

        var request = new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string>
            {
                { "resumeId", resumeId }
            },
            RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
            {
                Authorizer = new APIGatewayCustomAuthorizerContext
                {
                    Claims = new Dictionary<string, string>
                    {
                        { "sub", userId }
                    }
                }
            }
        };

        var context = new TestLambdaContext();

        // Act
        var response = await function.FunctionHandler(request, context);

        // Assert
        Assert.Equal(403, response.StatusCode);
        Assert.Contains("Forbidden", response.Body);
    }
}
