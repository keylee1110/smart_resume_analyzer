using Xunit;
using TheAnalyzer.Services;
using FsCheck;
using FsCheck.Xunit;

namespace TheAnalyzer.Tests;

/// <summary>
/// Unit tests for RegexEntityExtractor
/// </summary>
public class RegexEntityExtractorTests
{
    private readonly RegexEntityExtractor _extractor;
    
    public RegexEntityExtractorTests()
    {
        _extractor = new RegexEntityExtractor();
    }
    
    [Fact]
    public void Extract_WithValidEmail_ExtractsEmail()
    {
        // Arrange
        var cvText = "Contact me at john.doe@example.com for more information.";
        
        // Act
        var result = _extractor.Extract(cvText);
        
        // Assert
        Assert.Equal("john.doe@example.com", result.Email);
        Assert.Equal("Regex", result.Method);
    }
    
    [Fact]
    public void Extract_WithValidPhone_ExtractsPhone()
    {
        // Arrange
        var cvText = "Call me at +1-555-123-4567 anytime.";
        
        // Act
        var result = _extractor.Extract(cvText);
        
        // Assert
        Assert.Equal("+1-555-123-4567", result.Phone);
        Assert.Equal("Regex", result.Method);
    }
    
    [Fact]
    public void Extract_WithNameOnFirstLine_ExtractsName()
    {
        // Arrange
        var cvText = "John Doe\nSoftware Engineer\nEmail: john@example.com";
        
        // Act
        var result = _extractor.Extract(cvText);
        
        // Assert
        Assert.Equal("John Doe", result.Name);
        Assert.Equal("Regex", result.Method);
    }
    
    [Fact]
    public void Extract_WithSkills_ExtractsSkills()
    {
        // Arrange
        var cvText = "I have experience with C#, .NET, AWS, and Python.";
        
        // Act
        var result = _extractor.Extract(cvText);
        
        // Assert
        Assert.Contains("C#", result.Skills);
        Assert.Contains(".NET", result.Skills);
        Assert.Contains("AWS", result.Skills);
        Assert.Contains("Python", result.Skills);
        Assert.Equal("Regex", result.Method);
    }
    
    [Fact]
    public void Extract_WithCompleteCV_ExtractsAllEntities()
    {
        // Arrange
        var cvText = @"Jane Smith
Senior Software Engineer

Contact: jane.smith@techcorp.com
Phone: (555) 987-6543

Skills: C#, .NET, AWS, Docker, Kubernetes, SQL

Experience:
- 5 years in cloud development
- Expert in microservices architecture";
        
        // Act
        var result = _extractor.Extract(cvText);
        
        // Assert
        Assert.Equal("Jane Smith", result.Name);
        Assert.Equal("jane.smith@techcorp.com", result.Email);
        Assert.Equal("(555) 987-6543", result.Phone);
        Assert.Contains("C#", result.Skills);
        Assert.Contains(".NET", result.Skills);
        Assert.Contains("AWS", result.Skills);
        Assert.Contains("Docker", result.Skills);
        Assert.Contains("Kubernetes", result.Skills);
        Assert.Contains("SQL", result.Skills);
        Assert.Equal("Regex", result.Method);
        Assert.True(result.TotalEntitiesFound >= 9); // 1 name + 1 email + 1 phone + 6 skills
    }
    
    [Fact]
    public void Extract_WithEmptyText_ReturnsEmptyEntities()
    {
        // Arrange
        var cvText = "";
        
        // Act
        var result = _extractor.Extract(cvText);
        
        // Assert
        Assert.Null(result.Name);
        Assert.Null(result.Email);
        Assert.Null(result.Phone);
        Assert.Empty(result.Skills);
        Assert.Equal("Regex", result.Method);
        Assert.Equal(0, result.TotalEntitiesFound);
    }
    
