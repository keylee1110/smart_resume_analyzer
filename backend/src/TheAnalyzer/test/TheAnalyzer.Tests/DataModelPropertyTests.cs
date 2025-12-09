using System.Text.Json;
using FsCheck;
using FsCheck.Xunit;
using TheAnalyzer.Models;
using Xunit;

namespace TheAnalyzer.Tests;

/// <summary>
/// Property-based tests for data model serialization
/// </summary>
public class DataModelPropertyTests
{
    /// <summary>
    /// Feature: resume-analyzer, Property 11: Complete entity storage
    /// Validates: Requirements 4.2
    /// 
    /// For any set of extracted entities (name, email, phone, skills), 
    /// when serialized to JSON and deserialized back, all fields should be preserved.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property ExtractedEntities_SerializationRoundTrip_PreservesAllFields()
    {
        // Generate random ExtractedEntities and verify serialization/deserialization
        return Prop.ForAll(
            ExtractedEntitiesGenerator(),
            entities =>
            {
                // Serialize to JSON
                var json = JsonSerializer.Serialize(entities);
                
                // Deserialize back
                var deserialized = JsonSerializer.Deserialize<ExtractedEntities>(json);
                
                // Verify all fields are preserved
                return deserialized != null &&
                       deserialized.Name == entities.Name &&
                       deserialized.Email == entities.Email &&
                       deserialized.Phone == entities.Phone &&
                       deserialized.Method == entities.Method &&
                       deserialized.TotalEntitiesFound == entities.TotalEntitiesFound &&
                       deserialized.Skills.SequenceEqual(entities.Skills);
            });
    }

    /// <summary>
    /// Generator for ExtractedEntities with random but valid data
    /// </summary>
    private static Arbitrary<ExtractedEntities> ExtractedEntitiesGenerator()
    {
        var nameGen = Gen.Elements(
            "John Doe", 
            "Jane Smith", 
            "Bob Johnson", 
            "Alice Williams",
            "Charlie Brown",
            null // Test null names
        );

        var emailGen = Gen.OneOf(
            from username in Gen.Elements("john", "jane", "test", "user", "admin")
            from domain in Gen.Elements("gmail.com", "yahoo.com", "company.com", "example.org")
            select $"{username}@{domain}",
            Gen.Constant<string?>(null) // Test null emails
        );

        var phoneGen = Gen.OneOf(
            from areaCode in Gen.Choose(200, 999)
            from prefix in Gen.Choose(200, 999)
            from lineNumber in Gen.Choose(1000, 9999)
            select $"+1-{areaCode}-{prefix}-{lineNumber}",
            Gen.Constant<string?>(null) // Test null phones
        );

        var skillsGen = Gen.ListOf(
            Gen.Elements("C#", ".NET", "AWS", "Python", "Java", "JavaScript", "SQL", "Docker")
        ).Select(skills => skills.ToList());

        var methodGen = Gen.Elements("Comprehend", "Regex");

        var entityCountGen = Gen.Choose(0, 20);

        var entitiesGen = from name in nameGen
                         from email in emailGen
                         from phone in phoneGen
                         from skills in skillsGen
                         from method in methodGen
                         from count in entityCountGen
                         select new ExtractedEntities
                         {
                             Name = name,
                             Email = email,
                             Phone = phone,
                             Skills = skills,
                             Method = method,
                             TotalEntitiesFound = count
                         };

        return Arb.From(entitiesGen);
    }
}
