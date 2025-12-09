using FsCheck;
using FsCheck.Xunit;
using Moq;
using TheAnalyzer.Handlers;
using TheAnalyzer.Interfaces;
using TheAnalyzer.Models;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using System.Text.Json;
using Xunit;

namespace TheAnalyzer.Tests;

/// <summary>
/// Property-based tests for GetResumeLambdaFunction
/// </summary>
public class GetResumeLambdaFunctionPropertyTests
{
    /// <summary>
    /// Feature: resume-analyzer, Property 15: Retrieval by identifier
    /// Validates: Requirements 6.1
    /// 
    /// For any resume identifier that exists in the repository, 
    /// a GET request should successfully retrieve the corresponding profile record.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property RetrievalByIdentifier_ExistingProfile_ReturnsProfile()
    {
        return Prop.ForAll(
            ProfileRecordGenerator(),
            profile =>
            {
                // Arrange
                var mockRepository = new Mock<IProfileRepository>();
                mockRepository.Setup(r => r.GetProfileAsync(profile.ResumeId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(profile);
                
                var request = new APIGatewayProxyRequest
                {
                    PathParameters = new Dictionary<string, string>
                    {
                        { "id", profile.ResumeId }
                    }
                };
                
                var mockContext = new Mock<ILambdaContext>();
                var mockLogger = new Mock<ILambdaLogger>();
                mockContext.Setup(c => c.Logger).Returns(mockLogger.Object);
                
                var function = new GetResumeLambdaFunction(mockRepository.Object);
                
                // Act
                var response = function.FunctionHandler(request, mockContext.Object).Result;
                
                // Assert
                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                var retrievedProfile = response.StatusCode == 200 
                    ? JsonSerializer.Deserialize<ProfileRecord>(response.Body, options) 
                    : null;
                
                return retrievedProfile != null && 
                       retrievedProfile.ResumeId == profile.ResumeId;
            });
    }

    /// <summary>
    /// Feature: resume-analyzer, Property 16: Successful retrieval response
    /// Validates: Requirements 6.2
    /// 
    /// For any existing profile record, the GET API should return 
    /// HTTP status 200 with the complete profile data in JSON format.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property SuccessfulRetrievalResponse_ExistingProfile_Returns200WithData()
    {
        return Prop.ForAll(
            ProfileRecordGenerator(),
            profile =>
            {
                // Arrange
                var mockRepository = new Mock<IProfileRepository>();
                mockRepository.Setup(r => r.GetProfileAsync(profile.ResumeId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(profile);
                
                var request = new APIGatewayProxyRequest
                {
                    PathParameters = new Dictionary<string, string>
                    {
                        { "id", profile.ResumeId }
                    }
                };
                
                var mockContext = new Mock<ILambdaContext>();
                var mockLogger = new Mock<ILambdaLogger>();
                mockContext.Setup(c => c.Logger).Returns(mockLogger.Object);
                
                var function = new GetResumeLambdaFunction(mockRepository.Object);
                
                // Act
                var response = function.FunctionHandler(request, mockContext.Object).Result;
                
                // Assert
                var hasCorrectStatus = response.StatusCode == 200;
                var hasBody = !string.IsNullOrEmpty(response.Body);
                var hasContentType = response.Headers.ContainsKey("Content-Type") && 
                                    response.Headers["Content-Type"] == "application/json";
                
                // Verify body can be deserialized
                ProfileRecord? deserializedProfile = null;
                try
                {
                    var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                    deserializedProfile = JsonSerializer.Deserialize<ProfileRecord>(response.Body, options);
                }
                catch
                {
                    // Deserialization failed
                }
                
                return hasCorrectStatus && 
                       hasBody && 
                       hasContentType && 
                       deserializedProfile != null;
            });
    }

    /// <summary>
    /// Feature: resume-analyzer, Property 17: Not found handling
    /// Validates: Requirements 6.3
    /// 
    /// For any resume identifier that does not exist in the repository, 
    /// the GET API should return HTTP status 404 with an error message.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property NotFoundHandling_NonExistentProfile_Returns404()
    {
        return Prop.ForAll(
            ResumeIdGenerator(),
            resumeId =>
            {
                // Arrange
                var mockRepository = new Mock<IProfileRepository>();
                mockRepository.Setup(r => r.GetProfileAsync(resumeId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync((ProfileRecord?)null);
                
                var request = new APIGatewayProxyRequest
                {
                    PathParameters = new Dictionary<string, string>
                    {
                        { "id", resumeId }
                    }
                };
                
                var mockContext = new Mock<ILambdaContext>();
                var mockLogger = new Mock<ILambdaLogger>();
                mockContext.Setup(c => c.Logger).Returns(mockLogger.Object);
                
                var function = new GetResumeLambdaFunction(mockRepository.Object);
                
                // Act
                var response = function.FunctionHandler(request, mockContext.Object).Result;
                
                // Assert
                var hasCorrectStatus = response.StatusCode == 404;
                var hasBody = !string.IsNullOrEmpty(response.Body);
                
                // Verify error message is present in new format
                JsonElement errorResponse = default;
                bool hasErrorMessage = false;
                try
                {
                    errorResponse = JsonSerializer.Deserialize<JsonElement>(response.Body);
                    hasErrorMessage = errorResponse.TryGetProperty("message", out var message) &&
                                     !string.IsNullOrEmpty(message.GetString()) &&
                                     errorResponse.TryGetProperty("errorType", out var errorType) &&
                                     !string.IsNullOrEmpty(errorType.GetString());
                }
                catch
                {
                    // Deserialization failed
                }
                
                return hasCorrectStatus && hasBody && hasErrorMessage;
            });
    }

    /// <summary>
    /// Feature: resume-analyzer, Property 18: Response completeness
    /// Validates: Requirements 6.4
    /// 
    /// For any profile retrieved via the GET API, the response should include 
    /// all stored entities (name, email, phone, skills) without omission.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property ResponseCompleteness_RetrievedProfile_ContainsAllEntities()
    {
        return Prop.ForAll(
            ProfileRecordGenerator(),
            profile =>
            {
                // Arrange
                var mockRepository = new Mock<IProfileRepository>();
                mockRepository.Setup(r => r.GetProfileAsync(profile.ResumeId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(profile);
                
                var request = new APIGatewayProxyRequest
                {
                    PathParameters = new Dictionary<string, string>
                    {
                        { "id", profile.ResumeId }
                    }
                };
                
                var mockContext = new Mock<ILambdaContext>();
                var mockLogger = new Mock<ILambdaLogger>();
                mockContext.Setup(c => c.Logger).Returns(mockLogger.Object);
                
                var function = new GetResumeLambdaFunction(mockRepository.Object);
                
                // Act
                var response = function.FunctionHandler(request, mockContext.Object).Result;
                
                // Assert
                if (response.StatusCode != 200)
                {
                    return false;
                }
                
                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                var retrievedProfile = JsonSerializer.Deserialize<ProfileRecord>(response.Body, options);
                if (retrievedProfile == null)
                {
                    return false;
                }
                
                // Verify all fields are present and match
                var resumeIdMatches = retrievedProfile.ResumeId == profile.ResumeId;
                var nameMatches = retrievedProfile.Name == profile.Name;
                var emailMatches = retrievedProfile.Email == profile.Email;
                var phoneMatches = retrievedProfile.Phone == profile.Phone;
                
                // Skills list should be present and contain same items
                var skillsPresent = retrievedProfile.Skills != null;
                var skillsMatch = skillsPresent && 
                                 retrievedProfile.Skills.Count == profile.Skills.Count &&
                                 retrievedProfile.Skills.All(s => profile.Skills.Contains(s));
                
                return resumeIdMatches && 
                       nameMatches && 
                       emailMatches && 
                       phoneMatches && 
                       skillsPresent && 
                       skillsMatch;
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
            Guid.NewGuid().ToString(),
            "test-" + Guid.NewGuid().ToString().Substring(0, 8),
            "resume-" + Guid.NewGuid().ToString().Substring(0, 8)
        );

        var nameGen = Gen.Elements(
            "John Doe", 
            "Jane Smith", 
            "Bob Johnson", 
            "Alice Williams",
            "Charlie Brown",
            "Diana Prince",
            "Eve Anderson",
            null
        );

        var emailGen = Gen.OneOf(
            from username in Gen.Elements("john", "jane", "test", "user", "admin", "contact")
            from domain in Gen.Elements("gmail.com", "yahoo.com", "company.com", "example.org")
            select $"{username}@{domain}",
            Gen.Constant<string?>(null)
        );

        var phoneGen = Gen.OneOf(
            from areaCode in Gen.Choose(200, 999)
            from prefix in Gen.Choose(200, 999)
            from lineNumber in Gen.Choose(1000, 9999)
            select $"+1-{areaCode}-{prefix}-{lineNumber}",
            from countryCode in Gen.Elements("44", "33", "49", "81")
            from number in Gen.Choose(10000000, 99999999)
            select $"+{countryCode}-{number}",
            Gen.Constant<string?>(null)
        );

        var skillsGen = Gen.ListOf(
            Gen.Elements("C#", ".NET", "AWS", "Python", "Java", "JavaScript", 
                        "SQL", "Docker", "Kubernetes", "React", "Angular", "TypeScript")
        ).Select(skills => skills.Distinct().ToList());

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
                            CreatedAt = DateTime.UtcNow.AddDays(-Gen.Choose(0, 30).Sample(0, 1).First())
                        };

        return Arb.From(profileGen);
    }

    /// <summary>
    /// Generator for resume IDs
    /// </summary>
    private static Arbitrary<string> ResumeIdGenerator()
    {
        var idGen = Gen.OneOf(
            Gen.Constant(Guid.NewGuid().ToString()),
            Gen.Constant("test-" + Guid.NewGuid().ToString().Substring(0, 8)),
            Gen.Constant("resume-" + Guid.NewGuid().ToString().Substring(0, 8)),
            from prefix in Gen.Elements("cv", "profile", "candidate")
            from suffix in Gen.Choose(1000, 9999)
            select $"{prefix}-{suffix}"
        );

        return Arb.From(idGen);
    }
}