    [Fact]
    public void Extract_WithNoEntities_ReturnsNulls()
    {
        // Arrange
        var cvText = "This is just some random text without any useful information.";
        
        // Act
        var result = _extractor.Extract(cvText);
        
        // Assert
        Assert.Equal("Regex", result.Method);
        // Name might be extracted from first line, but email and phone should be null
        Assert.Null(result.Email);
        Assert.Null(result.Phone);
    }
    
    [Theory]
    [InlineData("user@example.com")]
    [InlineData("first.last@company.co.uk")]
    [InlineData("test_user+tag@domain.org")]
    public void Extract_WithVariousEmailFormats_ExtractsEmail(string email)
    {
        // Arrange
        var cvText = $"Contact: {email}";
        
        // Act
        var result = _extractor.Extract(cvText);
        
        // Assert
        Assert.Equal(email, result.Email);
    }
    
    [Theory]
    [InlineData("+1-555-123-4567")]
    [InlineData("(555) 123-4567")]
    [InlineData("555.123.4567")]
    [InlineData("5551234567")]
    public void Extract_WithVariousPhoneFormats_ExtractsPhone(string phone)
    {
        // Arrange
        var cvText = $"Phone: {phone}";
        
        // Act
        var result = _extractor.Extract(cvText);
        
        // Assert
        Assert.NotNull(result.Phone);
        // Verify that the extracted phone contains the core digits
        var expectedDigits = phone.Replace(" ", "").Replace("(", "").Replace(")", "").Replace("-", "").Replace(".", "").Replace("+", "");
        var actualDigits = result.Phone.Replace(" ", "").Replace("(", "").Replace(")", "").Replace("-", "").Replace(".", "").Replace("+", "");
        Assert.Contains(expectedDigits.Substring(expectedDigits.Length - 7), actualDigits);
    }
}

// ============================================================================
// Property-Based Tests using FsCheck
// ============================================================================

/// <summary>
/// Property-based tests for RegexEntityExtractor using FsCheck
/// </summary>
public class RegexEntityExtractorPropertyTests
{
        private readonly RegexEntityExtractor _extractor;
        
        public RegexEntityExtractorPropertyTests()
        {
            _extractor = new RegexEntityExtractor();
        }
        
        // ============================================================================
        // Custom Generators for Property-Based Testing
        // ============================================================================
        
        /// <summary>
        /// Generates valid email addresses in various formats
        /// </summary>
        public static Gen<string> EmailGenerator()
        {
            var usernameGen = Gen.Elements(
                "john.doe", "jane_smith", "user123", "test+tag", 
                "first.last", "a", "user_name", "test.user.name"
            );
            
            var domainGen = Gen.Elements(
                "example.com", "gmail.com", "yahoo.com", "company.co.uk",
                "test.org", "domain.io", "mail.net"
            );
            
            return from username in usernameGen
                   from domain in domainGen
                   select $"{username}@{domain}";
        }
        
        /// <summary>
        /// Generates valid phone numbers in various formats
        /// </summary>
        public static Gen<string> PhoneGenerator()
        {
            var formatGen = Gen.Choose(0, 4);
            var areaCodeGen = Gen.Choose(200, 999);
            var prefixGen = Gen.Choose(200, 999);
            var lineNumberGen = Gen.Choose(1000, 9999);
            
            return from format in formatGen
                   from areaCode in areaCodeGen
                   from prefix in prefixGen
                   from lineNumber in lineNumberGen
                   select format switch
                   {
                       0 => $"+1-{areaCode}-{prefix}-{lineNumber}",
                       1 => $"({areaCode}) {prefix}-{lineNumber}",
                       2 => $"{areaCode}.{prefix}.{lineNumber}",
                       3 => $"{areaCode}{prefix}{lineNumber}",
                       _ => $"+1 {areaCode} {prefix} {lineNumber}"
                   };
        }
        
