using Xunit;
using Moq;
using TheAnalyzer.Handlers;
using TheAnalyzer.Interfaces;
using TheAnalyzer.Models;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using System.Text.Json;

namespace TheAnalyzer.Tests;

/// <summary>
/// Unit tests for GetResumeLambdaFunction
/// </summary>
public class GetResumeLambdaFunctionTests
{
    private readonly Mock<IProfileRepository> _mockRepository;
    private readonly Mock<ILambdaContext> _mockContext;
    private readonly Mock<ILambdaLogger> _mockLogger;
    
    public GetResumeLambdaFunctionTests()
    {
        _mockRepository = new Mock<IProfileRepository>();
        _mockContext = new Mock<ILambdaContext>();
        _mockLogger = new Mock<ILambdaLogger>();
        _mockContext.Setup(c => c.Logger).Returns(_mockLogger.Object);
    }
    
    [Fact]
    public async Task FunctionHandler_ExistingProfile_Returns200WithCompleteData()
    {
        // Arrange
        var resumeId = "test-123";
        var profile = new ProfileRecord
        {
            ResumeId = resumeId,
            Name = "John Doe",
            Email = "john.doe@example.com",
            Phone = "+1-555-0123",
            Skills = new List<string> { "C#", ".NET", "AWS" },
            CreatedAt = DateTime.UtcNow
        };
        
        _mockRepository.Setup(r => r.GetProfileAsync(resumeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);
        
        var request = new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string>
            {
                { "id", resumeId }
            }
        };
        
        var function = new GetResumeLambdaFunction(_mockRepository.Object);
        
        // Act
        var response = await function.FunctionHandler(request, _mockContext.Object);
        
        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(response.Body);
        Assert.Equal("application/json", response.Headers["Content-Type"]);
        
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var returnedProfile = JsonSerializer.Deserialize<ProfileRecord>(response.Body, options);
        Assert.NotNull(returnedProfile);
        Assert.Equal(resumeId, returnedProfile.ResumeId);
        Assert.Equal("John Doe", returnedProfile.Name);
        Assert.Equal("john.doe@example.com", returnedProfile.Email);
        Assert.Equal("+1-555-0123", returnedProfile.Phone);
        Assert.Equal(3, returnedProfile.Skills.Count);
        Assert.Contains("C#", returnedProfile.Skills);
        Assert.Contains(".NET", returnedProfile.Skills);
        Assert.Contains("AWS", returnedProfile.Skills);
        
        _mockRepository.Verify(r => r.GetProfileAsync(resumeId, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task FunctionHandler_NonExistentProfile_Returns404()
    {
        // Arrange
        var resumeId = "non-existent-id";
        
        _mockRepository.Setup(r => r.GetProfileAsync(resumeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProfileRecord?)null);
        
        var request = new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string>
            {
                { "id", resumeId }
            }
        };
        
        var function = new GetResumeLambdaFunction(_mockRepository.Object);
        
        // Act
        var response = await function.FunctionHandler(request, _mockContext.Object);
        
        // Assert
        Assert.Equal(404, response.StatusCode);
        Assert.NotNull(response.Body);
        Assert.Equal("application/json", response.Headers["Content-Type"]);
        
        var errorResponse = JsonSerializer.Deserialize<JsonElement>(response.Body);
        Assert.Equal("NotFoundError", errorResponse.GetProperty("errorType").GetString());
        Assert.Equal("Resume not found", errorResponse.GetProperty("message").GetString());
        Assert.Equal(404, errorResponse.GetProperty("statusCode").GetInt32());
        
        _mockRepository.Verify(r => r.GetProfileAsync(resumeId, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task FunctionHandler_MissingResumeId_Returns400()
    {
        // Arrange
        var request = new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string>()
        };
        
        var function = new GetResumeLambdaFunction(_mockRepository.Object);
        
        // Act
        var response = await function.FunctionHandler(request, _mockContext.Object);
        
        // Assert
        Assert.Equal(400, response.StatusCode);
        Assert.NotNull(response.Body);
        Assert.Equal("application/json", response.Headers["Content-Type"]);
        
        var errorResponse = JsonSerializer.Deserialize<JsonElement>(response.Body);
        Assert.Equal("ValidationError", errorResponse.GetProperty("errorType").GetString());
        Assert.Equal("Missing resume ID", errorResponse.GetProperty("message").GetString());
        Assert.Equal(400, errorResponse.GetProperty("statusCode").GetInt32());
        
        _mockRepository.Verify(r => r.GetProfileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public async Task FunctionHandler_NullPathParameters_Returns400()
    {
        // Arrange
        var request = new APIGatewayProxyRequest
        {
            PathParameters = null
        };
        
        var function = new GetResumeLambdaFunction(_mockRepository.Object);
        
        // Act
        var response = await function.FunctionHandler(request, _mockContext.Object);
        
        // Assert
        Assert.Equal(400, response.StatusCode);
        Assert.NotNull(response.Body);
        
        var errorResponse = JsonSerializer.Deserialize<JsonElement>(response.Body);
        Assert.Equal("ValidationError", errorResponse.GetProperty("errorType").GetString());
        Assert.Equal("Missing resume ID", errorResponse.GetProperty("message").GetString());
        
        _mockRepository.Verify(r => r.GetProfileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public async Task FunctionHandler_EmptyResumeId_Returns400()
    {
        // Arrange
        var request = new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string>
            {
                { "id", "" }
            }
        };
        
        var function = new GetResumeLambdaFunction(_mockRepository.Object);
        
        // Act
        var response = await function.FunctionHandler(request, _mockContext.Object);
        
        // Assert
        Assert.Equal(400, response.StatusCode);
        
        var errorResponse = JsonSerializer.Deserialize<JsonElement>(response.Body);
        Assert.Equal("ValidationError", errorResponse.GetProperty("errorType").GetString());
        Assert.Equal("Missing resume ID", errorResponse.GetProperty("message").GetString());
        
        _mockRepository.Verify(r => r.GetProfileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public async Task FunctionHandler_WhitespaceResumeId_Returns400()
    {
        // Arrange
        var request = new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string>
            {
                { "id", "   " }
            }
        };
        
        var function = new GetResumeLambdaFunction(_mockRepository.Object);
        
        // Act
        var response = await function.FunctionHandler(request, _mockContext.Object);
        
        // Assert
        Assert.Equal(400, response.StatusCode);
        
        var errorResponse = JsonSerializer.Deserialize<JsonElement>(response.Body);
        Assert.Equal("ValidationError", errorResponse.GetProperty("errorType").GetString());
        Assert.Equal("Missing resume ID", errorResponse.GetProperty("message").GetString());
    }
    
    [Fact]
    public async Task FunctionHandler_DynamoDBError_Returns500()
    {
        // Arrange
        var resumeId = "test-error";
        
        _mockRepository.Setup(r => r.GetProfileAsync(resumeId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DynamoDB connection failed"));
        
        var request = new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string>
            {
                { "id", resumeId }
            }
        };
        
        var function = new GetResumeLambdaFunction(_mockRepository.Object);
        
        // Act
        var response = await function.FunctionHandler(request, _mockContext.Object);
        
        // Assert
        Assert.Equal(500, response.StatusCode);
        Assert.NotNull(response.Body);
        Assert.Equal("application/json", response.Headers["Content-Type"]);
        
        var errorResponse = JsonSerializer.Deserialize<JsonElement>(response.Body);
        Assert.Equal("InternalError", errorResponse.GetProperty("errorType").GetString());
        Assert.Equal("Internal server error", errorResponse.GetProperty("message").GetString());
        Assert.Equal(500, errorResponse.GetProperty("statusCode").GetInt32());
    }
    
    [Fact]
    public async Task FunctionHandler_ProfileWithAllFields_ReturnsCompleteData()
    {
        // Arrange
        var resumeId = "complete-profile";
        var profile = new ProfileRecord
        {
            ResumeId = resumeId,
            Name = "Jane Smith",
            Email = "jane.smith@company.com",
            Phone = "+44-20-1234-5678",
            Skills = new List<string> { "Python", "Java", "Docker", "Kubernetes", "AWS", "Azure" },
            CreatedAt = new DateTime(2025, 12, 6, 10, 30, 0, DateTimeKind.Utc)
        };
        
        _mockRepository.Setup(r => r.GetProfileAsync(resumeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);
        
        var request = new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string>
            {
                { "id", resumeId }
            }
        };
        
        var function = new GetResumeLambdaFunction(_mockRepository.Object);
        
        // Act
        var response = await function.FunctionHandler(request, _mockContext.Object);
        
        // Assert
        Assert.Equal(200, response.StatusCode);
        
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var returnedProfile = JsonSerializer.Deserialize<ProfileRecord>(response.Body, options);
        Assert.NotNull(returnedProfile);
        Assert.Equal("Jane Smith", returnedProfile.Name);
        Assert.Equal("jane.smith@company.com", returnedProfile.Email);
        Assert.Equal("+44-20-1234-5678", returnedProfile.Phone);
        Assert.Equal(6, returnedProfile.Skills.Count);
        Assert.Contains("Python", returnedProfile.Skills);
        Assert.Contains("Kubernetes", returnedProfile.Skills);
    }
    
    [Fact]
    public async Task FunctionHandler_ProfileWithMinimalData_ReturnsSuccessfully()
    {
        // Arrange
        var resumeId = "minimal-profile";
        var profile = new ProfileRecord
        {
            ResumeId = resumeId,
            Name = "Bob Johnson",
            Email = null,
            Phone = null,
            Skills = new List<string>(),
            CreatedAt = DateTime.UtcNow
        };
        
        _mockRepository.Setup(r => r.GetProfileAsync(resumeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);
        
        var request = new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string>
            {
                { "id", resumeId }
            }
        };
        
        var function = new GetResumeLambdaFunction(_mockRepository.Object);
        
        // Act
        var response = await function.FunctionHandler(request, _mockContext.Object);
        
        // Assert
        Assert.Equal(200, response.StatusCode);
        
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var returnedProfile = JsonSerializer.Deserialize<ProfileRecord>(response.Body, options);
        Assert.NotNull(returnedProfile);
        Assert.Equal("Bob Johnson", returnedProfile.Name);
        Assert.Null(returnedProfile.Email);
        Assert.Null(returnedProfile.Phone);
        Assert.Empty(returnedProfile.Skills);
    }
}
