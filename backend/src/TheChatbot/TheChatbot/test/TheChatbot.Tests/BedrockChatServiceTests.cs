using Xunit;
using Moq;
using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using Microsoft.Extensions.Logging;
using TheChatbot.Services;
using TheChatbot.Models;
using System.Text;
using System.Text.Json;

namespace TheChatbot.Tests;

public class BedrockChatServiceTests
{
    private readonly Mock<IAmazonBedrockRuntime> _mockBedrockClient;
    private readonly Mock<ILogger<BedrockChatService>> _mockLogger;
    private readonly BedrockChatService _service;

    public BedrockChatServiceTests()
    {
        _mockBedrockClient = new Mock<IAmazonBedrockRuntime>();
        _mockLogger = new Mock<ILogger<BedrockChatService>>();
        _service = new BedrockChatService(_mockBedrockClient.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetChatResponseAsync_WithCvAndJdContext_ConstructsPromptCorrectly()
    {
        // Arrange
        var context = new ChatContext
        {
            CvText = "Software Engineer with 5 years of experience in Python and AWS.",
            JobDescription = "Looking for a Senior Python Developer with cloud experience.",
            UserMessage = "What are my strengths for this role?",
            LastAnalysis = new AnalysisResult
            {
                FitScore = 85.5,
                MatchedSkills = new List<string> { "Python", "AWS" },
                MissingSkills = new List<string> { "Kubernetes" },
                Recommendation = "Strong candidate with relevant experience"
            },
            ChatHistory = new List<ChatMessage>()
        };

        var expectedResponse = "You have strong Python and AWS experience which aligns well with the role.";
        SetupSuccessfulBedrockResponse(expectedResponse);

        // Act
        var result = await _service.GetChatResponseAsync(context);

        // Assert
        Assert.Equal(expectedResponse, result);
        
        // Verify that InvokeModelAsync was called
        _mockBedrockClient.Verify(
            x => x.InvokeModelAsync(
                It.Is<InvokeModelRequest>(req => 
                    req.ContentType == "application/json" &&
                    req.Accept == "application/json"
                ),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task GetChatResponseAsync_WithChatHistory_IncludesHistoryInRequest()
    {
        // Arrange
        var context = new ChatContext
        {
            CvText = "Software Engineer",
            JobDescription = "Python Developer",
            UserMessage = "What about my experience?",
            ChatHistory = new List<ChatMessage>
            {
                new ChatMessage { Role = "user", Content = "Hello", Timestamp = "2024-01-01T10:00:00Z" },
                new ChatMessage { Role = "assistant", Content = "Hi! How can I help?", Timestamp = "2024-01-01T10:00:05Z" }
            }
        };

        var expectedResponse = "Your experience is relevant.";
        SetupSuccessfulBedrockResponse(expectedResponse);

        // Act
        var result = await _service.GetChatResponseAsync(context);

        // Assert
        Assert.Equal(expectedResponse, result);
        _mockBedrockClient.Verify(
            x => x.InvokeModelAsync(It.IsAny<InvokeModelRequest>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task GetChatResponseAsync_WithAnalysisResults_IncludesFitScoreAndSkills()
    {
        // Arrange
        var context = new ChatContext
        {
            CvText = "Data Scientist with ML experience",
            JobDescription = "ML Engineer position",
            UserMessage = "How well do I match?",
            LastAnalysis = new AnalysisResult
            {
                FitScore = 92.3,
                MatchedSkills = new List<string> { "Machine Learning", "Python", "TensorFlow" },
                MissingSkills = new List<string> { "PyTorch" },
                Recommendation = "Excellent match"
            },
            ChatHistory = new List<ChatMessage>()
        };

        var expectedResponse = "You match very well with a 92.3% fit score.";
        SetupSuccessfulBedrockResponse(expectedResponse);

        // Act
        var result = await _service.GetChatResponseAsync(context);

        // Assert
        Assert.Equal(expectedResponse, result);
    }

    [Fact]
    public async Task GetChatResponseAsync_WithoutJobDescription_StillWorks()
    {
        // Arrange
        var context = new ChatContext
        {
            CvText = "Software Engineer",
            JobDescription = null,
            UserMessage = "Can you review my resume?",
            ChatHistory = new List<ChatMessage>()
        };

        var expectedResponse = "Your resume looks good.";
        SetupSuccessfulBedrockResponse(expectedResponse);

        // Act
        var result = await _service.GetChatResponseAsync(context);

        // Assert
        Assert.Equal(expectedResponse, result);
    }

    [Fact]
    public async Task GetChatResponseAsync_WithoutAnalysis_StillWorks()
    {
        // Arrange
        var context = new ChatContext
        {
            CvText = "Software Engineer",
            JobDescription = "Developer position",
            UserMessage = "General career advice?",
            LastAnalysis = null,
            ChatHistory = new List<ChatMessage>()
        };

        var expectedResponse = "Focus on building your skills.";
        SetupSuccessfulBedrockResponse(expectedResponse);

        // Act
        var result = await _service.GetChatResponseAsync(context);

        // Assert
        Assert.Equal(expectedResponse, result);
    }

    [Fact]
    public async Task GetChatResponseAsync_ParsesBedrockResponseCorrectly()
    {
        // Arrange
        var context = new ChatContext
        {
            CvText = "Engineer",
            UserMessage = "Test message",
            ChatHistory = new List<ChatMessage>()
        };

        var aiMessage = "This is the AI response with **markdown** formatting.";
        SetupSuccessfulBedrockResponse(aiMessage);

        // Act
        var result = await _service.GetChatResponseAsync(context);

        // Assert
        Assert.Equal(aiMessage, result);
    }

    [Fact]
    public async Task GetChatResponseAsync_WithEmptyContentArray_ReturnsNoResponseMessage()
    {
        // Arrange
        var context = new ChatContext
        {
            CvText = "Engineer",
            UserMessage = "Test",
            ChatHistory = new List<ChatMessage>()
        };

        // Setup response with empty content array
        var responseBody = JsonSerializer.Serialize(new
        {
            content = new object[] { }
        });

        SetupBedrockResponse(responseBody);

        // Act
        var result = await _service.GetChatResponseAsync(context);

        // Assert
        Assert.Equal("No text content found in AI response.", result);
    }

    [Fact]
    public async Task GetChatResponseAsync_WithMalformedResponse_ReturnsNoResponseMessage()
    {
        // Arrange
        var context = new ChatContext
        {
            CvText = "Engineer",
            UserMessage = "Test",
            ChatHistory = new List<ChatMessage>()
        };

        // Setup response without content property
        var responseBody = JsonSerializer.Serialize(new
        {
            id = "msg_123",
            type = "message"
        });

        SetupBedrockResponse(responseBody);

        // Act
        var result = await _service.GetChatResponseAsync(context);

        // Assert
        Assert.Equal("No text content found in AI response.", result);
    }

    [Fact]
    public async Task GetChatResponseAsync_WhenBedrockThrowsException_ThrowsInvalidOperationException()
    {
        // Arrange
        var context = new ChatContext
        {
            CvText = "Engineer",
            UserMessage = "Test",
            ChatHistory = new List<ChatMessage>()
        };

        _mockBedrockClient
            .Setup(x => x.InvokeModelAsync(It.IsAny<InvokeModelRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AmazonBedrockRuntimeException("Service unavailable"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.GetChatResponseAsync(context)
        );
        
        Assert.Contains("Error communicating with Bedrock", exception.Message);
    }

    [Fact]
    public async Task GetChatResponseAsync_WhenUnexpectedErrorOccurs_ThrowsException()
    {
        // Arrange
        var context = new ChatContext
        {
            CvText = "Engineer",
            UserMessage = "Test",
            ChatHistory = new List<ChatMessage>()
        };

        _mockBedrockClient
            .Setup(x => x.InvokeModelAsync(It.IsAny<InvokeModelRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Unexpected error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.GetChatResponseAsync(context)
        );
    }

    [Fact]
    public async Task GetChatResponseAsync_WithLongChatHistory_TruncatesOldMessages()
    {
        // Arrange
        var longHistory = new List<ChatMessage>();
        for (int i = 0; i < 100; i++)
        {
            longHistory.Add(new ChatMessage 
            { 
                Role = i % 2 == 0 ? "user" : "assistant",
                Content = new string('x', 1000), // 1000 chars each
                Timestamp = $"2024-01-01T10:{i:D2}:00Z"
            });
        }

        var context = new ChatContext
        {
            CvText = new string('y', 3000),
            JobDescription = new string('z', 2000),
            UserMessage = "Current message",
            ChatHistory = longHistory
        };

        var expectedResponse = "Response";
        SetupSuccessfulBedrockResponse(expectedResponse);

        // Act
        var result = await _service.GetChatResponseAsync(context);

        // Assert
        Assert.Equal(expectedResponse, result);
        // The service should handle this without throwing
    }

    [Fact]
    public async Task GetChatResponseAsync_WithVeryLongCvText_TruncatesText()
    {
        // Arrange
        var context = new ChatContext
        {
            CvText = new string('x', 10000), // Very long CV
            JobDescription = "Short JD",
            UserMessage = "Review my CV",
            ChatHistory = new List<ChatMessage>()
        };

        var expectedResponse = "CV reviewed";
        SetupSuccessfulBedrockResponse(expectedResponse);

        // Act
        var result = await _service.GetChatResponseAsync(context);

        // Assert
        Assert.Equal(expectedResponse, result);
    }

    [Fact]
    public async Task GetChatResponseAsync_WithEmptyMatchedSkills_HandlesGracefully()
    {
        // Arrange
        var context = new ChatContext
        {
            CvText = "Junior Developer",
            JobDescription = "Senior Developer",
            UserMessage = "What should I improve?",
            LastAnalysis = new AnalysisResult
            {
                FitScore = 30.0,
                MatchedSkills = new List<string>(),
                MissingSkills = new List<string> { "Java", "Spring", "Microservices" },
                Recommendation = "Need more experience"
            },
            ChatHistory = new List<ChatMessage>()
        };

        var expectedResponse = "Focus on learning the missing skills.";
        SetupSuccessfulBedrockResponse(expectedResponse);

        // Act
        var result = await _service.GetChatResponseAsync(context);

        // Assert
        Assert.Equal(expectedResponse, result);
    }

    // Helper methods

    private void SetupSuccessfulBedrockResponse(string aiMessage)
    {
        var responseBody = JsonSerializer.Serialize(new
        {
            content = new[]
            {
                new { type = "text", text = aiMessage }
            }
        });

        SetupBedrockResponse(responseBody);
    }

    private void SetupBedrockResponse(string responseBody)
    {
        var responseStream = new MemoryStream(Encoding.UTF8.GetBytes(responseBody));
        
        var mockResponse = new InvokeModelResponse
        {
            Body = responseStream,
            ContentType = "application/json"
        };

        _mockBedrockClient
            .Setup(x => x.InvokeModelAsync(It.IsAny<InvokeModelRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse);
    }
}
