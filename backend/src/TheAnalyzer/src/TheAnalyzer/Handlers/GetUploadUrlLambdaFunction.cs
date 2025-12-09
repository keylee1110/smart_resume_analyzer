using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.DependencyInjection;

namespace TheAnalyzer.Handlers;

/// <summary>
/// Lambda function to generate Presigned URLs for S3 uploads
/// </summary>
public class GetUploadUrlLambdaFunction
{
    private readonly IAmazonS3 _s3Client;
    private readonly ILambdaLogger? _logger;
    private readonly string _bucketName;

    public GetUploadUrlLambdaFunction()
    {
        try 
        {
            var serviceProvider = new ServiceCollection()
                .ConfigureServices()
                .BuildServiceProvider();

            _s3Client = serviceProvider.GetRequiredService<IAmazonS3>();
            _bucketName = Environment.GetEnvironmentVariable("BUCKET_NAME") ?? "";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CRITICAL] Error initializing GetUploadUrlLambdaFunction: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            throw; // Rethrow to ensure Lambda fails fast if init fails
        }
    }

    public GetUploadUrlLambdaFunction(IAmazonS3 s3Client, ILambdaLogger? logger = null)
    {
        _s3Client = s3Client;
        _logger = logger;
        _bucketName = Environment.GetEnvironmentVariable("BUCKET_NAME") ?? "";
    }

    public APIGatewayProxyResponse FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        var logger = _logger ?? context.Logger;
        
        try
        {
            logger.LogInformation("Generating Presigned URL");

            // Validate bucket configuration
            if (string.IsNullOrEmpty(_bucketName))
            {
                logger.LogError("Bucket name is not configured.");
                return ApiResponseHelper.CreateErrorResponse(
                    500,
                    "Server configuration error",
                    "ConfigurationError"
                );
            }

            // Extract UserID from Cognito Authorizer context
            string userId = "anonymous";
            
            // When using API Gateway Cognito Authorizer, user info is in requestContext
            if (request.RequestContext?.Authorizer?.Claims != null && 
                request.RequestContext.Authorizer.Claims.TryGetValue("sub", out var cognitoUserId))
            {
                userId = cognitoUserId;
                logger.LogInformation($"User authenticated via Cognito: {userId}");
            }
            else
            {
                logger.LogWarning("No Cognito user ID found in request context");
                return ApiResponseHelper.CreateErrorResponse(
                    401,
                    "Unauthorized - No valid authentication",
                    "AuthenticationError"
                );
            }
            
            logger.LogInformation($"Generating upload URL for User: {userId}");

            // Validate required query parameters
            if (request.QueryStringParameters == null)
            {
                logger.LogWarning("Missing query parameters");
                return ApiResponseHelper.CreateErrorResponse(
                    400,
                    "Query parameters are required: fileName and contentType",
                    "ValidationError"
                );
            }

            if (!request.QueryStringParameters.ContainsKey("fileName") || 
                string.IsNullOrWhiteSpace(request.QueryStringParameters["fileName"]))
            {
                logger.LogWarning("Missing or empty fileName parameter");
                return ApiResponseHelper.CreateErrorResponse(
                    400,
                    "fileName query parameter is required",
                    "ValidationError"
                );
            }

            if (!request.QueryStringParameters.ContainsKey("contentType") || 
                string.IsNullOrWhiteSpace(request.QueryStringParameters["contentType"]))
            {
                logger.LogWarning("Missing or empty contentType parameter");
                return ApiResponseHelper.CreateErrorResponse(
                    400,
                    "contentType query parameter is required",
                    "ValidationError"
                );
            }

            string fileName = request.QueryStringParameters["fileName"];
            string contentType = request.QueryStringParameters["contentType"];

            // Generate S3 Key with UserID prefix: private/{userId}/{guid}-{fileName}
            // This structure allows us to secure access later if needed
            var key = $"private/{userId}/{Guid.NewGuid()}-{fileName}";

            var presignRequest = new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = key,
                Verb = HttpVerb.PUT,
                Expires = DateTime.UtcNow.AddMinutes(5),
                ContentType = contentType
            };

            var url = _s3Client.GetPreSignedURL(presignRequest);
            
            logger.LogInformation($"Successfully generated presigned URL for key: {key}");
            
            return ApiResponseHelper.CreateResponse(
                200,
                new { uploadUrl = url, key = key }
            );
        }
        catch (Exception ex)
        {
            logger.LogError($"Error generating upload URL: {ex.Message}");
            logger.LogError($"Stack trace: {ex.StackTrace}");
            
            return ApiResponseHelper.CreateErrorResponse(
                500,
                "An error occurred while generating the upload URL",
                "InternalError"
            );
        }
    }
}
