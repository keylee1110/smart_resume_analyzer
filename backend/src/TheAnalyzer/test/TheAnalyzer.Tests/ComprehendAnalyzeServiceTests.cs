using Amazon.Comprehend;
using Amazon.Comprehend.Model;
using FsCheck;
using FsCheck.Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using TheAnalyzer.Models;
using TheAnalyzer.Services;
using Xunit;

namespace TheAnalyzer.Tests;

/// <summary>
/// Unit tests for ComprehendAnalyzeService
/// </summary>
public class ComprehendAnalyzeServiceTests
{
    private readonly Mock<IAmazonComprehend> _mockComprehend;
    private readonly Mock<ILogger<ComprehendAnalyzeService>> _mockLogger;
    private readonly RegexEntityExtractor _regexFallback;
    private readonly ComprehendAnalyzeService _service;
    
    public ComprehendAnalyzeServiceTests()
    {
        _mockComprehend = new Mock<IAmazonComprehend>();
        _mockLogger = new Mock<ILogger<ComprehendAnalyzeService>>();
        _regexFallback = new RegexEntityExtractor();
        _service = new ComprehendAnalyzeService(_mockComprehend.Object, _mockLogger.Object, _regexFallback);
    }
    
    [Fact]
    public void ValidateInput_WithNullText_ReturnsFalse()
    {
        // Act
        var result = _service.ValidateInput(null!);
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void ValidateInput_WithEmptyText_ReturnsFalse()
    {
        // Act
        var result = _service.ValidateInput("");
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void ValidateInput_WithWhitespaceText_ReturnsFalse()
    {
        // Act
        var result = _service.ValidateInput("   \n\t   ");
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void ValidateInput_WithValidText_ReturnsTrue()
    {
        // Act
        var result = _service.ValidateInput("John Doe\nSoftware Engineer");
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public async Task ExtractEntitiesAsync_ComprehendThrowsException_UsesFallback()
    {
        // Arrange
        var cvText = "John Doe\nEmail: john@example.com\nPhone: 555-123-4567\nSkills: C#, AWS";
        
        _mockComprehend
            .Setup(x => x.DetectEntitiesAsync(It.IsAny<DetectEntitiesRequest>(), default))
            .ThrowsAsync(new AmazonComprehendException("Service unavailable"));
        
        // Act
        var result = await _service.ExtractEntitiesAsync(cvText);
        
        // Assert
        Assert.Equal("Regex", result.Method);
        Assert.NotNull(result.Email);
        Assert.Equal("john@example.com", result.Email);
    }
    
    [Fact]
    public async Task ExtractEntitiesAsync_ComprehendFails_SetsMethodToRegex()
    {
        // Arrange
        var cvText = "Test CV text";
        
        _mockComprehend
            .Setup(x => x.DetectEntitiesAsync(It.IsAny<DetectEntitiesRequest>(), default))
            .ThrowsAsync(new Exception("Network error"));
        
        // Act
        var result = await _service.ExtractEntitiesAsync(cvText);
        
        // Assert
        Assert.Equal("Regex", result.Method);
    }
    
    [Fact]
    public async Task ExtractEntitiesAsync_ComprehendSucceeds_SetsMethodToComprehend()
    {
        // Arrange
        var cvText = "John Doe is a software engineer. Email: john@example.com";
        
        var comprehendResponse = new DetectEntitiesResponse
        {
            Entities = new List<Entity>
            {
                new Entity
                {
                    Type = EntityType.PERSON,
                    Text = "John Doe",
                    Score = 0.99f
                }
            }
        };
        
        _mockComprehend
            .Setup(x => x.DetectEntitiesAsync(It.IsAny<DetectEntitiesRequest>(), default))
            .ReturnsAsync(comprehendResponse);
        
        // Act
        var result = await _service.ExtractEntitiesAsync(cvText);
        
        // Assert
        Assert.Equal("Comprehend", result.Method);
    }
    
    [Fact]
    public async Task ExtractEntitiesAsync_ComprehendReturnsPersonEntity_ExtractsName()
    {
        // Arrange
        var cvText = "Jane Smith is a data scientist.";
        
        var comprehendResponse = new DetectEntitiesResponse
        {
            Entities = new List<Entity>
            {
                new Entity
                {
                    Type = EntityType.PERSON,
                    Text = "Jane Smith",
                    Score = 0.95f
                }
            }
        };
        
        _mockComprehend
            .Setup(x => x.DetectEntitiesAsync(It.IsAny<DetectEntitiesRequest>(), default))
            .ReturnsAsync(comprehendResponse);
        
        // Act
        var result = await _service.ExtractEntitiesAsync(cvText);
        
        // Assert
        Assert.Equal("Jane Smith", result.Name);
        Assert.Equal("Comprehend", result.Method);
    }
    
    [Fact]
    public async Task ExtractEntitiesAsync_ComprehendReturnsMultiplePersons_ExtractsHighestScore()
    {
        // Arrange
        var cvText = "John Doe worked with Jane Smith at ABC Corp.";
        
        var comprehendResponse = new DetectEntitiesResponse
        {
            Entities = new List<Entity>
            {
                new Entity
                {
                    Type = EntityType.PERSON,
                    Text = "John Doe",
                    Score = 0.99f
                },
                new Entity
                {
                    Type = EntityType.PERSON,
                    Text = "Jane Smith",
                    Score = 0.85f
                }
            }
        };
        
        _mockComprehend
            .Setup(x => x.DetectEntitiesAsync(It.IsAny<DetectEntitiesRequest>(), default))
            .ReturnsAsync(comprehendResponse);
        
        // Act
        var result = await _service.ExtractEntitiesAsync(cvText);
        
        // Assert
        Assert.Equal("John Doe", result.Name); // Highest score
    }
    
    [Fact]
    public async Task ExtractEntitiesAsync_WithEmail_ExtractsEmail()
    {
        // Arrange
        var cvText = "Contact: test@example.com";
        
        var comprehendResponse = new DetectEntitiesResponse
        {
            Entities = new List<Entity>()
        };
        
        _mockComprehend
            .Setup(x => x.DetectEntitiesAsync(It.IsAny<DetectEntitiesRequest>(), default))
            .ReturnsAsync(comprehendResponse);
        
        // Act
        var result = await _service.ExtractEntitiesAsync(cvText);
        
        // Assert
        Assert.Equal("test@example.com", result.Email);
    }
    
    [Fact]
    public async Task ExtractEntitiesAsync_WithPhone_ExtractsPhone()
    {
        // Arrange
        var cvText = "Phone: +1-555-987-6543";
        
        var comprehendResponse = new DetectEntitiesResponse
        {
            Entities = new List<Entity>()
        };
        
        _mockComprehend
            .Setup(x => x.DetectEntitiesAsync(It.IsAny<DetectEntitiesRequest>(), default))
            .ReturnsAsync(comprehendResponse);
        
        // Act
        var result = await _service.ExtractEntitiesAsync(cvText);
        
        // Assert
        Assert.Equal("+1-555-987-6543", result.Phone);
    }
    
    [Fact]
    public async Task ExtractEntitiesAsync_WithSkills_ExtractsSkills()
    {
        // Arrange
        var cvText = "I have experience with C#, AWS, Python, and Docker.";
        
        var comprehendResponse = new DetectEntitiesResponse
        {
            Entities = new List<Entity>()
        };
        
        _mockComprehend
            .Setup(x => x.DetectEntitiesAsync(It.IsAny<DetectEntitiesRequest>(), default))
            .ReturnsAsync(comprehendResponse);
        
        // Act
        var result = await _service.ExtractEntitiesAsync(cvText);
        
        // Assert
        Assert.Contains("C#", result.Skills);
        Assert.Contains("AWS", result.Skills);
        Assert.Contains("Python", result.Skills);
        Assert.Contains("Docker", result.Skills);
    }
    
    [Fact]
    public async Task ExtractEntitiesAsync_WithAllEntities_CountsCorrectly()
    {
        // Arrange
        var cvText = "John Doe\nEmail: john@example.com\nPhone: 555-123-4567\nSkills: C#, AWS";
        
        var comprehendResponse = new DetectEntitiesResponse
        {
            Entities = new List<Entity>
            {
                new Entity
                {
                    Type = EntityType.PERSON,
                    Text = "John Doe",
                    Score = 0.99f
                }
            }
        };
        
        _mockComprehend
            .Setup(x => x.DetectEntitiesAsync(It.IsAny<DetectEntitiesRequest>(), default))
            .ReturnsAsync(comprehendResponse);
        
        // Act
        var result = await _service.ExtractEntitiesAsync(cvText);
        
        // Assert
        Assert.True(result.TotalEntitiesFound >= 4); // name + email + phone + at least 2 skills
    }
    
    [Fact]
    public async Task ExtractEntitiesAsync_ComprehendFails_LogsWarning()
    {
        // Arrange
        var cvText = "Test CV";
        
        _mockComprehend
            .Setup(x => x.DetectEntitiesAsync(It.IsAny<DetectEntitiesRequest>(), default))
            .ThrowsAsync(new AmazonComprehendException("Service error"));
        
        // Act
        await _service.ExtractEntitiesAsync(cvText);
        
        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Comprehend failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}

/// <summary>
/// Property-based tests for ComprehendAnalyzeService
/// </summary>
public class ComprehendAnalyzeServicePropertyTests
{
    private readonly Mock<IAmazonComprehend> _mockComprehend;
    private readonly Mock<ILogger<ComprehendAnalyzeService>> _mockLogger;
    private readonly RegexEntityExtractor _regexFallback;
    
    public ComprehendAnalyzeServicePropertyTests()
    {
        _mockComprehend = new Mock<IAmazonComprehend>();
        _mockLogger = new Mock<ILogger<ComprehendAnalyzeService>>();
        _regexFallback = new RegexEntityExtractor();
    }
    
    // ============================================================================
    // Custom Generators for Property-Based Testing
    // ============================================================================
    
    /// <summary>
    /// Generates CV text with embedded email addresses
    /// </summary>
    public static Gen<string> CVWithEmailGenerator()
    {
        var emailGen = Gen.Elements(
            "john.doe@example.com",
            "jane_smith@company.com",
            "user+tag@domain.org",
            "test.user@mail.co.uk"
        );
        
        var templateGen = Gen.Elements(
            "Contact me at {0}",
            "Email: {0}",
            "Reach out to {0} for inquiries",
            "My email is {0}"
        );
        
        return from email in emailGen
               from template in templateGen
               select string.Format(template, email);
    }
    
    /// <summary>
    /// Generates CV text with embedded phone numbers
    /// </summary>
    public static Gen<string> CVWithPhoneGenerator()
    {
        var phoneGen = Gen.Elements(
            "+1-555-123-4567",
            "(555) 987-6543",
            "555.123.4567",
            "5551234567"
        );
        
        var templateGen = Gen.Elements(
            "Call me at {0}",
            "Phone: {0}",
            "Mobile: {0}",
            "Contact number: {0}"
        );
        
        return from phone in phoneGen
               from template in templateGen
               select string.Format(template, phone);
    }
    
    /// <summary>
    /// Generates CV text with known skills
    /// </summary>
    public static Gen<string> CVWithSkillsGenerator()
    {
        var skillSets = new[]
        {
            new[] { "C#", ".NET", "AWS" },
            new[] { "Python", "Django", "Docker" },
            new[] { "JavaScript", "React", "Node.js" },
            new[] { "Java", "Spring", "Kubernetes" }
        };
        
        return from skillSet in Gen.Elements(skillSets)
               select $"Skills: {string.Join(", ", skillSet)}";
    }
    
    // ============================================================================
    // Property 5: Email extraction completeness
    // Feature: resume-analyzer, Property 5: Email extraction completeness
    // Validates: Requirements 2.3
    // ============================================================================
    
    [Property(MaxTest = 100)]
    public Property EmailExtraction_WithEmailInCV_ExtractsEmail()
    {
        // Feature: resume-analyzer, Property 5: Email extraction completeness
        return Prop.ForAll(
            Arb.From(CVWithEmailGenerator()),
            cvText =>
            {
                // Arrange
                var comprehendResponse = new DetectEntitiesResponse
                {
                    Entities = new List<Entity>()
                };
                
                _mockComprehend
                    .Setup(x => x.DetectEntitiesAsync(It.IsAny<DetectEntitiesRequest>(), default))
                    .ReturnsAsync(comprehendResponse);
                
                var service = new ComprehendAnalyzeService(_mockComprehend.Object, _mockLogger.Object, _regexFallback);
                
                // Act
                var result = service.ExtractEntitiesAsync(cvText).Result;
                
                // Assert
                return result.Email != null &&
                       result.Email.Contains("@") &&
                       result.Method == "Comprehend";
            });
    }
    
    [Property(MaxTest = 100)]
    public Property EmailExtraction_ComprehendFails_FallbackExtractsEmail()
    {
        // Feature: resume-analyzer, Property 5: Email extraction completeness
        return Prop.ForAll(
            Arb.From(CVWithEmailGenerator()),
            cvText =>
            {
                // Arrange
                _mockComprehend
                    .Setup(x => x.DetectEntitiesAsync(It.IsAny<DetectEntitiesRequest>(), default))
                    .ThrowsAsync(new AmazonComprehendException("Service error"));
                
                var service = new ComprehendAnalyzeService(_mockComprehend.Object, _mockLogger.Object, _regexFallback);
                
                // Act
                var result = service.ExtractEntitiesAsync(cvText).Result;
                
                // Assert
                return result.Email != null &&
                       result.Email.Contains("@") &&
                       result.Method == "Regex";
            });
    }
    
    // ============================================================================
    // Property 6: Phone extraction completeness
    // Feature: resume-analyzer, Property 6: Phone extraction completeness
    // Validates: Requirements 2.4
    // ============================================================================
    
    [Property(MaxTest = 100)]
    public Property PhoneExtraction_WithPhoneInCV_ExtractsPhone()
    {
        // Feature: resume-analyzer, Property 6: Phone extraction completeness
        return Prop.ForAll(
            Arb.From(CVWithPhoneGenerator()),
            cvText =>
            {
                // Arrange
                var comprehendResponse = new DetectEntitiesResponse
                {
                    Entities = new List<Entity>()
                };
                
                _mockComprehend
                    .Setup(x => x.DetectEntitiesAsync(It.IsAny<DetectEntitiesRequest>(), default))
                    .ReturnsAsync(comprehendResponse);
                
                var service = new ComprehendAnalyzeService(_mockComprehend.Object, _mockLogger.Object, _regexFallback);
                
                // Act
                var result = service.ExtractEntitiesAsync(cvText).Result;
                
                // Assert
                return result.Phone != null &&
                       result.Phone.Any(char.IsDigit) &&
                       result.Method == "Comprehend";
            });
    }
    
    [Property(MaxTest = 100)]
    public Property PhoneExtraction_ComprehendFails_FallbackExtractsPhone()
    {
        // Feature: resume-analyzer, Property 6: Phone extraction completeness
        return Prop.ForAll(
            Arb.From(CVWithPhoneGenerator()),
            cvText =>
            {
                // Arrange
                _mockComprehend
                    .Setup(x => x.DetectEntitiesAsync(It.IsAny<DetectEntitiesRequest>(), default))
                    .ThrowsAsync(new AmazonComprehendException("Service error"));
                
                var service = new ComprehendAnalyzeService(_mockComprehend.Object, _mockLogger.Object, _regexFallback);
                
                // Act
                var result = service.ExtractEntitiesAsync(cvText).Result;
                
                // Assert
                return result.Phone != null &&
                       result.Phone.Any(char.IsDigit) &&
                       result.Method == "Regex";
            });
    }
    
    // ============================================================================
    // Property 7: Skill extraction accuracy
    // Feature: resume-analyzer, Property 7: Skill extraction accuracy
    // Validates: Requirements 2.5
    // ============================================================================
    
    [Property(MaxTest = 100)]
    public Property SkillExtraction_WithKnownSkills_ExtractsSkills()
    {
        // Feature: resume-analyzer, Property 7: Skill extraction accuracy
        return Prop.ForAll(
            Arb.From(CVWithSkillsGenerator()),
            cvText =>
            {
                // Arrange
                var comprehendResponse = new DetectEntitiesResponse
                {
                    Entities = new List<Entity>()
                };
                
                _mockComprehend
                    .Setup(x => x.DetectEntitiesAsync(It.IsAny<DetectEntitiesRequest>(), default))
                    .ReturnsAsync(comprehendResponse);
                
                var service = new ComprehendAnalyzeService(_mockComprehend.Object, _mockLogger.Object, _regexFallback);
                
                // Act
                var result = service.ExtractEntitiesAsync(cvText).Result;
                
                // Assert
                return result.Skills.Count > 0 &&
                       result.Method == "Comprehend";
            });
    }
    
    [Property(MaxTest = 100)]
    public Property SkillExtraction_ComprehendFails_FallbackExtractsSkills()
    {
        // Feature: resume-analyzer, Property 7: Skill extraction accuracy
        return Prop.ForAll(
            Arb.From(CVWithSkillsGenerator()),
            cvText =>
            {
                // Arrange
                _mockComprehend
                    .Setup(x => x.DetectEntitiesAsync(It.IsAny<DetectEntitiesRequest>(), default))
                    .ThrowsAsync(new AmazonComprehendException("Service error"));
                
                var service = new ComprehendAnalyzeService(_mockComprehend.Object, _mockLogger.Object, _regexFallback);
                
                // Act
                var result = service.ExtractEntitiesAsync(cvText).Result;
                
                // Assert
                return result.Skills.Count > 0 &&
                       result.Method == "Regex";
            });
    }
}
