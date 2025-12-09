using FsCheck;
using FsCheck.Xunit;
using TheAnalyzer.Models;
using Xunit;

namespace TheAnalyzer.Tests;

/// <summary>
/// Property-based tests for ProfileRepository data models
/// Note: These tests verify data model correctness and key formats
/// Full repository integration tests require DynamoDB Local or AWS environment
/// </summary>
public class ProfileRepositoryPropertyTests
{
    /// <summary>
    /// Feature: resume-analyzer, Property 11: Complete entity storage
    /// Validates: Requirements 4.2
    /// 
    /// For any set of extracted entities (name, email, phone, skills), 
    /// when creating a ProfileRecord, all fields should be properly initialized
    /// and the key format should be correct for DynamoDB storage.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property CompleteEntityStorage_ProfileRecord_ContainsAllFields()
    {
        return Prop.ForAll(
            ProfileRecordGenerator(),
            profile =>
            {
                // Verify all fields are properly set
                var hasResumeId = !string.IsNullOrEmpty(profile.ResumeId);
                var hasValidPK = profile.PK == $"RESUME#{profile.ResumeId}";
                var hasValidSK = profile.SK == "PROFILE";
                var hasValidEntityType = profile.EntityType == "Profile";
                var hasCreatedAt = profile.CreatedAt != default;
                var hasSkillsList = profile.Skills != null;
                
                // All required fields should be present and valid
                return hasResumeId &&
                       hasValidPK &&
                       hasValidSK &&
                       hasValidEntityType &&
                       hasCreatedAt &&
                       hasSkillsList;
            });
    }

    /// <summary>
    /// Feature: resume-analyzer, Property 12: Analysis metadata completeness
    /// Validates: Requirements 4.3
    /// 
    /// For any analysis operation, the analysis record should contain 
    /// a timestamp, extraction method ("Comprehend" or "Regex"), entity count,
    /// and proper key format for DynamoDB storage.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property AnalysisMetadataCompleteness_AnalysisRecord_ContainsAllMetadata()
    {
        return Prop.ForAll(
            AnalysisRecordGenerator(),
            analysis =>
            {
                // Verify all required metadata is present
                var hasResumeId = !string.IsNullOrEmpty(analysis.ResumeId);
                var hasValidPK = analysis.PK == $"RESUME#{analysis.ResumeId}";
                var hasValidSK = analysis.SK.StartsWith("ANALYSIS#");
                var hasValidEntityType = analysis.EntityType == "Analysis";
                var hasTimestamp = analysis.AnalysisTimestamp != default;
                var hasValidMethod = analysis.ExtractionMethod == "Comprehend" || 
                                    analysis.ExtractionMethod == "Regex";
                var hasValidCount = analysis.EntityCount >= 0;
                var hasCreatedAt = analysis.CreatedAt != default;
                
                return hasResumeId &&
                       hasValidPK &&
                       hasValidSK &&
                       hasValidEntityType &&
                       hasTimestamp &&
                       hasValidMethod &&
                       hasValidCount &&
                       hasCreatedAt;
            });
    }

    /// <summary>
    /// Generator for ProfileRecord with random but valid data
    /// </summary>
    private static Arbitrary<ProfileRecord> ProfileRecordGenerator()
    {
        var resumeIdGen = Gen.Elements(
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString()
        );

        var nameGen = Gen.Elements(
            "John Doe", 
            "Jane Smith", 
            "Bob Johnson", 
            "Alice Williams",
            "Charlie Brown",
            null
        );

        var emailGen = Gen.OneOf(
            from username in Gen.Elements("john", "jane", "test", "user", "admin")
            from domain in Gen.Elements("gmail.com", "yahoo.com", "company.com")
            select $"{username}@{domain}",
            Gen.Constant<string?>(null)
        );

        var phoneGen = Gen.OneOf(
            from areaCode in Gen.Choose(200, 999)
            from prefix in Gen.Choose(200, 999)
            from lineNumber in Gen.Choose(1000, 9999)
            select $"+1-{areaCode}-{prefix}-{lineNumber}",
            Gen.Constant<string?>(null)
        );

        var skillsGen = Gen.ListOf(
            Gen.Elements("C#", ".NET", "AWS", "Python", "Java", "JavaScript", "SQL", "Docker")
        ).Select(skills => skills.ToList());

        var profileGen = from resumeId in resumeIdGen
                        from name in nameGen
                        from email in emailGen
                        from phone in phoneGen
                        from skills in skillsGen
                        select new ProfileRecord
                        {
                            ResumeId = resumeId,
                            Name = name,
                            Email = email,
                            Phone = phone,
                            Skills = skills,
                            CreatedAt = DateTime.UtcNow
                        };

        return Arb.From(profileGen);
    }

    /// <summary>
    /// Generator for AnalysisRecord with random but valid data
    /// </summary>
    private static Arbitrary<AnalysisRecord> AnalysisRecordGenerator()
    {
        var resumeIdGen = Gen.Elements(
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString()
        );

        var methodGen = Gen.Elements("Comprehend", "Regex");
        var entityCountGen = Gen.Choose(0, 20);

        var analysisGen = from resumeId in resumeIdGen
                         from method in methodGen
                         from count in entityCountGen
                         select new AnalysisRecord
                         {
                             ResumeId = resumeId,
                             AnalysisTimestamp = DateTime.UtcNow,
                             ExtractionMethod = method,
                             EntityCount = count,
                             CreatedAt = DateTime.UtcNow
                         };

        return Arb.From(analysisGen);
    }
}
