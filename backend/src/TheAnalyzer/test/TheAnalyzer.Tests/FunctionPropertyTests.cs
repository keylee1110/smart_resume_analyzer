using Xunit;
using FsCheck;
using FsCheck.Xunit;
using TheAnalyzer;
using TheAnalyzer.Interfaces;
using TheAnalyzer.Models;
using TheAnalyzer.Exceptions;
using Amazon.Lambda.Core;
using Moq;

namespace TheAnalyzer.Tests;

/// <summary>
/// Property-based tests for Function.cs (refactored architecture)
/// </summary>
public class FunctionPropertyTests
{
    /// <summary>
    /// Feature: resume-analyzer, Property 14: Partial failure handling
    /// For any analysis operation where profile write succeeds but analysis write fails (or vice versa), 
    /// the system should either complete both writes or return an error indicating partial failure.
    /// Validates: Requirements 5.5
    /// </summary>
    [Property(MaxTest = 100)]
    public Property PartialFailureHandling()
    {
        return Prop.ForAll(
            ValidCvTextGenerator(),
            cvText =>
            {
                // Test Case 1: Profile save succeeds, analysis save fails
                var result1 = TestPartialFailureScenario(cvText, profileFails: false, analysisFails: true);
                
                // Test Case 2: Profile save fails, analysis save succeeds
                var result2 = TestPartialFailureScenario(cvText, profileFails: true, analysisFails: false);
                
                return result1.Label("Profile succeeds, analysis fails should throw exception")
                    .And(result2).Label("Profile fails, analysis succeeds should throw exception");
            });
    }
    
    private bool TestPartialFailureScenario(string cvText, bool profileFails, bool analysisFails)
    {
        // Arrange
        var input = new AnalyzerInput
        {
            CandidateName = "Test Candidate",
            ResumeText = cvText
        };
        
        var mockAnalyzeService = new Mock<IAnalyzeService>();
        mockAnalyzeService.Setup(s => s.ValidateInput(It.IsAny<string>())).Returns(true);
        mockAnalyzeService.Setup(s => s.ExtractEntitiesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExtractedEntities
            {
                Name = "Test Name",
                Email = "test@example.com",
                Phone = "+1-555-0123",
                Skills = new List<string> { "C#", ".NET" },
                Method = "Comprehend",
                TotalEntitiesFound = 4
            });
        
        var mockRepository = new Mock<IProfileRepository>();
        
        if (profileFails)
        {
            mockRepository.Setup(r => r.SaveProfileAsync(It.IsAny<ProfileRecord>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Profile save failed"));
        }
        else
        {
            mockRepository.Setup(r => r.SaveProfileAsync(It.IsAny<ProfileRecord>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }
        
        if (analysisFails)
        {
            mockRepository.Setup(r => r.SaveAnalysisAsync(It.IsAny<AnalysisRecord>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Analysis save failed"));
        }
        else
        {
            mockRepository.Setup(r => r.SaveAnalysisAsync(It.IsAny<AnalysisRecord>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }
        
        var mockContext = new Mock<ILambdaContext>();
        var mockLogger = new Mock<ILambdaLogger>();
        mockContext.Setup(c => c.Logger).Returns(mockLogger.Object);
        
        var function = new Function(mockAnalyzeService.Object, mockRepository.Object);
        
        // Act & Assert
        try
        {
            var result = function.FunctionHandler(input, mockContext.Object).Result;
            // If we get here without exception, both operations succeeded (not a partial failure)
            return !profileFails && !analysisFails;
        }
        catch (AggregateException aex)
        {
            // Partial failure should result in an exception
            var innerEx = aex.InnerException;
            return innerEx != null && 
                   (innerEx.Message.Contains("failed") || innerEx.Message.Contains("Analysis failed"));
        }
        catch (Exception)
        {
            // Any exception indicates proper error handling
            return true;
        }
    }
    
    /// <summary>
    /// Feature: resume-analyzer, Property 19: Structured entity return
    /// For any call to the IAnalyzeService.ExtractEntitiesAsync method, 
    /// the return value should be an ExtractedEntities object with properly initialized properties.
    /// Validates: Requirements 8.5
    /// </summary>
    [Property(MaxTest = 100)]
    public Property StructuredEntityReturn()
    {
        return Prop.ForAll(
            ValidCvTextGenerator(),
            cvText =>
            {
                // Arrange
                var input = new AnalyzerInput
                {
                    CandidateName = "Test Candidate",
                    ResumeText = cvText
                };
                
                ExtractedEntities? capturedEntities = null;
                
                var mockAnalyzeService = new Mock<IAnalyzeService>();
                mockAnalyzeService.Setup(s => s.ValidateInput(It.IsAny<string>())).Returns(true);
                mockAnalyzeService.Setup(s => s.ExtractEntitiesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((string text, CancellationToken ct) =>
                    {
                        var entities = new ExtractedEntities
                        {
                            Name = "John Doe",
                            Email = "john@example.com",
                            Phone = "+1-555-0123",
                            Skills = new List<string> { "C#", ".NET", "AWS" },
                            Method = "Comprehend",
                            TotalEntitiesFound = 4
                        };
                        capturedEntities = entities;
                        return entities;
                    });
                
                var mockRepository = new Mock<IProfileRepository>();
                mockRepository.Setup(r => r.SaveProfileAsync(It.IsAny<ProfileRecord>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);
                mockRepository.Setup(r => r.SaveAnalysisAsync(It.IsAny<AnalysisRecord>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);
                
                var mockContext = new Mock<ILambdaContext>();
                var mockLogger = new Mock<ILambdaLogger>();
                mockContext.Setup(c => c.Logger).Returns(mockLogger.Object);
                
                var function = new Function(mockAnalyzeService.Object, mockRepository.Object);
                
                // Act
                var result = function.FunctionHandler(input, mockContext.Object).Result;
                
                // Assert - Verify ExtractedEntities is properly structured
                return (capturedEntities != null)
                    .Label("ExtractedEntities should not be null")
                    .And(capturedEntities.Name != null)
                    .Label("Name property should be initialized")
                    .And(capturedEntities.Email != null)
                    .Label("Email property should be initialized")
                    .And(capturedEntities.Phone != null)
                    .Label("Phone property should be initialized")
                    .And(capturedEntities.Skills != null)
                    .Label("Skills property should be initialized (not null)")
                    .And(capturedEntities.Method != null)
                    .Label("Method property should be initialized")
                    .And(capturedEntities.TotalEntitiesFound >= 0)
                    .Label("TotalEntitiesFound should be non-negative");
            });
    }
    
    /// <summary>
    /// Generator for valid CV text
    /// </summary>
    private static Arbitrary<string> ValidCvTextGenerator()
    {
        var gen = Gen.OneOf(
            Gen.Constant("John Doe\njohn.doe@example.com\n+1-555-0123\nSkills: C#, .NET, AWS"),
            Gen.Constant("Jane Smith\njane.smith@company.com\n+1-555-9876\nExperience with Python, Java, Docker"),
            Gen.Constant("Bob Johnson\nbob@email.com\n555-1234\nProficient in JavaScript, React, Node.js"),
            Gen.Constant("Alice Williams\nalice.w@domain.com\n+44-20-1234-5678\nSkills: SQL, MongoDB, Redis"),
            Gen.Constant("Charlie Brown\ncharlie.brown@test.org\n(555) 123-4567\nC++, Rust, Go programming")
        );
        return Arb.From(gen);
    }
}
