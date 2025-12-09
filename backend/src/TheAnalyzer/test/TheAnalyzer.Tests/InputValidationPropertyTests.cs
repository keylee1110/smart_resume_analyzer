using FsCheck;
using FsCheck.Xunit;
using TheAnalyzer.Services;
using Xunit;

namespace TheAnalyzer.Tests;

/// <summary>
/// Property-based tests for input validation
/// </summary>
public class InputValidationPropertyTests
{
    private readonly AnalyzeService _service;

    public InputValidationPropertyTests()
    {
        _service = new AnalyzeService();
    }

    /// <summary>
    /// Feature: resume-analyzer, Property 1: Empty input rejection
    /// Validates: Requirements 1.1, 1.2
    /// 
    /// For any input string that is null, empty, or contains only whitespace,
    /// the validation function should return false and the system should reject the input.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property ValidateInput_EmptyOrWhitespaceInput_ReturnsFalse()
    {
        // Generate null, empty, and whitespace-only strings
        return Prop.ForAll(
            InvalidCvTextGenerator(),
            invalidText =>
            {
                // Act
                var result = _service.ValidateInput(invalidText);
                
                // Assert - should always return false for invalid input
                return !result;
            });
    }

    /// <summary>
    /// Feature: resume-analyzer, Property 2: Valid input acceptance
    /// Validates: Requirements 1.3
    /// 
    /// For any non-empty input string containing meaningful text,
    /// the validation function should return true and the system should accept the input for processing.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property ValidateInput_NonEmptyText_ReturnsTrue()
    {
        // Generate non-empty text with meaningful content
        return Prop.ForAll(
            ValidCvTextGenerator(),
            validText =>
            {
                // Act
                var result = _service.ValidateInput(validText);
                
                // Assert - should always return true for valid input
                return result;
            });
    }

    /// <summary>
    /// Generator for invalid CV text (null, empty, or whitespace-only strings)
    /// </summary>
    private static Arbitrary<string> InvalidCvTextGenerator()
    {
        var nullGen = Gen.Constant<string>(null!);
        
        var emptyGen = Gen.Constant(string.Empty);
        
        var whitespaceGen = Gen.OneOf(
            Gen.Constant(" "),
            Gen.Constant("  "),
            Gen.Constant("   "),
            Gen.Constant("\t"),
            Gen.Constant("\n"),
            Gen.Constant("\r"),
            Gen.Constant("\r\n"),
            Gen.Constant("  \t  "),
            Gen.Constant("\n\n"),
            Gen.Constant("   \t\n   "),
            Gen.Constant("\t\t\t"),
            Gen.Constant("     \n\r\t     ")
        );
        
        var combinedGen = Gen.OneOf(nullGen, emptyGen, whitespaceGen);
        
        return Arb.From(combinedGen);
    }

    /// <summary>
    /// Generator for valid CV text (non-empty strings with meaningful content)
    /// </summary>
    private static Arbitrary<string> ValidCvTextGenerator()
    {
        // Generate various types of valid CV content
        var simpleTextGen = Gen.OneOf(
            Gen.Constant("John Doe"),
            Gen.Constant("Software Engineer"),
            Gen.Constant("Skills: C#, .NET, AWS"),
            Gen.Constant("Experience: 5 years"),
            Gen.Constant("john.doe@example.com"),
            Gen.Constant("+1-555-0123"),
            Gen.Constant("Education: BS Computer Science")
        );

        var multiLineTextGen = Gen.OneOf(
            Gen.Constant("John Doe\nSoftware Engineer\nSkills: C#, .NET"),
            Gen.Constant("Jane Smith\nEmail: jane@example.com\nPhone: 555-0123"),
            Gen.Constant("Experience:\n- Software Developer at ABC Corp\n- Senior Engineer at XYZ Inc"),
            Gen.Constant("Education:\nBS Computer Science\nMS Software Engineering"),
            Gen.Constant("Skills:\nC#, .NET, AWS, Python, SQL\nDocker, Kubernetes")
        );

        var textWithWhitespaceGen = from text in simpleTextGen
                                    from leadingSpaces in Gen.Choose(0, 3)
                                    from trailingSpaces in Gen.Choose(0, 3)
                                    select new string(' ', leadingSpaces) + text + new string(' ', trailingSpaces);

        var alphanumericGen = from length in Gen.Choose(1, 100)
                             from chars in Gen.ArrayOf(length, Gen.Elements(
                                 "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 .,\n".ToCharArray()))
                             let str = new string(chars)
                             where !string.IsNullOrWhiteSpace(str)
                             select str;

        var combinedGen = Gen.OneOf(simpleTextGen, multiLineTextGen, textWithWhitespaceGen, alphanumericGen);
        
        return Arb.From(combinedGen);
    }
}
