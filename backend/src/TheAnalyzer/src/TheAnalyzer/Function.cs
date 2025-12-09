using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;
using TheAnalyzer.Interfaces;
using TheAnalyzer.Models;
using TheAnalyzer.Exceptions;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace TheAnalyzer;

/// <summary>
/// Main Lambda function handler using new service-based architecture
/// Refactored to use IAnalyzeService and IProfileRepository for better testability and maintainability
/// </summary>
public class Function
{
    private readonly IAnalyzeService _analyzeService;
    private readonly IProfileRepository _profileRepository;
    private readonly ILambdaLogger? _logger;

    /// <summary>
    /// Default constructor for AWS Lambda runtime
    /// Initializes services using dependency injection
    /// </summary>
    public Function()
    {
        var serviceProvider = new ServiceCollection()
            .ConfigureServices()
            .BuildServiceProvider();
        
        _analyzeService = serviceProvider.GetRequiredService<IAnalyzeService>();
        _profileRepository = serviceProvider.GetRequiredService<IProfileRepository>();
    }

    /// <summary>
    /// Constructor with dependency injection for testing
    /// </summary>
    public Function(IAnalyzeService analyzeService, IProfileRepository profileRepository, ILambdaLogger? logger = null)
    {
        _analyzeService = analyzeService ?? throw new ArgumentNullException(nameof(analyzeService));
        _profileRepository = profileRepository ?? throw new ArgumentNullException(nameof(profileRepository));
        _logger = logger;
    }

    /// <summary>
    /// Main Lambda function handler for CV analysis
    /// Processes CV text, extracts entities, and stores results in DynamoDB
    /// </summary>
    /// <param name="input">Analysis request containing CV text and optional resume ID</param>
    /// <param name="context">Lambda execution context</param>
    /// <returns>Analysis result with resume ID and status message</returns>
    public async Task<AnalyzerResult> FunctionHandler(AnalyzerInput input, ILambdaContext context)
    {
        var logger = _logger ?? context?.Logger;
        
        try
        {
            logger?.LogInformation($"[Function] Starting CV analysis for: {input?.CandidateName ?? "Unknown"}");
            
            // 1. Validate input
            if (input == null)
            {
                throw new ValidationException("Request cannot be null");
            }
            
            if (!_analyzeService.ValidateInput(input.ResumeText))
            {
                throw new ValidationException("Invalid CV text: text cannot be empty or whitespace only");
            }
            
            // 2. Extract entities using service layer
            logger?.LogInformation("[Function] Extracting entities from CV text");
            var entities = await _analyzeService.ExtractEntitiesAsync(input.ResumeText);
            
            // 3. Create profile record
            var resumeId = !string.IsNullOrWhiteSpace(input.ResumeId) 
                ? input.ResumeId 
                : Guid.NewGuid().ToString();
            
            var profile = new ProfileRecord
            {
                ResumeId = resumeId,
                Name = entities.Name ?? input.CandidateName,
                Email = entities.Email,
                Phone = entities.Phone,
                Skills = entities.Skills ?? new List<string>(),
                CreatedAt = DateTime.UtcNow
            };
            
            // Initialize DynamoDB keys
            profile.InitializeKeys();
            
            // 4. Save profile to DynamoDB
            logger?.LogInformation($"[Function] Saving profile record for ResumeId: {resumeId}");
            await _profileRepository.SaveProfileAsync(profile);
            
            // 5. Save analysis metadata
            var analysis = new AnalysisRecord
            {
                ResumeId = resumeId,
                AnalysisTimestamp = DateTime.UtcNow,
                ExtractionMethod = entities.Method,
                EntityCount = entities.TotalEntitiesFound,
                CreatedAt = DateTime.UtcNow
            };
            
            // Initialize DynamoDB keys
            analysis.InitializeKeys();
            
            logger?.LogInformation($"[Function] Saving analysis record for ResumeId: {resumeId}");
            await _profileRepository.SaveAnalysisAsync(analysis);
            
            logger?.LogInformation($"[Function] Analysis completed successfully for ResumeId: {resumeId}");
            
            // 6. Return success response (maintaining backward compatibility with AnalyzerResult)
            return new AnalyzerResult
            {
                Message = "Analysis completed successfully",
                AnalysisId = resumeId,
                DetectedEntities = new List<string>
                {
                    $"Name: {entities.Name}",
                    $"Email: {entities.Email}",
                    $"Phone: {entities.Phone}",
                    $"Skills: {string.Join(", ", entities.Skills ?? new List<string>())}",
                    $"Method: {entities.Method}",
                    $"Total Entities: {entities.TotalEntitiesFound}"
                }
            };
        }
        catch (ValidationException ex)
        {
            logger?.LogError($"[Function] Validation error: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            logger?.LogError($"[Function] Unexpected error: {ex.Message}");
            logger?.LogError($"[Function] Stack trace: {ex.StackTrace}");
            throw new Exception($"Analysis failed: {ex.Message}", ex);
        }
    }
}

// 1. Input model (maintaining backward compatibility)
public class AnalyzerInput
{
    public string? CandidateName { get; set; }
    public string ResumeText { get; set; } = string.Empty;
    public string? ResumeId { get; set; }
}

// 2. Output model (maintaining backward compatibility)
public class AnalyzerResult
{
    public string Message { get; set; } = string.Empty;
    public string AnalysisId { get; set; } = string.Empty;
    public List<string> DetectedEntities { get; set; } = new();
}