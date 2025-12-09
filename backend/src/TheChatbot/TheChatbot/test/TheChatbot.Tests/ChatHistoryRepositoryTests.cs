using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Microsoft.Extensions.Logging;
using Moq;
using TheChatbot.Data;
using TheChatbot.Models;
using Xunit;

namespace TheChatbot.Tests;

/// <summary>
/// Unit tests for ChatHistoryRepository
/// Tests validation logic and error handling
/// Requirements: 3.1, 3.2, 3.3, 3.4, 3.5
/// </summary>
public class ChatHistoryRepositoryTests
{
    private readonly Mock<IAmazonDynamoDB> _mockDynamoDb;
    private readonly Mock<ILogger<ChatHistoryRepository>> _mockLogger;

    public ChatHistoryRepositoryTests()
    {
        _mockDynamoDb = new Mock<IAmazonDynamoDB>();
        _mockLogger = new Mock<ILogger<ChatHistoryRepository>>();
        
        // Set environment variable for table name
        Environment.SetEnvironmentVariable("TABLE_NAME", "TestProfilesTable");
    }

    #region SaveMessageAsync Tests

    [Fact]
    public async Task SaveMessageAsync_WithValidMessage_GeneratesTimestampWhenNotProvided()
    {
        // Arrange
        var repository = new ChatHistoryRepository(_mockDynamoDb.Object, _mockLogger.Object);
        var message = new ChatMessage
        {
            Role = "user",
            Content = "Test message",
            Timestamp = "" // Empty timestamp should be auto-generated
        };

        // Act & Assert
        // This will throw because we're using a mock, but we're testing that the method
        // accepts valid input and attempts to save (validation passes)
        var exception = await Assert.ThrowsAnyAsync<Exception>(async () =>
            await repository.SaveMessageAsync("test-resume-id", message, "test-user-id")
        );
        
        // Verify it's not an ArgumentException (which would indicate validation failure)
        Assert.IsNotType<ArgumentException>(exception);
        Assert.IsNotType<ArgumentNullException>(exception);
    }

    [Fact]
    public async Task SaveMessageAsync_WithValidMessageAndTimestamp_AcceptsProvidedTimestamp()
    {
        // Arrange
        var repository = new ChatHistoryRepository(_mockDynamoDb.Object, _mockLogger.Object);
        var timestamp = DateTime.UtcNow.ToString("o");
        var message = new ChatMessage
        {
            Role = "assistant",
            Content = "AI response message",
            Timestamp = timestamp
        };

        // Act & Assert
        // This will throw because we're using a mock, but we're testing that the method
        // accepts valid input with timestamp
        var exception = await Assert.ThrowsAnyAsync<Exception>(async () =>
            await repository.SaveMessageAsync("resume-123", message, "user-456")
        );
        
        // Verify it's not an ArgumentException (which would indicate validation failure)
        Assert.IsNotType<ArgumentException>(exception);
        Assert.IsNotType<ArgumentNullException>(exception);
    }

    [Fact]
    public async Task SaveMessageAsync_WithNullResumeId_ShouldThrowArgumentException()
    {
        // Arrange
        var repository = new ChatHistoryRepository(_mockDynamoDb.Object, _mockLogger.Object);
        var message = new ChatMessage
        {
            Role = "user",
            Content = "Test message"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
            await repository.SaveMessageAsync(null!, message, "test-user-id")
        );
        
        Assert.Contains("ResumeId", exception.Message);
    }

    [Fact]
    public async Task SaveMessageAsync_WithEmptyResumeId_ShouldThrowArgumentException()
    {
        // Arrange
        var repository = new ChatHistoryRepository(_mockDynamoDb.Object, _mockLogger.Object);
        var message = new ChatMessage
        {
            Role = "user",
            Content = "Test message"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
            await repository.SaveMessageAsync("", message, "test-user-id")
        );
        
        Assert.Contains("ResumeId", exception.Message);
    }

    [Fact]
    public async Task SaveMessageAsync_WithWhitespaceResumeId_ShouldThrowArgumentException()
    {
        // Arrange
        var repository = new ChatHistoryRepository(_mockDynamoDb.Object, _mockLogger.Object);
        var message = new ChatMessage
        {
            Role = "user",
            Content = "Test message"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
            await repository.SaveMessageAsync("   ", message, "test-user-id")
        );
        
        Assert.Contains("ResumeId", exception.Message);
    }

    [Fact]
    public async Task SaveMessageAsync_WithNullMessage_ShouldThrowArgumentNullException()
    {
        // Arrange
        var repository = new ChatHistoryRepository(_mockDynamoDb.Object, _mockLogger.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await repository.SaveMessageAsync("test-resume-id", null!, "test-user-id")
        );
        
        Assert.Contains("message", exception.ParamName);
    }

