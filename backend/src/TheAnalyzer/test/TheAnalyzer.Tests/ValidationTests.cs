using Xunit;
using TheAnalyzer.Services;
using TheAnalyzer.Models;
using TheAnalyzer.Exceptions;

namespace TheAnalyzer.Tests;

/// <summary>
/// Tests for input validation logic
/// </summary>
public class ValidationTests
{
    private readonly AnalyzeService _service;

    public ValidationTests()
    {
        _service = new AnalyzeService();
    }

    #region CV Text Validation Tests

    [Fact]
    public void ValidateInput_NullText_ReturnsFalse()
    {
        // Arrange
        string? cvText = null;

        // Act
        var result = _service.ValidateInput(cvText!);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ValidateInput_EmptyText_ReturnsFalse()
    {
        // Arrange
        var cvText = string.Empty;

        // Act
        var result = _service.ValidateInput(cvText);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ValidateInput_WhitespaceOnlyText_ReturnsFalse()
    {
        // Arrange
        var cvText = "   \t\n   ";

        // Act
        var result = _service.ValidateInput(cvText);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ValidateInput_ValidText_ReturnsTrue()
    {
        // Arrange
        var cvText = "John Doe\nSoftware Engineer\nSkills: C#, .NET";

        // Act
        var result = _service.ValidateInput(cvText);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ValidateInput_TextWithLeadingTrailingWhitespace_ReturnsTrue()
    {
        // Arrange
        var cvText = "  Valid CV content  ";

        // Act
        var result = _service.ValidateInput(cvText);

        // Assert
        Assert.True(result);
    }

    #endregion

    #region Resume ID Validation Tests

    [Fact]
    public void ValidateResumeId_NullId_ReturnsTrue()
    {
        // Arrange
        string? resumeId = null;

        // Act
        var result = _service.ValidateResumeId(resumeId);

        // Assert
        Assert.True(result); // Null is acceptable, will be generated
    }

    [Fact]
    public void ValidateResumeId_EmptyId_ReturnsTrue()
    {
        // Arrange
        var resumeId = string.Empty;

        // Act
        var result = _service.ValidateResumeId(resumeId);

        // Assert
        Assert.True(result); // Empty is acceptable, will be generated
    }

    [Fact]
    public void ValidateResumeId_ValidAlphanumericId_ReturnsTrue()
    {
        // Arrange
        var resumeId = "resume123";

        // Act
        var result = _service.ValidateResumeId(resumeId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ValidateResumeId_ValidIdWithHyphens_ReturnsTrue()
    {
        // Arrange
        var resumeId = "resume-123-abc";

        // Act
        var result = _service.ValidateResumeId(resumeId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ValidateResumeId_ValidIdWithUnderscores_ReturnsTrue()
    {
        // Arrange
        var resumeId = "resume_123_abc";

        // Act
        var result = _service.ValidateResumeId(resumeId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ValidateResumeId_ValidMixedFormat_ReturnsTrue()
    {
        // Arrange
        var resumeId = "Resume_123-ABC";

        // Act
        var result = _service.ValidateResumeId(resumeId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ValidateResumeId_InvalidWithSpaces_ReturnsFalse()
    {
        // Arrange
        var resumeId = "resume 123";

        // Act
        var result = _service.ValidateResumeId(resumeId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ValidateResumeId_InvalidWithSpecialCharacters_ReturnsFalse()
    {
        // Arrange
        var resumeId = "resume@123";

        // Act
        var result = _service.ValidateResumeId(resumeId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ValidateResumeId_InvalidWithDots_ReturnsFalse()
    {
        // Arrange
        var resumeId = "resume.123";

        // Act
        var result = _service.ValidateResumeId(resumeId);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Request Validation Tests

    [Fact]
    public void ValidateRequest_ValidRequest_DoesNotThrow()
    {
        // Arrange
        var request = new AnalyzeRequest
        {
            ResumeId = "test-123",
            ResumeText = "Valid CV content"
        };

        // Act & Assert
        var exception = Record.Exception(() => _service.ValidateRequest(request));
        Assert.Null(exception);
    }

    [Fact]
    public void ValidateRequest_ValidRequestWithoutResumeId_DoesNotThrow()
    {
        // Arrange
        var request = new AnalyzeRequest
        {
            ResumeId = null,
            ResumeText = "Valid CV content"
        };

        // Act & Assert
        var exception = Record.Exception(() => _service.ValidateRequest(request));
        Assert.Null(exception);
    }

    [Fact]
    public void ValidateRequest_EmptyResumeText_ThrowsValidationException()
    {
        // Arrange
        var request = new AnalyzeRequest
        {
            ResumeId = "test-123",
            ResumeText = ""
        };

        // Act & Assert
        var exception = Assert.Throws<ValidationException>(() => _service.ValidateRequest(request));
        Assert.Equal(400, exception.StatusCode);
        Assert.Contains("CV text cannot be null, empty, or contain only whitespace", exception.Errors);
    }

    [Fact]
    public void ValidateRequest_WhitespaceResumeText_ThrowsValidationException()
    {
        // Arrange
        var request = new AnalyzeRequest
        {
            ResumeId = "test-123",
            ResumeText = "   \t\n   "
        };

        // Act & Assert
        var exception = Assert.Throws<ValidationException>(() => _service.ValidateRequest(request));
        Assert.Equal(400, exception.StatusCode);
        Assert.Contains("CV text cannot be null, empty, or contain only whitespace", exception.Errors);
    }

    [Fact]
    public void ValidateRequest_InvalidResumeId_ThrowsValidationException()
    {
        // Arrange
        var request = new AnalyzeRequest
        {
            ResumeId = "invalid@id",
            ResumeText = "Valid CV content"
        };

        // Act & Assert
        var exception = Assert.Throws<ValidationException>(() => _service.ValidateRequest(request));
        Assert.Equal(400, exception.StatusCode);
        Assert.Contains(exception.Errors, e => e.Contains("Resume identifier contains invalid characters"));
    }

    [Fact]
    public void ValidateRequest_BothInvalid_ThrowsValidationExceptionWithMultipleErrors()
    {
        // Arrange
        var request = new AnalyzeRequest
        {
            ResumeId = "invalid@id",
            ResumeText = ""
        };

        // Act & Assert
        var exception = Assert.Throws<ValidationException>(() => _service.ValidateRequest(request));
        Assert.Equal(400, exception.StatusCode);
        Assert.Equal(2, exception.Errors.Count);
        Assert.Contains(exception.Errors, e => e.Contains("CV text cannot be null, empty, or contain only whitespace"));
        Assert.Contains(exception.Errors, e => e.Contains("Resume identifier contains invalid characters"));
    }

    #endregion

    #region Validation Error Response Tests

    [Fact]
    public void ValidationErrorResponse_CanBeCreated()
    {
        // Arrange & Act
        var response = new ValidationErrorResponse
        {
            Error = "Validation failed",
            ValidationErrors = new List<string> { "Error 1", "Error 2" },
            StatusCode = 400
        };

        // Assert
        Assert.Equal("Validation failed", response.Error);
        Assert.Equal(2, response.ValidationErrors.Count);
        Assert.Equal(400, response.StatusCode);
    }

    [Fact]
    public void ValidationException_ContainsCorrectStatusCode()
    {
        // Arrange & Act
        var exception = new ValidationException("Test error", 400);

        // Assert
        Assert.Equal(400, exception.StatusCode);
        Assert.Equal("Test error", exception.Message);
        Assert.Single(exception.Errors);
    }

    [Fact]
    public void ValidationException_WithMultipleErrors_ContainsAllErrors()
    {
        // Arrange
        var errors = new List<string> { "Error 1", "Error 2", "Error 3" };

        // Act
        var exception = new ValidationException(errors, 400);

        // Assert
        Assert.Equal(400, exception.StatusCode);
        Assert.Equal(3, exception.Errors.Count);
        Assert.Contains("Error 1", exception.Errors);
        Assert.Contains("Error 2", exception.Errors);
        Assert.Contains("Error 3", exception.Errors);
    }

    #endregion
}