        /// <summary>
        /// Generates realistic person names
        /// </summary>
        public static Gen<string> NameGenerator()
        {
            var firstNames = new[] { "John", "Jane", "Michael", "Sarah", "David", "Emily", "Robert", "Lisa" };
            var lastNames = new[] { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis" };
            
            return from firstName in Gen.Elements(firstNames)
                   from lastName in Gen.Elements(lastNames)
                   select $"{firstName} {lastName}";
        }
        
        /// <summary>
        /// Generates CV text with a name on the first line
        /// </summary>
        public static Gen<string> CVWithNameGenerator()
        {
            return from name in NameGenerator()
                   from additionalText in Gen.Elements(
                       "\nSoftware Engineer\nExperience: 5 years",
                       "\nSenior Developer\nSkills: C#, AWS",
                       "\nData Scientist\nPython expert"
                   )
                   select name + additionalText;
        }
        
        // ============================================================================
        // Property 8: Regex email pattern matching
        // Feature: resume-analyzer, Property 8: Email extraction completeness
        // Validates: Requirements 3.3
        // ============================================================================
        
        [Property(MaxTest = 100)]
        public Property EmailExtraction_WithValidEmail_AlwaysExtractsEmail()
        {
            // Feature: resume-analyzer, Property 8: Email extraction completeness
            return Prop.ForAll(
                Arb.From(EmailGenerator()),
                email =>
                {
                    // Arrange
                    var cvText = $"Contact me at {email} for more information.";
                    
                    // Act
                    var result = _extractor.Extract(cvText);
                    
                    // Assert
                    return result.Email != null &&
                           result.Email.Contains("@") &&
                           result.Email.Contains(email) &&
                           result.Method == "Regex";
                });
        }
        
        [Property(MaxTest = 100)]
        public Property EmailExtraction_WithMultipleEmails_ExtractsAtLeastOne()
        {
            // Feature: resume-analyzer, Property 8: Email extraction completeness
            return Prop.ForAll(
                Arb.From(EmailGenerator()),
                Arb.From(EmailGenerator()),
                (email1, email2) =>
                {
                    // Arrange
                    var cvText = $"Primary: {email1}, Secondary: {email2}";
                    
                    // Act
                    var result = _extractor.Extract(cvText);
                    
                    // Assert
                    return result.Email != null && result.Email.Contains("@");
                });
        }
        
        [Property(MaxTest = 100)]
        public Property EmailExtraction_WithNoEmail_ReturnsNull()
        {
            // Feature: resume-analyzer, Property 8: Email extraction completeness
            return Prop.ForAll(
                Arb.From(Gen.Elements("No email here", "Just text", "Call me", "123-456-7890")),
                text =>
                {
                    // Act
                    var result = _extractor.Extract(text);
                    
                    // Assert
                    return result.Email == null;
                });
        }
        
        // ============================================================================
        // Property 9: Regex phone pattern matching
        // Feature: resume-analyzer, Property 9: Phone extraction completeness
        // Validates: Requirements 3.4
        // ============================================================================
        
        [Property(MaxTest = 100)]
        public Property PhoneExtraction_WithValidPhone_AlwaysExtractsPhone()
        {
            // Feature: resume-analyzer, Property 9: Phone extraction completeness
            return Prop.ForAll(
                Arb.From(PhoneGenerator()),
                phone =>
                {
                    // Arrange
                    var cvText = $"Call me at {phone} anytime.";
                    
                    // Act
                    var result = _extractor.Extract(cvText);
                    
                    // Assert
                    // Extract digits from both to compare
                    var phoneDigits = new string(phone.Where(char.IsDigit).ToArray());
                    var resultDigits = result.Phone != null ? new string(result.Phone.Where(char.IsDigit).ToArray()) : "";
                    
                    return result.Phone != null &&
                           resultDigits.Length >= 7 && // At least 7 digits
                           result.Method == "Regex";
                });
        }
        
        [Property(MaxTest = 100)]
        public Property PhoneExtraction_WithMultiplePhones_ExtractsAtLeastOne()
        {
            // Feature: resume-analyzer, Property 9: Phone extraction completeness
            return Prop.ForAll(
                Arb.From(PhoneGenerator()),
                Arb.From(PhoneGenerator()),
                (phone1, phone2) =>
                {
                    // Arrange
                    var cvText = $"Office: {phone1}, Mobile: {phone2}";
                    
                    // Act
                    var result = _extractor.Extract(cvText);
                    
                    // Assert
                    return result.Phone != null && result.Phone.Any(char.IsDigit);
                });
        }
        
        [Property(MaxTest = 100)]
        public Property PhoneExtraction_WithNoPhone_ReturnsNull()
        {
            // Feature: resume-analyzer, Property 9: Phone extraction completeness
            return Prop.ForAll(
                Arb.From(Gen.Elements("No phone here", "Just text", "Email: test@example.com", "ABC-DEF-GHIJ")),
                text =>
                {
                    // Act
                    var result = _extractor.Extract(text);
                    
                    // Assert
                    return result.Phone == null;
                });
        }
        
        // ============================================================================
        // Property 10: Name heuristic extraction
        // Feature: resume-analyzer, Property 10: Name extraction accuracy
        // Validates: Requirements 3.5
        // ============================================================================
        
        [Property(MaxTest = 100)]
        public Property NameExtraction_WithNameOnFirstLine_ExtractsName()
        {
            // Feature: resume-analyzer, Property 10: Name extraction accuracy
            return Prop.ForAll(
                Arb.From(CVWithNameGenerator()),
                cvText =>
                {
                    // Act
                    var result = _extractor.Extract(cvText);
                    
                    // Assert
                    return result.Name != null &&
                           result.Name.Length > 0 &&
                           result.Name.Length < 50 &&
                           !result.Name.Contains("@") &&
                           result.Method == "Regex";
                });
        }
        
        [Property(MaxTest = 100)]
        public Property NameExtraction_WithEmailOnFirstLine_SkipsToNextLine()
        {
            // Feature: resume-analyzer, Property 10: Name extraction accuracy
            return Prop.ForAll(
                Arb.From(EmailGenerator()),
                Arb.From(NameGenerator()),
                (email, name) =>
                {
                    // Arrange - Email on first line, name on second
                    var cvText = $"{email}\n{name}\nSoftware Engineer";
                    
                    // Act
                    var result = _extractor.Extract(cvText);
                    
                    // Assert
                    // Should skip email line and extract name from second line
                    return result.Name != null && result.Name == name;
                });
        }
        
        [Property(MaxTest = 100)]
        public Property NameExtraction_WithLongFirstLine_ReturnsNull()
        {
            // Feature: resume-analyzer, Property 10: Name extraction accuracy
            return Prop.ForAll(
                Arb.From(Gen.Constant(new string('A', 60))), // Line longer than 50 chars
                longLine =>
                {
                    // Arrange
                    var cvText = $"{longLine}\nSome other text";
                    
                    // Act
                    var result = _extractor.Extract(cvText);
                    
                    // Assert
                    // Should not extract overly long first line as name
                    return result.Name == null || result.Name != longLine;
                });
        }
        
        [Property(MaxTest = 100)]
        public Property NameExtraction_WithNumberOnFirstLine_SkipsToNextLine()
        {
            // Feature: resume-analyzer, Property 10: Name extraction accuracy
            return Prop.ForAll(
                Arb.From(NameGenerator()),
                name =>
                {
                    // Arrange - Number on first line, name on second
                    var cvText = $"123 Main Street\n{name}\nSoftware Engineer";
                    
                    // Act
                    var result = _extractor.Extract(cvText);
                    
                    // Assert
                    // Should skip line starting with number
                    return result.Name != null && result.Name == name;
                });
        }
        
        // ============================================================================
        // Additional Property Tests for Comprehensive Coverage
        // ============================================================================
        
        [Property(MaxTest = 100)]
        public Property Extract_WithEmptyOrWhitespace_ReturnsEmptyEntities()
        {
            return Prop.ForAll(
                Arb.From(Gen.Elements("", "   ", "\n\n", "\t\t", "     \n    ")),
                emptyText =>
                {
                    // Act
                    var result = _extractor.Extract(emptyText);
                    
                    // Assert
                    return result.Name == null &&
                           result.Email == null &&
                           result.Phone == null &&
                           result.Skills.Count == 0 &&
                           result.TotalEntitiesFound == 0 &&
                           result.Method == "Regex";
                });
        }
        
        [Property(MaxTest = 100)]
        public Property Extract_WithAllEntities_CountsCorrectly()
        {
            return Prop.ForAll(
                Arb.From(NameGenerator()),
                Arb.From(EmailGenerator()),
                Arb.From(PhoneGenerator()),
                (name, email, phone) =>
                {
                    // Arrange
                    var cvText = $"{name}\nEmail: {email}\nPhone: {phone}\nSkills: C#, AWS, Python";
                    
                    // Act
                    var result = _extractor.Extract(cvText);
                    
                    // Assert
                    var expectedCount = 
                        (result.Name != null ? 1 : 0) +
                        (result.Email != null ? 1 : 0) +
                        (result.Phone != null ? 1 : 0) +
                        result.Skills.Count;
                    
                    return result.TotalEntitiesFound == expectedCount &&
                           result.TotalEntitiesFound >= 3; // At least name, email, phone
                });
        }
        
        [Property(MaxTest = 100)]
        public Property Extract_AlwaysSetsMethodToRegex()
        {
            return Prop.ForAll(
                Arb.Default.String(),
                text =>
                {
                    // Act
                    var result = _extractor.Extract(text ?? "");
                    
                    // Assert
                    return result.Method == "Regex";
                });
        }
    }

// ============================================================================
// Additional Edge Case Unit Tests
// ============================================================================

public class RegexEntityExtractorEdgeCaseTests
{
        private readonly RegexEntityExtractor _extractor;
        
