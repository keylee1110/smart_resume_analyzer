namespace TheParser.Interfaces;

using TheParser.Models;

/// <summary>
/// Service for invoking downstream Lambda functions
/// </summary>
public interface ILambdaInvoker
{
    /// <summary>
    /// Invokes the Analyzer Lambda function asynchronously with the provided payload
    /// </summary>
    /// <param name="payload">The payload to send to the Analyzer function</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the asynchronous operation</returns>
    Task InvokeAnalyzerAsync(AnalyzerInvocationPayload payload, CancellationToken cancellationToken = default);
}