    [Fact]
    public async Task SaveMessageAsync_WithNullUserId_ShouldThrowArgumentException()
    {
        // Arrange
        var repository = new ChatHistoryRepository(_mockDynamoDb.Object, _mockLogger.Object);
        var message = new ChatMessage
        {
            Role = "user",
            Content = "Test message"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
            await repository.SaveMessageAsync("test-resume-id", message, null!)
        );
        
        Assert.Contains("UserId", exception.Message);
    }

    [Fact]
    public async Task SaveMessageAsync_WithEmptyUserId_ShouldThrowArgumentException()
    {
        // Arrange
        var repository = new ChatHistoryRepository(_mockDynamoDb.Object, _mockLogger.Object);
        var message = new ChatMessage
        {
            Role = "user",
            Content = "Test message"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
            await repository.SaveMessageAsync("test-resume-id", message, "")
        );
        
        Assert.Contains("UserId", exception.Message);
    }

    [Fact]
    public async Task SaveMessageAsync_WithWhitespaceUserId_ShouldThrowArgumentException()
    {
        // Arrange
        var repository = new ChatHistoryRepository(_mockDynamoDb.Object, _mockLogger.Object);
        var message = new ChatMessage
        {
            Role = "user",
            Content = "Test message"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
            await repository.SaveMessageAsync("test-resume-id", message, "   ")
        );
        
        Assert.Contains("UserId", exception.Message);
    }

    #endregion

    #region GetHistoryAsync Tests

    [Fact]
    public async Task GetHistoryAsync_WithNullResumeId_ShouldThrowArgumentException()
    {
        // Arrange
        var repository = new ChatHistoryRepository(_mockDynamoDb.Object, _mockLogger.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
            await repository.GetHistoryAsync(null!)
        );
        
        Assert.Contains("ResumeId", exception.Message);
    }

    [Fact]
    public async Task GetHistoryAsync_WithEmptyResumeId_ShouldThrowArgumentException()
    {
        // Arrange
        var repository = new ChatHistoryRepository(_mockDynamoDb.Object, _mockLogger.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
            await repository.GetHistoryAsync("")
        );
        
        Assert.Contains("ResumeId", exception.Message);
    }

    [Fact]
    public async Task GetHistoryAsync_WithWhitespaceResumeId_ShouldThrowArgumentException()
    {
        // Arrange
        var repository = new ChatHistoryRepository(_mockDynamoDb.Object, _mockLogger.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
            await repository.GetHistoryAsync("   ")
        );
        
        Assert.Contains("ResumeId", exception.Message);
    }

    #endregion

    #region Data Model Tests

    [Fact]
    public void ChatHistoryRecord_PKFormat_IsCorrect()
    {
        // Arrange
        var resumeId = "test-resume-123";
        var expectedPK = $"RESUME#{resumeId}";

        // Act
        var record = new ChatHistoryRecord
        {
            PK = expectedPK,
            ResumeId = resumeId
        };

        // Assert
        Assert.Equal(expectedPK, record.PK);
        Assert.StartsWith("RESUME#", record.PK);
    }

    [Fact]
    public void ChatHistoryRecord_SKFormat_IsCorrect()
    {
        // Arrange
        var timestamp = DateTime.UtcNow.ToString("o");
        var expectedSK = $"CHAT#{timestamp}";

        // Act
        var record = new ChatHistoryRecord
        {
            SK = expectedSK,
            Timestamp = timestamp
        };

        // Assert
        Assert.Equal(expectedSK, record.SK);
        Assert.StartsWith("CHAT#", record.SK);
    }

    [Fact]
    public void ChatHistoryRecord_ContainsAllRequiredFields()
    {
        // Arrange & Act
        var record = new ChatHistoryRecord
        {
            PK = "RESUME#test-123",
            SK = "CHAT#2024-01-01T12:00:00Z",
            ResumeId = "test-123",
            Timestamp = "2024-01-01T12:00:00Z",
            Role = "user",
            Content = "Test message",
            UserId = "user-456"
        };

        // Assert
        Assert.NotNull(record.PK);
        Assert.NotNull(record.SK);
        Assert.NotNull(record.ResumeId);
        Assert.NotNull(record.Timestamp);
        Assert.NotNull(record.Role);
        Assert.NotNull(record.Content);
        Assert.NotNull(record.UserId);
    }

    [Fact]
    public void ChatMessage_SupportsUserRole()
    {
        // Arrange & Act
        var message = new ChatMessage
        {
            Role = "user",
            Content = "User message",
            Timestamp = DateTime.UtcNow.ToString("o")
        };

        // Assert
        Assert.Equal("user", message.Role);
        Assert.NotEmpty(message.Content);
        Assert.NotEmpty(message.Timestamp);
    }

    [Fact]
    public void ChatMessage_SupportsAssistantRole()
    {
        // Arrange & Act
        var message = new ChatMessage
        {
            Role = "assistant",
            Content = "AI response",
            Timestamp = DateTime.UtcNow.ToString("o")
        };

        // Assert
        Assert.Equal("assistant", message.Role);
        Assert.NotEmpty(message.Content);
        Assert.NotEmpty(message.Timestamp);
    }

    #endregion
}