        public RegexEntityExtractorEdgeCaseTests()
        {
            _extractor = new RegexEntityExtractor();
        }
        
        [Fact]
        public void Extract_GmailAddress_ExtractsCorrectly()
        {
            var cvText = "Contact: john.doe@gmail.com";
            var result = _extractor.Extract(cvText);
            Assert.Equal("john.doe@gmail.com", result.Email);
        }
        
        [Fact]
        public void Extract_CorporateEmailWithSubdomain_ExtractsCorrectly()
        {
            var cvText = "Work email: jane.smith@mail.company.com";
            var result = _extractor.Extract(cvText);
            Assert.Equal("jane.smith@mail.company.com", result.Email);
        }
        
        [Fact]
        public void Extract_EmailWithPlusSign_ExtractsCorrectly()
        {
            var cvText = "Email: user+tag@example.com";
            var result = _extractor.Extract(cvText);
            Assert.Equal("user+tag@example.com", result.Email);
        }
        
        [Fact]
        public void Extract_EmailWithUnderscore_ExtractsCorrectly()
        {
            var cvText = "Contact: first_last@domain.org";
            var result = _extractor.Extract(cvText);
            Assert.Equal("first_last@domain.org", result.Email);
        }
        
        [Fact]
        public void Extract_USPhoneWithDashes_ExtractsCorrectly()
        {
            var cvText = "Phone: 555-123-4567";
            var result = _extractor.Extract(cvText);
            Assert.Equal("555-123-4567", result.Phone);
        }
        
