using Xunit;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;
using Amazon.Lambda.APIGatewayEvents;
using System.Text.Json;
using TheChatbot.Models;
using TheChatbot.Services;
using TheChatbot.Data;
using Microsoft.Extensions.Logging;
using Moq;

namespace TheChatbot.Tests;

public class FunctionTest
{
    [Fact]
    public async Task TestFunctionHandler_WithEmptyBody_Returns400()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<Function>>();
        var mockChatService = new Mock<IBedrockChatService>();
        var mockProfileRepo = new Mock<IProfileRepository>();
        
        var request = new APIGatewayProxyRequest
        {
            Body = null
        };
        var context = new TestLambdaContext();
        var function = new Function(mockLogger.Object, mockChatService.Object, mockProfileRepo.Object);

        // Act
        var response = await function.FunctionHandler(request, context);

        // Assert
        Assert.Equal(400, response.StatusCode);
    }

    [Fact]
    public async Task TestFunctionHandler_WithInvalidJson_Returns400()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<Function>>();
        var mockChatService = new Mock<IBedrockChatService>();
        var mockProfileRepo = new Mock<IProfileRepository>();
        
        var request = new APIGatewayProxyRequest
        {
            Body = "invalid json"
        };
        var context = new TestLambdaContext();
        var function = new Function(mockLogger.Object, mockChatService.Object, mockProfileRepo.Object);

        // Act
        var response = await function.FunctionHandler(request, context);

        // Assert
        Assert.Equal(400, response.StatusCode);
    }
}
