using Xunit;
using Moq;
using TheAnalyzer.Handlers;
using TheAnalyzer.Interfaces;
using TheAnalyzer.Models;
using TheAnalyzer.Exceptions;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using System.Text.Json;

namespace TheAnalyzer.Tests;

/// <summary>
/// Unit tests for AnalyzeLambdaFunction
/// </summary>
public class AnalyzeLambdaFunctionTests
{
    private readonly Mock<IAnalyzeService> _mockAnalyzeService;
    private readonly Mock<IProfileRepository> _mockRepository;
    private readonly Mock<ILambdaContext> _mockContext;
    private readonly Mock<ILambdaLogger> _mockLogger;
    
    public AnalyzeLambdaFunctionTests()
    {
        _mockAnalyzeService = new Mock<IAnalyzeService>();
        _mockRepository = new Mock<IProfileRepository>();
        _mockContext = new Mock<ILambdaContext>();
        _mockLogger = new Mock<ILambdaLogger>();
        _mockContext.Setup(c => c.Logger).Returns(_mockLogger.Object);
    }
    
    private APIGatewayProxyRequest CreateProxyRequest(AnalyzeRequest request, string? userId = "test-user-id")
    {
        var proxyRequest = new APIGatewayProxyRequest
        {
            Body = JsonSerializer.Serialize(request),
            RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
            {
                Authorizer = new APIGatewayCustomAuthorizerContext()
            }
        };
        
        if (userId != null)
        {
            proxyRequest.RequestContext.Authorizer.Claims = new Dictionary<string, string>
            {
                { "sub", userId }
            };
        }
        
        return proxyRequest;
    }

    
    [Fact]
    public async Task FunctionHandler_ValidInput_ReturnsSuccessResponse()
    {
        // Arrange
        var request = new AnalyzeRequest
        {
            ResumeId = "test-123",
            ResumeText = "John Doe\njohn.doe@example.com\n+1-555-0123\nSkills: C#, .NET, AWS"
        };
        
        var analysisResult = new AnalysisResult
        {
            ResumeEntities = new ExtractedEntities
            {
                Name = "John Doe",
                Email = "john.doe@example.com",
                Phone = "+1-555-0123",
                Skills = new List<string> { "C#", ".NET", "AWS" },
                Method = "Comprehend",
                TotalEntitiesFound = 4
            }
        };
        
        _mockAnalyzeService.Setup(s => s.AnalyzeCvAsync(
            It.IsAny<string>(), 
            It.IsAny<string?>(), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(analysisResult);
        
        _mockRepository.Setup(r => r.SaveProfileAsync(It.IsAny<ProfileRecord>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockRepository.Setup(r => r.SaveAnalysisAsync(It.IsAny<AnalysisRecord>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        var function = new AnalyzeLambdaFunction(_mockAnalyzeService.Object, _mockRepository.Object, _mockLogger.Object);
        var proxyRequest = CreateProxyRequest(request);
        
        // Act
        var response = await function.FunctionHandler(proxyRequest, _mockContext.Object);
        
        // Assert
        Assert.NotNull(response);
        Assert.Equal(200, response.StatusCode);
        
        var responseBody = JsonSerializer.Deserialize<AnalyzeResponse>(response.Body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(responseBody);
        Assert.Equal("test-123", responseBody.ResumeId);
        Assert.Equal("Analysis completed successfully", responseBody.Message);
    }
    
    [Fact]
    public async Task FunctionHandler_NoResumeId_GeneratesNewId()
    {
        // Arrange
        var request = new AnalyzeRequest
        {
            ResumeText = "John Doe\njohn.doe@example.com"
        };
        
        var analysisResult = new AnalysisResult
        {
            ResumeEntities = new ExtractedEntities
            {
                Name = "John Doe",
                Email = "john.doe@example.com",
                Method = "Comprehend",
                TotalEntitiesFound = 2
            }
        };
        
        _mockAnalyzeService.Setup(s => s.AnalyzeCvAsync(
            It.IsAny<string>(), 
            It.IsAny<string?>(), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(analysisResult);
        
        _mockRepository.Setup(r => r.SaveProfileAsync(It.IsAny<ProfileRecord>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockRepository.Setup(r => r.SaveAnalysisAsync(It.IsAny<AnalysisRecord>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        var function = new AnalyzeLambdaFunction(_mockAnalyzeService.Object, _mockRepository.Object, _mockLogger.Object);
        var proxyRequest = CreateProxyRequest(request);
        
        // Act
        var response = await function.FunctionHandler(proxyRequest, _mockContext.Object);
        
        // Assert
        Assert.NotNull(response);
        Assert.Equal(200, response.StatusCode);
        
        var responseBody = JsonSerializer.Deserialize<AnalyzeResponse>(response.Body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(responseBody);
        Assert.NotNull(responseBody.ResumeId);
        Assert.NotEmpty(responseBody.ResumeId);
        Assert.True(Guid.TryParse(responseBody.ResumeId, out _), "Generated ResumeId should be a valid GUID");
    }
    
    [Fact]
    public async Task FunctionHandler_EmptyBody_Returns400()
    {
        // Arrange
        var function = new AnalyzeLambdaFunction(_mockAnalyzeService.Object, _mockRepository.Object, _mockLogger.Object);
        var proxyRequest = new APIGatewayProxyRequest
        {
            Body = "",
            RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
            {
                Authorizer = new APIGatewayCustomAuthorizerContext
                {
                    Claims = new Dictionary<string, string> { { "sub", "test-user" } }
                }
            }
        };
        
        // Act
        var response = await function.FunctionHandler(proxyRequest, _mockContext.Object);
        
        // Assert
        Assert.Equal(400, response.StatusCode);
        Assert.Contains("empty", response.Body, StringComparison.OrdinalIgnoreCase);
    }
    
    [Fact]
    public async Task FunctionHandler_NoAuth_Returns401()
    {
        // Arrange
        var request = new AnalyzeRequest
        {
            ResumeText = "Valid CV text"
        };
        
        var function = new AnalyzeLambdaFunction(_mockAnalyzeService.Object, _mockRepository.Object, _mockLogger.Object);
        var proxyRequest = new APIGatewayProxyRequest
        {
            Body = JsonSerializer.Serialize(request),
            RequestContext = new APIGatewayProxyRequest.ProxyRequestContext()
        };
        
        // Act
        var response = await function.FunctionHandler(proxyRequest, _mockContext.Object);
        
        // Assert
        Assert.Equal(401, response.StatusCode);
        Assert.Contains("Unauthorized", response.Body);
    }
}
