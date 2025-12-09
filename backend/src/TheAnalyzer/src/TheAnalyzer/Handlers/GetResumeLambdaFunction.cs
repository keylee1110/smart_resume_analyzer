using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using TheAnalyzer.Interfaces;
using TheAnalyzer.Models;

namespace TheAnalyzer.Handlers;

/// <summary>
/// Lambda function handler for retrieving resume profiles via API Gateway
/// </summary>
public class GetResumeLambdaFunction
{
    private readonly IProfileRepository _profileRepository;
    private readonly ILambdaLogger? _logger;
    
    /// <summary>
    /// Default constructor for AWS Lambda runtime
    /// Initializes services using dependency injection
    /// </summary>
    public GetResumeLambdaFunction()
    {
        var serviceProvider = new ServiceCollection()
            .ConfigureServices()
            .BuildServiceProvider();
        
        _profileRepository = serviceProvider.GetRequiredService<IProfileRepository>();
    }
    
    /// <summary>
    /// Constructor with dependency injection for testing
    /// </summary>
    public GetResumeLambdaFunction(IProfileRepository profileRepository, ILambdaLogger? logger = null)
    {
        _profileRepository = profileRepository ?? throw new ArgumentNullException(nameof(profileRepository));
        _logger = logger;
    }
    
    /// <summary>
    /// Main Lambda function handler for retrieving resume profiles
    /// </summary>
    /// <param name="request">API Gateway proxy request</param>
    /// <param name="context">Lambda execution context</param>
    /// <returns>API Gateway proxy response with profile data or error</returns>
    public async Task<APIGatewayProxyResponse> FunctionHandler(
        APIGatewayProxyRequest request, 
        ILambdaContext context)
    {
        var logger = _logger ?? context?.Logger;
        
        try
        {
            logger?.LogInformation("[GetResumeLambdaFunction] Processing GET request for resume profile");
            
            // 1. Extract resume ID from path parameters
            if (request?.PathParameters == null || 
                !request.PathParameters.TryGetValue("id", out var resumeId) ||
                string.IsNullOrWhiteSpace(resumeId))
            {
                logger?.LogWarning("[GetResumeLambdaFunction] Missing or empty resume ID in path parameters");
                return ApiResponseHelper.CreateErrorResponse(
                    400,
                    "Missing resume ID",
                    "ValidationError"
                );
            }
            
            logger?.LogInformation($"[GetResumeLambdaFunction] Retrieving profile for ResumeId: {resumeId}");
            
            // 2. Retrieve from DynamoDB
            ProfileRecord? profile = null;
            var decodedId = System.Net.WebUtility.UrlDecode(resumeId);

            if (decodedId.Contains("private/", StringComparison.OrdinalIgnoreCase))
            {
                logger?.LogInformation($"[GetResumeLambdaFunction] ResumeId looks like an S3 key. Querying by S3 Key: {decodedId}");
                profile = await _profileRepository.GetProfileByS3KeyAsync(decodedId);
            }
            else
            {
                profile = await _profileRepository.GetProfileAsync(decodedId);
            }
            
            // 3. Handle not found
            if (profile == null)
            {
                logger?.LogInformation($"[GetResumeLambdaFunction] Profile not found for ResumeId: {resumeId}");
                return ApiResponseHelper.CreateErrorResponse(
                    404,
                    "Resume not found",
                    "NotFoundError"
                );
            }
            
            // 4. Generate presigned URL for the resume PDF if S3Key exists
            string? presignedUrl = null;
            if (!string.IsNullOrEmpty(profile.S3Key))
            {
                try
                {
                    var bucketName = Environment.GetEnvironmentVariable("BUCKET_NAME");
                    var s3Client = new Amazon.S3.AmazonS3Client();
                    var presignedRequest = new Amazon.S3.Model.GetPreSignedUrlRequest
                    {
                        BucketName = bucketName,
                        Key = profile.S3Key,
                        Expires = DateTime.UtcNow.AddMinutes(15),
                        Verb = Amazon.S3.HttpVerb.GET
                    };
                    presignedUrl = s3Client.GetPreSignedURL(presignedRequest);
                    logger?.LogInformation($"[GetResumeLambdaFunction] Generated presigned URL for S3Key: {profile.S3Key}");
                }
                catch (Exception ex)
                {
                    logger?.LogWarning($"[GetResumeLambdaFunction] Error generating presigned URL: {ex.Message}");
                }
            }

            // 5. Create response with presigned URL
            var responseData = new
            {
                resumeId = profile.ResumeId,
                userId = profile.UserId,
                name = profile.Name,
                email = profile.Email,
                phone = profile.Phone,
                skills = profile.Skills,
                s3Key = profile.S3Key,
                presignedUrl = presignedUrl,
                createdAt = profile.CreatedAt,
                resumeText = profile.ResumeText,
                lastAnalysis = profile.LastAnalysis
            };
            
            logger?.LogInformation($"[GetResumeLambdaFunction] Successfully retrieved profile for ResumeId: {resumeId}");
            return ApiResponseHelper.CreateResponse(200, responseData);
        }
        catch (Exception ex)
        {
            logger?.LogError($"[GetResumeLambdaFunction] Error retrieving resume: {ex.Message}");
            logger?.LogError($"[GetResumeLambdaFunction] Stack trace: {ex.StackTrace}");
            return ApiResponseHelper.CreateErrorResponse(
                500,
                "Internal server error",
                "InternalError"
            );
        }
    }
}
