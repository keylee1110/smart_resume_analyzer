using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;
using Moq;
using TheAnalyzer.Handlers;
using TheAnalyzer.Interfaces;
using TheAnalyzer.Models;
using Xunit;

namespace TheAnalyzer.Tests;

public class ListResumesLambdaFunctionTests
{
    [Fact]
    public async Task FunctionHandler_WithValidUser_ReturnsSuccessResponse()
    {
        // Arrange
        var mockRepository = new Mock<IProfileRepository>();
        var mockLogger = new Mock<ILambdaLogger>();
        
        var profiles = new List<ProfileRecord>
        {
            new ProfileRecord
            {
                UserId = "user-123",
                ResumeId = "123",
                Name = "John Doe",
                CreatedAt = DateTime.UtcNow
            }
        };
        
        mockRepository
            .Setup(r => r.GetProfilesByUserAsync("user-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(profiles);
        
        var function = new ListResumesLambdaFunction(mockRepository.Object, mockLogger.Object);
        
        var request = new APIGatewayProxyRequest
        {
            RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
            {
                Authorizer = new APIGatewayCustomAuthorizerContext
                {
                    Claims = new Dictionary<string, string>
                    {
                        { "sub", "user-123" }
                    }
                }
            }
        };
        
        var context = new TestLambdaContext();
        
        // Act
        var response = await function.FunctionHandler(request, context);
        
        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.Contains("Access-Control-Allow-Origin", response.Headers.Keys);
        Assert.Contains("Access-Control-Allow-Headers", response.Headers.Keys);
        Assert.Contains("Access-Control-Allow-Methods", response.Headers.Keys);
        Assert.NotNull(response.Body);
    }
    
    [Fact]
    public async Task FunctionHandler_WithoutAuthentication_Returns401()
    {
        // Arrange
        var mockRepository = new Mock<IProfileRepository>();
        var mockLogger = new Mock<ILambdaLogger>();
        
        var function = new ListResumesLambdaFunction(mockRepository.Object, mockLogger.Object);
        
        var request = new APIGatewayProxyRequest
        {
            RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
            {
                Authorizer = new APIGatewayCustomAuthorizerContext
                {
                    Claims = new Dictionary<string, string>()
                }
            }
        };
        
        var context = new TestLambdaContext();
        
        // Act
        var response = await function.FunctionHandler(request, context);
        
        // Assert
        Assert.Equal(401, response.StatusCode);
        Assert.Contains("Access-Control-Allow-Origin", response.Headers.Keys);
        Assert.Contains("AuthenticationError", response.Body);
    }
    
    [Fact]
    public async Task FunctionHandler_WithException_Returns500()
    {
        // Arrange
        var mockRepository = new Mock<IProfileRepository>();
        var mockLogger = new Mock<ILambdaLogger>();
        
        mockRepository
            .Setup(r => r.GetProfilesByUserAsync("user-123", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));
        
        var function = new ListResumesLambdaFunction(mockRepository.Object, mockLogger.Object);
        
        var request = new APIGatewayProxyRequest
        {
            RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
            {
                Authorizer = new APIGatewayCustomAuthorizerContext
                {
                    Claims = new Dictionary<string, string>
                    {
                        { "sub", "user-123" }
                    }
                }
            }
        };
        
        var context = new TestLambdaContext();
        
        // Act
        var response = await function.FunctionHandler(request, context);
        
        // Assert
        Assert.Equal(500, response.StatusCode);
        Assert.Contains("Access-Control-Allow-Origin", response.Headers.Keys);
        Assert.Contains("InternalError", response.Body);
    }
}
