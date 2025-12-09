using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Microsoft.Extensions.DependencyInjection;
using TheAnalyzer.Interfaces;

namespace TheAnalyzer.Handlers;

/// <summary>
/// Lambda function handler for retrieving all resumes for a user
/// </summary>
public class ListResumesLambdaFunction
{
    private readonly IProfileRepository _profileRepository;
    private readonly ILambdaLogger? _logger;
    
    public ListResumesLambdaFunction()
    {
        var serviceProvider = new ServiceCollection()
            .ConfigureServices()
            .BuildServiceProvider();
        
        _profileRepository = serviceProvider.GetRequiredService<IProfileRepository>();
    }
    
    public ListResumesLambdaFunction(IProfileRepository profileRepository, ILambdaLogger? logger = null)
    {
        _profileRepository = profileRepository ?? throw new ArgumentNullException(nameof(profileRepository));
        _logger = logger;
    }
    
    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        var logger = _logger ?? context?.Logger;
        
        try
        {
            logger?.LogInformation("[ListResumesLambdaFunction] Processing GET request for user resumes");
            
            // 1. Extract UserID from Cognito Authorizer context
            string? userId = null;
            
            if (request.RequestContext?.Authorizer?.Claims != null && 
                request.RequestContext.Authorizer.Claims.TryGetValue("sub", out var cognitoUserId))
            {
                userId = cognitoUserId;
                logger?.LogInformation($"[ListResumesLambdaFunction] User authenticated via Cognito: {userId}");
            }
            else
            {
                logger?.LogWarning("[ListResumesLambdaFunction] Unauthorized: No user ID found in Cognito context");
                return ApiResponseHelper.CreateErrorResponse(
                    401,
                    "Unauthorized - No valid authentication",
                    "AuthenticationError"
                );
            }

            // Extract sortBy and order query parameters
            string sortBy = "createdAt"; // Default
            string order = "desc";       // Default

            if (request.QueryStringParameters != null)
            {
                if (request.QueryStringParameters.TryGetValue("sortBy", out var sortByParam))
                {
                    sortBy = sortByParam;
                }
                if (request.QueryStringParameters.TryGetValue("order", out var orderParam))
                {
                    order = orderParam;
                }
            }
            
            logger?.LogInformation($"[ListResumesLambdaFunction] Retrieving resumes for User: {userId}, SortBy: {sortBy}, Order: {order}");
            
            // 2. Retrieve from DynamoDB using GSI
            var profiles = await _profileRepository.GetProfilesByUserAsync(userId, sortBy, order);
            
            // 3. Return list
            return ApiResponseHelper.CreateResponse(200, profiles);
        }
        catch (Exception ex)
        {
            logger?.LogError($"[ListResumesLambdaFunction] Error: {ex.Message}");
            logger?.LogError($"[ListResumesLambdaFunction] Stack trace: {ex.StackTrace}");
            return ApiResponseHelper.CreateErrorResponse(
                500,
                "Internal server error",
                "InternalError"
            );
        }
    }
}
