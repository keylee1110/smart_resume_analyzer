using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using TheAnalyzer.Interfaces;
using TheAnalyzer.Models;
using TheAnalyzer.Exceptions;
using System.IO;

namespace TheAnalyzer.Handlers;

/// <summary>
/// Lambda function handler for CV analysis
/// </summary>
public class AnalyzeLambdaFunction
{
    private readonly IAnalyzeService? _analyzeService;
    private readonly IProfileRepository? _profileRepository;
    private readonly ILambdaLogger? _logger;
    private static Exception? _initException;
    
    /// <summary>
    /// Default constructor for AWS Lambda runtime
    /// Initializes services using dependency injection
    /// </summary>
    public AnalyzeLambdaFunction()
    {
        try
        {
            var serviceProvider = new ServiceCollection()
                .ConfigureServices()
                .BuildServiceProvider();
            
            _analyzeService = serviceProvider.GetRequiredService<IAnalyzeService>();
            _profileRepository = serviceProvider.GetRequiredService<IProfileRepository>();
        }
        catch (Exception ex)
        {
            _initException = ex;
            Console.WriteLine($"[CRITICAL] Constructor Initialization Failed: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }
    
    /// <summary>
    /// Constructor with dependency injection for testing
    /// </summary>
    public AnalyzeLambdaFunction(IAnalyzeService analyzeService, IProfileRepository profileRepository, ILambdaLogger? logger = null)
    {
        _analyzeService = analyzeService ?? throw new ArgumentNullException(nameof(analyzeService));
        _profileRepository = profileRepository ?? throw new ArgumentNullException(nameof(profileRepository));
        _logger = logger;
    }
    
    /// <summary>
    /// Main Lambda function handler for CV analysis
    /// </summary>
    /// <param name="inputStream">Input stream containing the request payload</param>
    /// <param name="context">Lambda execution context</param>
    /// <returns>Stream containing the response</returns>
    public async Task<Stream> FunctionHandler(Stream inputStream, ILambdaContext context)
    {
        var logger = _logger ?? context?.Logger;

        if (_initException != null)
        {
            logger?.LogLine($"[CRITICAL] Function initialization failed previously: {_initException.Message}");
            logger?.LogLine(_initException.StackTrace);
            
            var errorResponse = ApiResponseHelper.CreateErrorResponse(500, $"Internal Server Error (Init): {_initException.Message}", "InitError");
            var ms = new MemoryStream();
            var w = new StreamWriter(ms);
            await w.WriteAsync(JsonSerializer.Serialize(errorResponse));
            await w.FlushAsync();
            ms.Position = 0;
            return ms;
        }

        using var reader = new StreamReader(inputStream);
        var requestBody = await reader.ReadToEndAsync();
        
        // Heuristic: Check for "httpMethod" to identify API Gateway request
        if (requestBody.Contains("\"httpMethod\"") || requestBody.Contains("\"requestContext\""))
        {
            logger?.LogInformation("[AnalyzeLambdaFunction] Detected API Gateway Request");
            APIGatewayProxyRequest? proxyRequest = null;
            try
            {
                proxyRequest = JsonSerializer.Deserialize<APIGatewayProxyRequest>(requestBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (JsonException ex)
            {
                logger?.LogError($"Error deserializing API Gateway Request: {ex.Message}");
            }

            var response = await HandleApiGatewayRequest(proxyRequest, logger);
            
            var responseStream = new MemoryStream();
            var writer = new StreamWriter(responseStream);
            await writer.WriteAsync(JsonSerializer.Serialize(response));
            await writer.FlushAsync();
            responseStream.Position = 0;
            return responseStream;
        }
        else
        {
            logger?.LogInformation("[AnalyzeLambdaFunction] Detected Direct Invocation (Parser Payload)");
            AnalyzerInvocationPayload? payload = null;
            try
            {
                payload = JsonSerializer.Deserialize<AnalyzerInvocationPayload>(requestBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (JsonException ex)
            {
                logger?.LogError($"Error deserializing Direct Payload: {ex.Message}");
            }

            await HandleDirectInvocation(payload, logger);
            return new MemoryStream(); // Return empty stream
        }
    }

    private async Task<APIGatewayProxyResponse> HandleApiGatewayRequest(APIGatewayProxyRequest? proxyRequest, ILambdaLogger? logger)
    {
        try
        {
            logger?.LogInformation($"[HandleApiGatewayRequest] Processing analysis request");
            
            if (proxyRequest == null || string.IsNullOrEmpty(proxyRequest.Body))
            {
                logger?.LogWarning("[HandleApiGatewayRequest] Request body is empty");
                return ApiResponseHelper.CreateErrorResponse(400, "Request body cannot be empty", "ValidationError");
            }

            AnalyzeRequest? request;
            try
            {
                request = JsonSerializer.Deserialize<AnalyzeRequest>(proxyRequest.Body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (JsonException ex)
            {
                logger?.LogError($"[HandleApiGatewayRequest] JSON parsing error: {ex.Message}");
                return ApiResponseHelper.CreateErrorResponse(400, "Invalid JSON in request body", "ValidationError");
            }
            
            if (request == null)
            {
                logger?.LogWarning("[HandleApiGatewayRequest] Deserialized request is null");
                return ApiResponseHelper.CreateErrorResponse(400, "Invalid request body", "ValidationError");
            }

            string? userId = null;
            
            if (proxyRequest.RequestContext?.Authorizer?.Claims != null && 
                proxyRequest.RequestContext.Authorizer.Claims.TryGetValue("sub", out var cognitoUserId))
            {
                userId = cognitoUserId;
                logger?.LogInformation($"[HandleApiGatewayRequest] User authenticated via Cognito: {userId}");
            }
            else
            {
                logger?.LogWarning("[HandleApiGatewayRequest] Unauthorized: No user ID found in Cognito context");
                return ApiResponseHelper.CreateErrorResponse(401, "Unauthorized - No valid authentication", "AuthenticationError");
            }
            
            request.UserId = userId; 

            logger?.LogInformation($"[HandleApiGatewayRequest] Starting CV analysis for ResumeId: {request.ResumeId ?? "new"}");
            
            string resumeTextToAnalyze = request.ResumeText;
            ProfileRecord? existingProfile = null;
            
            if (string.IsNullOrWhiteSpace(resumeTextToAnalyze) && !string.IsNullOrWhiteSpace(request.ResumeId)) 
            {
                 bool isS3Key = request.ResumeId.Contains("private/", StringComparison.OrdinalIgnoreCase);
                 
                 if (isS3Key)
                 {
                     logger?.LogInformation($"[HandleApiGatewayRequest] ResumeId appears to be an S3 key, querying by S3 key: {request.ResumeId}");
                     existingProfile = await _profileRepository.GetProfileByS3KeyAsync(request.ResumeId);
                     
                     if (existingProfile == null)
                     {
                         logger?.LogWarning($"[HandleApiGatewayRequest] Profile not found for S3 key: {request.ResumeId}");
                         return ApiResponseHelper.CreateErrorResponse(404, 
                             "The resume is still being processed. Please wait a moment and try again.", 
                             "NotFoundError");
                     }
                     
                     logger?.LogInformation($"[HandleApiGatewayRequest] Found profile for S3 key, ResumeId: {existingProfile.ResumeId}");
                 }
                 else
                 {
                     existingProfile = await _profileRepository.GetProfileAsync(request.ResumeId);
                 }
                 
                 if (existingProfile != null && !string.IsNullOrWhiteSpace(existingProfile.ResumeText))
                 {
                     resumeTextToAnalyze = existingProfile.ResumeText;
                     logger?.LogInformation($"[HandleApiGatewayRequest] Retrieved ResumeText from storage for ResumeId: {existingProfile.ResumeId}");
                 }
            } 
            
            if (string.IsNullOrWhiteSpace(resumeTextToAnalyze))
            {
                logger?.LogWarning("[HandleApiGatewayRequest] Resume text not found");
                return ApiResponseHelper.CreateErrorResponse(400, 
                    "Resume text not found. Please upload a resume first.", 
                    "ValidationError");
            }
            
            logger?.LogInformation("[HandleApiGatewayRequest] Analyzing CV text and comparing with JD");
            var result = await _analyzeService.AnalyzeCvAsync(resumeTextToAnalyze, request.JobDescription);
            
            var resumeId = existingProfile?.ResumeId 
                ?? (!string.IsNullOrWhiteSpace(request.ResumeId) && !request.ResumeId.Contains("private/") 
                    ? request.ResumeId 
                    : Guid.NewGuid().ToString());
            
            var profile = new ProfileRecord
            {
                ResumeId = resumeId,
                Name = result.ResumeEntities.Name,
                Email = result.ResumeEntities.Email,
                Phone = result.ResumeEntities.Phone,
                Skills = result.ResumeEntities.Skills ?? new List<string>(),
                ResumeText = resumeTextToAnalyze,
                LastAnalysis = result,
                CreatedAt = existingProfile?.CreatedAt ?? DateTime.UtcNow,
                UserId = request.UserId,
                S3Key = (!string.IsNullOrWhiteSpace(request.ResumeId) && request.ResumeId.Contains("private/")) ? request.ResumeId : existingProfile?.S3Key,
                JobTitle = result.JobTitle,
                Company = result.Company,
                SessionName = !string.IsNullOrWhiteSpace(result.JobTitle) && !string.IsNullOrWhiteSpace(result.Company) 
                    ? $"{result.JobTitle} at {result.Company}" 
                    : (!string.IsNullOrWhiteSpace(result.JobTitle) ? result.JobTitle : $"Job Application - {DateTime.UtcNow:MMM dd, yyyy}")
            };
            
            profile.InitializeKeys();
            
            logger?.LogInformation($"[HandleApiGatewayRequest] Saving profile record for ResumeId: {resumeId}");
            await _profileRepository.SaveProfileAsync(profile);
            
            var analysis = new AnalysisRecord
            {
                ResumeId = resumeId,
                AnalysisTimestamp = DateTime.UtcNow,
                ExtractionMethod = result.ResumeEntities.Method,
                EntityCount = result.ResumeEntities.TotalEntitiesFound,
                CreatedAt = DateTime.UtcNow
            };
            
            analysis.InitializeKeys();
            
            logger?.LogInformation($"[HandleApiGatewayRequest] Saving analysis record for ResumeId: {resumeId}");
            await _profileRepository.SaveAnalysisAsync(analysis);
            
            logger?.LogInformation($"[HandleApiGatewayRequest] Analysis completed successfully for ResumeId: {resumeId}");
            
            var responseBody = new AnalyzeResponse
            {
                ResumeId = resumeId,
                Message = "Analysis completed successfully",
                Analysis = result
            };

            return ApiResponseHelper.CreateResponse(200, responseBody);
        }
        catch (ValidationException ex)
        {
            logger?.LogError($"[HandleApiGatewayRequest] Validation error: {ex.Message}");
            return ApiResponseHelper.CreateErrorResponse(400, ex.Message, "ValidationError");
        }
        catch (Exception ex)
        {
            logger?.LogError($"[HandleApiGatewayRequest] Unexpected error: {ex.Message}");
            logger?.LogError($"[HandleApiGatewayRequest] Stack trace: {ex.StackTrace}");
            return ApiResponseHelper.CreateErrorResponse(500, $"Internal Error: {ex.Message}", "InternalError");
        }
    }

    private async Task HandleDirectInvocation(AnalyzerInvocationPayload? payload, ILambdaLogger? logger)
    {
        if (payload == null)
        {
            logger?.LogWarning("[HandleDirectInvocation] Payload is null");
            return;
        }

        try
        {
            logger?.LogInformation($"[HandleDirectInvocation] Processing extracted text for S3Key: {payload.ResumeId}");

            // Perform initial analysis (Entity Extraction only, JD is null)
            var result = await _analyzeService.AnalyzeCvAsync(payload.ResumeText, null);

            // Generate a new ResumeId (GUID)
            var resumeId = Guid.NewGuid().ToString();

            var profile = new ProfileRecord
            {
                ResumeId = resumeId,
                Name = result.ResumeEntities.Name,
                Email = result.ResumeEntities.Email,
                Phone = result.ResumeEntities.Phone,
                Skills = result.ResumeEntities.Skills ?? new List<string>(),
                ResumeText = payload.ResumeText,
                LastAnalysis = result,
                CreatedAt = DateTime.UtcNow,
                UserId = payload.UserId, // From payload (extracted from S3 key)
                S3Key = payload.ResumeId // Matches S3 Key
            };
            profile.InitializeKeys();

            logger?.LogInformation($"[HandleDirectInvocation] Saving new profile with ResumeId: {resumeId} for S3Key: {payload.ResumeId}");
            await _profileRepository.SaveProfileAsync(profile);
            
            logger?.LogInformation("[HandleDirectInvocation] Profile saved successfully");
        }
        catch (Exception ex)
        {
            logger?.LogError($"[HandleDirectInvocation] Error saving profile: {ex.Message}");
            throw;
        }
    }
}