        [Fact]
        public void Extract_InternationalPhoneWithCountryCode_ExtractsCorrectly()
        {
            var cvText = "Mobile: +1-555-987-6543";
            var result = _extractor.Extract(cvText);
            Assert.Equal("+1-555-987-6543", result.Phone);
        }
        
        [Fact]
        public void Extract_PhoneWithParentheses_ExtractsCorrectly()
        {
            var cvText = "Call: (555) 123-4567";
            var result = _extractor.Extract(cvText);
            Assert.Equal("(555) 123-4567", result.Phone);
        }
        
        [Fact]
        public void Extract_PhoneWithDots_ExtractsCorrectly()
        {
            var cvText = "Tel: 555.123.4567";
            var result = _extractor.Extract(cvText);
            Assert.Equal("555.123.4567", result.Phone);
        }
        
        [Fact]
        public void Extract_PhoneWithSpaces_ExtractsCorrectly()
        {
            var cvText = "Contact: +1 555 123 4567";
            var result = _extractor.Extract(cvText);
            Assert.NotNull(result.Phone);
            Assert.Contains("555", result.Phone);
        }
        
        [Fact]
        public void Extract_PhoneWithoutSeparators_ExtractsCorrectly()
        {
            var cvText = "Phone: 5551234567";
            var result = _extractor.Extract(cvText);
            Assert.Equal("5551234567", result.Phone);
        }
        
