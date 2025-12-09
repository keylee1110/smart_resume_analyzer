using FsCheck;
using FsCheck.Xunit;
using Xunit;

namespace TheAnalyzer.Tests;

/// <summary>
/// Property-based tests for ApiResponseHelper
/// </summary>
public class ApiResponseHelperPropertyTests
{
    /// <summary>
    /// Feature: api-endpoint-fixes, Property 1: CORS Headers Present
    /// Validates: Requirements 3.1, 3.2, 3.3, 4.3
    /// 
    /// For any API response (success or error), the response SHALL include 
    /// the required CORS headers (Access-Control-Allow-Origin, 
    /// Access-Control-Allow-Headers, Access-Control-Allow-Methods)
    /// </summary>
    [Property(MaxTest = 100)]
    public Property CreateResponse_AlwaysIncludesCorsHeaders()
    {
        // Generate random status codes and response bodies
        return Prop.ForAll(
            StatusCodeGenerator(),
            ResponseBodyGenerator(),
            (statusCode, body) =>
            {
                // Create response using ApiResponseHelper
                var response = ApiResponseHelper.CreateResponse(statusCode, body);
                
                // Verify all required CORS headers are present
                var hasOriginHeader = response.Headers.ContainsKey("Access-Control-Allow-Origin");
                var hasHeadersHeader = response.Headers.ContainsKey("Access-Control-Allow-Headers");
                var hasMethodsHeader = response.Headers.ContainsKey("Access-Control-Allow-Methods");
                
                // Verify header values are not null or empty
                var originNotEmpty = hasOriginHeader && !string.IsNullOrEmpty(response.Headers["Access-Control-Allow-Origin"]);
                var headersNotEmpty = hasHeadersHeader && !string.IsNullOrEmpty(response.Headers["Access-Control-Allow-Headers"]);
                var methodsNotEmpty = hasMethodsHeader && !string.IsNullOrEmpty(response.Headers["Access-Control-Allow-Methods"]);
                
                return hasOriginHeader && hasHeadersHeader && hasMethodsHeader &&
                       originNotEmpty && headersNotEmpty && methodsNotEmpty;
            });
    }

    /// <summary>
    /// Feature: api-endpoint-fixes, Property 1: CORS Headers Present
    /// Validates: Requirements 3.1, 3.2, 3.3, 4.3
    /// 
    /// For any error response, the response SHALL include the required CORS headers
    /// </summary>
    [Property(MaxTest = 100)]
    public Property CreateErrorResponse_AlwaysIncludesCorsHeaders()
    {
        // Generate random error status codes, messages, and error types
        return Prop.ForAll(
            ErrorStatusCodeGenerator(),
            ErrorMessageGenerator(),
            ErrorTypeGenerator(),
            (statusCode, message, errorType) =>
            {
                // Create error response using ApiResponseHelper
                var response = ApiResponseHelper.CreateErrorResponse(statusCode, message, errorType);
                
                // Verify all required CORS headers are present
                var hasOriginHeader = response.Headers.ContainsKey("Access-Control-Allow-Origin");
                var hasHeadersHeader = response.Headers.ContainsKey("Access-Control-Allow-Headers");
                var hasMethodsHeader = response.Headers.ContainsKey("Access-Control-Allow-Methods");
                
                // Verify header values are not null or empty
                var originNotEmpty = hasOriginHeader && !string.IsNullOrEmpty(response.Headers["Access-Control-Allow-Origin"]);
                var headersNotEmpty = hasHeadersHeader && !string.IsNullOrEmpty(response.Headers["Access-Control-Allow-Headers"]);
                var methodsNotEmpty = hasMethodsHeader && !string.IsNullOrEmpty(response.Headers["Access-Control-Allow-Methods"]);
                
                return hasOriginHeader && hasHeadersHeader && hasMethodsHeader &&
                       originNotEmpty && headersNotEmpty && methodsNotEmpty;
            });
    }

