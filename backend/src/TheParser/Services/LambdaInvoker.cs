namespace TheParser.Services;

using System.Text;
using System.Text.Json;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using Microsoft.Extensions.Logging;
using TheParser.Exceptions;
using TheParser.Interfaces;
using TheParser.Models;

/// <summary>
/// Service for invoking downstream Lambda functions with retry logic
/// </summary>
public class LambdaInvoker : ILambdaInvoker
{
    private readonly IAmazonLambda _lambdaClient;
    private readonly ILogger<LambdaInvoker> _logger;
    private readonly string _analyzerFunctionName;
    private const int MaxRetries = 2;
    private const int BaseDelayMs = 1000;

    /// <summary>
    /// Creates a new LambdaInvoker instance
    /// </summary>
    /// <param name="lambdaClient">AWS Lambda client</param>
    /// <param name="logger">Logger instance</param>
    /// <param name="analyzerFunctionName">Name or ARN of the Analyzer function</param>
    public LambdaInvoker(
        IAmazonLambda lambdaClient,
        ILogger<LambdaInvoker> logger,
        string analyzerFunctionName)
    {
        _lambdaClient = lambdaClient ?? throw new ArgumentNullException(nameof(lambdaClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _analyzerFunctionName = analyzerFunctionName ?? throw new ArgumentNullException(nameof(analyzerFunctionName));
    }

    /// <summary>
    /// Invokes the Analyzer Lambda function asynchronously with retry logic
    /// </summary>
    /// <param name="payload">The payload to send to the Analyzer function</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the asynchronous operation</returns>
    public async Task InvokeAnalyzerAsync(AnalyzerInvocationPayload payload, CancellationToken cancellationToken = default)
    {
        if (payload == null)
        {
            throw new ArgumentNullException(nameof(payload));
        }

        var correlationId = payload.CorrelationId;
        var attempt = 0;
        Exception? lastException = null;

        while (attempt <= MaxRetries)
        {
            attempt++;
            
            try
            {
                _logger.LogInformation(
                    "Invoking Analyzer function. CorrelationId: {CorrelationId}, Attempt: {Attempt}/{MaxAttempts}, Function: {FunctionName}",
                    correlationId,
                    attempt,
                    MaxRetries + 1,
                    _analyzerFunctionName);

                // Serialize payload to JSON
                var jsonPayload = JsonSerializer.Serialize(payload, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                // Create invocation request
                var request = new InvokeRequest
                {
                    FunctionName = _analyzerFunctionName,
                    InvocationType = InvocationType.Event, // Asynchronous invocation
                    Payload = jsonPayload
                };

                // Invoke the Lambda function
                var response = await _lambdaClient.InvokeAsync(request, cancellationToken);

                // Check for successful invocation (status code 202 for async invocation)
                if (response.StatusCode == 202)
                {
                    _logger.LogInformation(
                        "Successfully invoked Analyzer function. CorrelationId: {CorrelationId}, StatusCode: {StatusCode}",
                        correlationId,
                        response.StatusCode);
                    return;
                }
                else
                {
                    var errorMessage = $"Unexpected status code from Analyzer invocation: {response.StatusCode}";
                    _logger.LogWarning(
                        "Analyzer invocation returned unexpected status. CorrelationId: {CorrelationId}, StatusCode: {StatusCode}, Attempt: {Attempt}",
                        correlationId,
                        response.StatusCode,
                        attempt);
                    
                    lastException = new ParserException(errorMessage, "ANALYZER_INVOCATION_FAILED");
                }
            }
            catch (Exception ex)
            {
                lastException = ex;
                _logger.LogError(
                    ex,
                    "Error invoking Analyzer function. CorrelationId: {CorrelationId}, Attempt: {Attempt}/{MaxAttempts}, Error: {ErrorMessage}",
                    correlationId,
                    attempt,
                    MaxRetries + 1,
                    ex.Message);
            }

            // If we haven't exhausted retries, wait before retrying
            if (attempt <= MaxRetries)
            {
                var delayMs = BaseDelayMs * attempt; // Linear backoff: 1s, 2s
                _logger.LogInformation(
                    "Retrying Analyzer invocation after {DelayMs}ms. CorrelationId: {CorrelationId}",
                    delayMs,
                    correlationId);
                await Task.Delay(delayMs, cancellationToken);
            }
        }

        // All retries exhausted
        var finalErrorMessage = $"Failed to invoke Analyzer function after {MaxRetries + 1} attempts";
        _logger.LogError(
            "Analyzer invocation failed after all retries. CorrelationId: {CorrelationId}, Attempts: {Attempts}",
            correlationId,
            MaxRetries + 1);

        throw new AnalyzerInvocationException(finalErrorMessage, lastException!)
        {
            CorrelationId = correlationId
        };
    }
}
