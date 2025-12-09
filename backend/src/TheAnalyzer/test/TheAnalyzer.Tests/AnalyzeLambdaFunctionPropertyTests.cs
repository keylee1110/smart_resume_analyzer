using Xunit;
using FsCheck;
using FsCheck.Xunit;
using TheAnalyzer.Handlers;
using TheAnalyzer.Interfaces;
using TheAnalyzer.Models;
using TheAnalyzer.Exceptions;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Moq;
using System.Text.Json;

namespace TheAnalyzer.Tests;

/// <summary>
/// Property-based tests for AnalyzeLambdaFunction
/// </summary>
public class AnalyzeLambdaFunctionPropertyTests
{
    private static APIGatewayProxyRequest CreateProxyRequest(AnalyzeRequest request, string userId = "test-user-id")
    {
        return new APIGatewayProxyRequest
        {
            Body = JsonSerializer.Serialize(request),
            RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
            {
                Authorizer = new APIGatewayCustomAuthorizerContext
                {
                    Claims = new Dictionary<string, string> { { "sub", userId } }
                }
            }
        };
    }
    
    /// <summary>
    /// Feature: resume-analyzer, Property 3: Identifier preservation
    /// For any resume identifier provided in the request, the identifier should appear 
    /// unchanged in the analysis response and all stored database records.
    /// Validates: Requirements 1.4
    /// </summary>
    [Property(MaxTest = 100)]
    public Property IdentifierPreservation()
    {
        return Prop.ForAll<Guid>(guid =>
        {
            // Arrange
            var resumeId = guid.ToString();
            var request = new AnalyzeRequest
            {
                ResumeId = resumeId,
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
            
            var mockAnalyzeService = new Mock<IAnalyzeService>();
            mockAnalyzeService.Setup(s => s.AnalyzeCvAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(analysisResult);
            
            ProfileRecord? savedProfile = null;
            AnalysisRecord? savedAnalysis = null;
            
            var mockRepository = new Mock<IProfileRepository>();
            mockRepository.Setup(r => r.SaveProfileAsync(It.IsAny<ProfileRecord>(), It.IsAny<CancellationToken>()))
                .Callback<ProfileRecord, CancellationToken>((p, ct) => savedProfile = p)
                .Returns(Task.CompletedTask);
            mockRepository.Setup(r => r.SaveAnalysisAsync(It.IsAny<AnalysisRecord>(), It.IsAny<CancellationToken>()))
                .Callback<AnalysisRecord, CancellationToken>((a, ct) => savedAnalysis = a)
                .Returns(Task.CompletedTask);
            
            var mockContext = new Mock<ILambdaContext>();
            var mockLogger = new Mock<ILambdaLogger>();
            mockContext.Setup(c => c.Logger).Returns(mockLogger.Object);
            
            var function = new AnalyzeLambdaFunction(mockAnalyzeService.Object, mockRepository.Object, mockLogger.Object);
            var proxyRequest = CreateProxyRequest(request);
            
            // Act
            var response = function.FunctionHandler(proxyRequest, mockContext.Object).Result;
            var responseBody = JsonSerializer.Deserialize<AnalyzeResponse>(response.Body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            // Assert
            return (responseBody?.ResumeId == resumeId)
                .Label($"Response ResumeId should match: expected {resumeId}, got {responseBody?.ResumeId}")
                .And(savedProfile != null && savedProfile.ResumeId == resumeId)
                .Label($"Saved profile ResumeId should match: expected {resumeId}, got {savedProfile?.ResumeId}")
                .And(savedAnalysis != null && savedAnalysis.ResumeId == resumeId)
                .Label($"Saved analysis ResumeId should match: expected {resumeId}, got {savedAnalysis?.ResumeId}");
        });
    }
    
    /// <summary>
    /// Feature: resume-analyzer, Property 13: Successful extraction triggers storage
    /// For any CV text that is successfully processed and entities extracted, 
    /// a profile record should be written to DynamoDB.
    /// Validates: Requirements 5.1
    /// </summary>
    [Property(MaxTest = 100)]
    public bool SuccessfulExtractionTriggersStorage(NonEmptyString cvTextGen)
    {
        // Arrange
        var cvText = cvTextGen.Get;
        if (string.IsNullOrWhiteSpace(cvText))
        {
            return true; // Skip invalid inputs
        }
        
        var request = new AnalyzeRequest
        {
            ResumeText = cvText
        };
        
        var analysisResult = new AnalysisResult
        {
            ResumeEntities = new ExtractedEntities
            {
                Name = "Test Name",
                Email = "test@example.com",
                Phone = "+1-555-0123",
                Skills = new List<string> { "C#" },
                Method = "Comprehend",
                TotalEntitiesFound = 4
            }
        };
        
        var mockAnalyzeService = new Mock<IAnalyzeService>();
        mockAnalyzeService.Setup(s => s.AnalyzeCvAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(analysisResult);
        
        bool profileSaved = false;
        
        var mockRepository = new Mock<IProfileRepository>();
        mockRepository.Setup(r => r.SaveProfileAsync(It.IsAny<ProfileRecord>(), It.IsAny<CancellationToken>()))
            .Callback<ProfileRecord, CancellationToken>((p, ct) => profileSaved = true)
            .Returns(Task.CompletedTask);
        mockRepository.Setup(r => r.SaveAnalysisAsync(It.IsAny<AnalysisRecord>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        var mockContext = new Mock<ILambdaContext>();
        var mockLogger = new Mock<ILambdaLogger>();
        mockContext.Setup(c => c.Logger).Returns(mockLogger.Object);
        
        var function = new AnalyzeLambdaFunction(mockAnalyzeService.Object, mockRepository.Object, mockLogger.Object);
        var proxyRequest = CreateProxyRequest(request);
        
        // Act
        var response = function.FunctionHandler(proxyRequest, mockContext.Object).Result;
        var responseBody = JsonSerializer.Deserialize<AnalyzeResponse>(response.Body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        // Assert
        return profileSaved && !string.IsNullOrEmpty(responseBody?.ResumeId);
    }
    
    /// <summary>
    /// Feature: resume-analyzer, Property 20: Error response structure
    /// For any operation that fails due to empty body, the system should return 
    /// a structured error response containing an error message and appropriate HTTP status code.
    /// Validates: Requirements 9.2
    /// </summary>
    [Property(MaxTest = 100)]
    public Property ErrorResponseStructure()
    {
        return Prop.ForAll(
            InvalidInputGenerator(),
            invalidInput =>
            {
                // Arrange
                var mockAnalyzeService = new Mock<IAnalyzeService>();
                var mockRepository = new Mock<IProfileRepository>();
                
                var mockContext = new Mock<ILambdaContext>();
                var mockLogger = new Mock<ILambdaLogger>();
                mockContext.Setup(c => c.Logger).Returns(mockLogger.Object);
                
                var function = new AnalyzeLambdaFunction(mockAnalyzeService.Object, mockRepository.Object, mockLogger.Object);
                
                var proxyRequest = new APIGatewayProxyRequest
                {
                    Body = invalidInput,
                    RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
                    {
                        Authorizer = new APIGatewayCustomAuthorizerContext
                        {
                            Claims = new Dictionary<string, string> { { "sub", "test-user" } }
                        }
                    }
                };
                
                // Act
                var response = function.FunctionHandler(proxyRequest, mockContext.Object).Result;
                
                // Assert - Should return 400 for invalid/empty body
                return response.StatusCode == 400 && !string.IsNullOrEmpty(response.Body);
            });
    }
    
    /// <summary>
    /// Generator for invalid input strings (empty or whitespace bodies)
    /// </summary>
    private static Arbitrary<string> InvalidInputGenerator()
    {
        var gen = Gen.OneOf(
            Gen.Constant(""),
            Gen.Constant((string)null!)
        );
        return Arb.From(gen);
    }
}
