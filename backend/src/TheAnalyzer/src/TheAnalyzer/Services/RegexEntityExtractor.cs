using System.Text.RegularExpressions;
using TheAnalyzer.Models;

namespace TheAnalyzer.Services;

/// <summary>
/// Fallback entity extractor using regex patterns when AWS Comprehend is unavailable
/// </summary>
public class RegexEntityExtractor
{
    // Email regex pattern matching standard email format
    private static readonly Regex EmailPattern = new Regex(
        @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );
    
    // Phone regex pattern matching various formats with/without country codes
    // Matches formats like: +1-555-123-4567, (555) 123-4567, 555.123.4567, 5551234567, etc.
    private static readonly Regex PhonePattern = new Regex(
        @"(\+?\d{1,3}[-.\s]?)?\(?\d{3}\)?[-.\s]?\d{3}[-.\s]?\d{4}",
        RegexOptions.Compiled
    );
    
    // Predefined list of common technical skills
    private static readonly string[] SkillKeywords = new[]
    {
        "C#", "C++", "Java", "Python", "JavaScript", "TypeScript", "Go", "Rust", "Ruby", "PHP",
        ".NET", "ASP.NET", "Node.js", "React", "Angular", "Vue", "Django", "Flask", "Spring",
        "AWS", "Azure", "GCP", "Docker", "Kubernetes", "Jenkins", "Git", "CI/CD",
        "SQL", "MySQL", "PostgreSQL", "MongoDB", "DynamoDB", "Redis", "Elasticsearch",
        "REST", "GraphQL", "gRPC", "Microservices", "API", "Agile", "Scrum",
        "Machine Learning", "AI", "Data Science", "TensorFlow", "PyTorch",
        "HTML", "CSS", "SASS", "Bootstrap", "Tailwind",
        "Linux", "Unix", "Windows", "MacOS",
        "Terraform", "Ansible", "CloudFormation", "Serverless", "Lambda"
    };
    
    /// <summary>
    /// Extracts all entities from CV text using regex patterns
    /// </summary>
    /// <param name="cvText">The CV text to analyze</param>
    /// <returns>Extracted entities with method set to "Regex"</returns>
    public ExtractedEntities Extract(string cvText)
    {
        if (string.IsNullOrWhiteSpace(cvText))
        {
            return new ExtractedEntities
            {
                Method = "Regex",
                TotalEntitiesFound = 0
            };
        }
        
        var entities = new ExtractedEntities
        {
            Name = ExtractName(cvText),
            Email = ExtractEmail(cvText),
            Phone = ExtractPhone(cvText),
            Skills = ExtractSkills(cvText),
            Method = "Regex"
        };
        
        // Calculate total entities found
        entities.TotalEntitiesFound = 
            (string.IsNullOrEmpty(entities.Name) ? 0 : 1) +
            (string.IsNullOrEmpty(entities.Email) ? 0 : 1) +
            (string.IsNullOrEmpty(entities.Phone) ? 0 : 1) +
            entities.Skills.Count;
        
        return entities;
    }
    
    /// <summary>
    /// Extracts email address from CV text using regex pattern
    /// </summary>
    /// <param name="text">The text to search</param>
    /// <returns>First email address found, or null if none found</returns>
    private string? ExtractEmail(string text)
    {
        var match = EmailPattern.Match(text);
        return match.Success ? match.Value : null;
    }
    
    /// <summary>
    /// Extracts phone number from CV text using regex pattern
    /// </summary>
    /// <param name="text">The text to search</param>
    /// <returns>First phone number found, or null if none found</returns>
    private string? ExtractPhone(string text)
    {
        var match = PhonePattern.Match(text);
        return match.Success ? match.Value : null;
    }
    
    /// <summary>
    /// Extracts candidate name using heuristic: first non-empty line if it looks like a name
    /// </summary>
    /// <param name="text">The CV text to analyze</param>
    /// <returns>Extracted name, or null if not found</returns>
    private string? ExtractName(string text)
    {
        // Split by newlines and find first non-empty line
        var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            
            // Skip empty lines
            if (string.IsNullOrWhiteSpace(trimmedLine))
            {
                continue;
            }
            
            // Heuristic: First line should be short (< 50 chars) and look like a name
            // Names typically don't contain special characters like @, numbers at the start, etc.
            if (trimmedLine.Length < 50 && 
                !trimmedLine.Contains('@') && 
                !trimmedLine.Contains("http") &&
                !char.IsDigit(trimmedLine[0]))
            {
                return trimmedLine;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Extracts skills by matching against predefined keyword list
    /// </summary>
    /// <param name="text">The CV text to analyze</param>
    /// <returns>List of skills found in the text</returns>
    private List<string> ExtractSkills(string text)
    {
        var foundSkills = new List<string>();
        
        foreach (var skill in SkillKeywords)
        {
            // Case-insensitive search for skill keywords
            if (text.Contains(skill, StringComparison.OrdinalIgnoreCase))
            {
                foundSkills.Add(skill);
            }
        }
        
        return foundSkills;
    }
}