    /// <summary>
    /// Feature: api-endpoint-fixes, Property 1: CORS Headers Present
    /// Validates: Requirements 3.1, 3.2, 3.3, 4.3
    /// 
    /// For any response with additional headers, the CORS headers SHALL still be present
    /// </summary>
    [Property(MaxTest = 100)]
    public Property CreateResponseWithAdditionalHeaders_StillIncludesCorsHeaders()
    {
        // Generate random status codes, bodies, and additional headers
        return Prop.ForAll(
            StatusCodeGenerator(),
            ResponseBodyGenerator(),
            AdditionalHeadersGenerator(),
            (statusCode, body, additionalHeaders) =>
            {
                // Create response with additional headers
                var response = ApiResponseHelper.CreateResponse(statusCode, body, additionalHeaders);
                
                // Verify all required CORS headers are present
                var hasOriginHeader = response.Headers.ContainsKey("Access-Control-Allow-Origin");
                var hasHeadersHeader = response.Headers.ContainsKey("Access-Control-Allow-Headers");
                var hasMethodsHeader = response.Headers.ContainsKey("Access-Control-Allow-Methods");
                
                // Verify header values are not null or empty
                var originNotEmpty = hasOriginHeader && !string.IsNullOrEmpty(response.Headers["Access-Control-Allow-Origin"]);
                var headersNotEmpty = hasHeadersHeader && !string.IsNullOrEmpty(response.Headers["Access-Control-Allow-Headers"]);
                var methodsNotEmpty = hasMethodsHeader && !string.IsNullOrEmpty(response.Headers["Access-Control-Allow-Methods"]);
                
                // Verify additional headers are also present
                var additionalHeadersPresent = additionalHeaders == null || 
                    additionalHeaders.All(kvp => response.Headers.ContainsKey(kvp.Key));
                
                return hasOriginHeader && hasHeadersHeader && hasMethodsHeader &&
                       originNotEmpty && headersNotEmpty && methodsNotEmpty &&
                       additionalHeadersPresent;
            });
    }

    /// <summary>
    /// Generator for HTTP status codes (covering 2xx, 4xx, 5xx ranges)
    /// </summary>
    private static Arbitrary<int> StatusCodeGenerator()
    {
        var statusCodeGen = Gen.OneOf(
            Gen.Choose(200, 299), // Success codes
            Gen.Choose(400, 499), // Client error codes
            Gen.Choose(500, 599)  // Server error codes
        );
        return Arb.From(statusCodeGen);
    }

    /// <summary>
    /// Generator for error status codes (4xx and 5xx only)
    /// </summary>
    private static Arbitrary<int> ErrorStatusCodeGenerator()
    {
        var errorCodeGen = Gen.OneOf(
            Gen.Choose(400, 499), // Client error codes
            Gen.Choose(500, 599)  // Server error codes
        );
        return Arb.From(errorCodeGen);
    }

    /// <summary>
    /// Generator for response body objects
    /// </summary>
    private static Arbitrary<object> ResponseBodyGenerator()
    {
        var bodyGen = Gen.OneOf(
            Gen.Constant<object>(new { message = "Success" }),
            Gen.Constant<object>(new { data = "test data", count = 42 }),
            Gen.Constant<object>(new { id = "123", name = "Test", active = true }),
            Gen.Constant<object>(new { items = new[] { "item1", "item2", "item3" } }),
            Gen.Constant<object>(new { })
        );
        return Arb.From(bodyGen);
    }

    /// <summary>
    /// Generator for error messages
    /// </summary>
    private static Arbitrary<string> ErrorMessageGenerator()
    {
        var messageGen = Gen.Elements(
            "Invalid request",
            "Resource not found",
            "Internal server error",
            "Validation failed",
            "Unauthorized access",
            "Bad request",
            "Service unavailable"
        );
        return Arb.From(messageGen);
    }

    /// <summary>
    /// Generator for error types
    /// </summary>
    private static Arbitrary<string> ErrorTypeGenerator()
    {
        var typeGen = Gen.Elements(
            "ValidationError",
            "NotFoundError",
            "InternalError",
            "AuthenticationError",
            "AuthorizationError",
            "ServiceError"
        );
        return Arb.From(typeGen);
    }

    /// <summary>
    /// Generator for additional headers dictionary
    /// </summary>
    private static Arbitrary<Dictionary<string, string>?> AdditionalHeadersGenerator()
    {
        var headerGen = Gen.OneOf(
            Gen.Constant<Dictionary<string, string>?>(null),
            Gen.Constant<Dictionary<string, string>?>(new Dictionary<string, string>()),
            Gen.Constant<Dictionary<string, string>?>(new Dictionary<string, string>
            {
                { "X-Custom-Header", "custom-value" }
            }),
            Gen.Constant<Dictionary<string, string>?>(new Dictionary<string, string>
            {
                { "X-Request-Id", "req-123" },
                { "X-Correlation-Id", "corr-456" }
            })
        );
        return Arb.From(headerGen);
    }
}