        [Fact]
        public void Extract_MalformedEmail_ReturnsNull()
        {
            var cvText = "Email: notanemail@";
            var result = _extractor.Extract(cvText);
            Assert.Null(result.Email);
        }
        
        [Fact]
        public void Extract_MalformedPhone_ReturnsNull()
        {
            var cvText = "Phone: 123-45"; // Too short
            var result = _extractor.Extract(cvText);
            Assert.Null(result.Phone);
        }
        
        [Fact]
        public void Extract_NameWithSpecialCharacters_SkipsLine()
        {
            var cvText = "http://website.com\nJohn Doe\nEngineer";
            var result = _extractor.Extract(cvText);
            Assert.Equal("John Doe", result.Name);
        }
        
        [Fact]
        public void Extract_MultipleSkills_ExtractsAll()
        {
            var cvText = "Skills: C#, .NET, AWS, Docker, Kubernetes, Python, SQL, React";
            var result = _extractor.Extract(cvText);
            Assert.Contains("C#", result.Skills);
            Assert.Contains(".NET", result.Skills);
            Assert.Contains("AWS", result.Skills);
            Assert.Contains("Docker", result.Skills);
            Assert.Contains("Kubernetes", result.Skills);
            Assert.Contains("Python", result.Skills);
            Assert.Contains("SQL", result.Skills);
            Assert.Contains("React", result.Skills);
        }
        
        [Fact]
        public void Extract_SkillsCaseInsensitive_ExtractsCorrectly()
        {
            var cvText = "I know python, aws, and docker";
            var result = _extractor.Extract(cvText);
            Assert.Contains("Python", result.Skills);
            Assert.Contains("AWS", result.Skills);
            Assert.Contains("Docker", result.Skills);
        }
        
        [Fact]
        public void Extract_NoSkills_ReturnsEmptyList()
        {
            var cvText = "I have experience in various technologies";
            var result = _extractor.Extract(cvText);
            // Should not match "various" or "technologies" as they're not in the skill list
            Assert.Empty(result.Skills);
        }
        
        [Fact]
        public void Extract_NullInput_ReturnsEmptyEntities()
        {
            var result = _extractor.Extract(null!);
            Assert.Null(result.Name);
            Assert.Null(result.Email);
            Assert.Null(result.Phone);
            Assert.Empty(result.Skills);
            Assert.Equal(0, result.TotalEntitiesFound);
        }
        
        [Fact]
        public void Extract_WhitespaceOnly_ReturnsEmptyEntities()
        {
            var result = _extractor.Extract("     \n\t\n     ");
            Assert.Null(result.Name);
            Assert.Null(result.Email);
            Assert.Null(result.Phone);
            Assert.Empty(result.Skills);
            Assert.Equal(0, result.TotalEntitiesFound);
        }
    }
