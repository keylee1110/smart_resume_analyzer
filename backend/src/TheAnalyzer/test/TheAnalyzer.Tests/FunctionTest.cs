using Xunit;
using TheAnalyzer.Models;

namespace TheAnalyzer.Tests;

public class FunctionTest
{
    [Fact]
    public void TestProjectStructureSetup()
    {
        // Verify that core models can be instantiated
        var request = new AnalyzeRequest
        {
            ResumeId = "test-123",
            ResumeText = "Test resume text"
        };

        var response = new AnalyzeResponse
        {
            ResumeId = "test-123",
            Message = "Success"
        };

        var entities = new ExtractedEntities
        {
            Name = "John Doe",
            Email = "john@example.com",
            Phone = "+1-555-0123",
            Skills = new List<string> { "C#", ".NET" },
            Method = "Comprehend",
            TotalEntitiesFound = 4
        };

        var profile = new ProfileRecord
        {
            ResumeId = "test-123",
            Name = "John Doe",
            Email = "john@example.com",
            Phone = "+1-555-0123",
            Skills = new List<string> { "C#", ".NET" },
            CreatedAt = DateTime.UtcNow
        };

        var analysis = new AnalysisRecord
        {
            ResumeId = "test-123",
            AnalysisTimestamp = DateTime.UtcNow,
            ExtractionMethod = "Comprehend",
            EntityCount = 4,
            CreatedAt = DateTime.UtcNow
        };

        // Assert that all models are properly initialized
        Assert.NotNull(request);
        Assert.Equal("test-123", request.ResumeId);
        Assert.NotNull(response);
        Assert.Equal("Success", response.Message);
        Assert.NotNull(entities);
        Assert.Equal("John Doe", entities.Name);
        Assert.NotNull(profile);
        Assert.Equal("RESUME#test-123", profile.PK);
        Assert.Equal("PROFILE", profile.SK);
        Assert.NotNull(analysis);
        Assert.StartsWith("ANALYSIS#", analysis.SK);
    }
}
