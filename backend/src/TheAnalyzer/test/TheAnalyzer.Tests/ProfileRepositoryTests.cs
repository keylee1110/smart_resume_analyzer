using TheAnalyzer.Models;
using Xunit;

namespace TheAnalyzer.Tests;

/// <summary>
/// Unit tests for ProfileRepository data models and key formats
/// Note: Full repository integration tests require DynamoDB Local or AWS environment
/// </summary>
public class ProfileRepositoryTests
{

    [Fact]
    public void ProfileRecord_PKFormat_IsCorrect()
    {
        // Arrange
        var profile = new ProfileRecord
        {
            ResumeId = "test-789"
        };

        // Act
        var pk = profile.PK;

        // Assert
        Assert.Equal("RESUME#test-789", pk);
    }

    [Fact]
    public void ProfileRecord_SKFormat_IsCorrect()
    {
        // Arrange
        var profile = new ProfileRecord
        {
            ResumeId = "test-789"
        };

        // Act
        var sk = profile.SK;

        // Assert
        Assert.Equal("PROFILE", sk);
    }

    [Fact]
    public void AnalysisRecord_PKFormat_IsCorrect()
    {
        // Arrange
        var analysis = new AnalysisRecord
        {
            ResumeId = "test-789"
        };

        // Act
        var pk = analysis.PK;

        // Assert
        Assert.Equal("RESUME#test-789", pk);
    }

    [Fact]
    public void AnalysisRecord_SKFormat_StartsWithAnalysis()
    {
        // Arrange
        var analysis = new AnalysisRecord
        {
            ResumeId = "test-789",
            AnalysisTimestamp = DateTime.UtcNow
        };

        // Act
        var sk = analysis.SK;

        // Assert
        Assert.StartsWith("ANALYSIS#", sk);
    }
}
