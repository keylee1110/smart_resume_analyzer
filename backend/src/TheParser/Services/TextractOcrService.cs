using Amazon.Textract;
using Amazon.Textract.Model;
using Microsoft.Extensions.Logging;
using TheParser.Exceptions;
using TheParser.Interfaces;

namespace TheParser.Services;

/// <summary>
/// Service for extracting text from PDF documents using AWS Textract OCR
/// </summary>
public class TextractOcrService : IOcrService
{
    private readonly IAmazonTextract _textractClient;
    private readonly ILogger<TextractOcrService> _logger;
    private const int MaxRetries = 2;
    private const int InitialRetryDelayMs = 1000;

    /// <summary>
    /// Creates a new TextractOcrService instance
    /// </summary>
    /// <param name="textractClient">Textract client for OCR operations</param>
    /// <param name="logger">Logger instance</param>
    public TextractOcrService(IAmazonTextract textractClient, ILogger<TextractOcrService> logger)
    {
        _textractClient = textractClient ?? throw new ArgumentNullException(nameof(textractClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Extracts text from a PDF document in S3 using AWS Textract
    /// </summary>
    /// <param name="bucketName">S3 bucket containing the document</param>
    /// <param name="objectKey">S3 object key of the document</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Extracted text content from all pages</returns>
    /// <exception cref="TextExtractionException">Thrown when Textract extraction fails</exception>
    public async Task<string> ExtractTextFromPdfAsync(
        string bucketName, 
        string objectKey, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(bucketName))
            throw new ArgumentException("Bucket name cannot be null or empty", nameof(bucketName));
        
        if (string.IsNullOrWhiteSpace(objectKey))
            throw new ArgumentException("Object key cannot be null or empty", nameof(objectKey));

        var startTime = DateTime.UtcNow;
        
        _logger.LogInformation(
            "Starting Textract PDF extraction. Bucket: {BucketName}, Key: {ObjectKey}",
            bucketName,
            objectKey);

        var request = new DetectDocumentTextRequest
        {
            Document = new Document
            {
                S3Object = new Amazon.Textract.Model.S3Object
                {
                    Bucket = bucketName,
                    Name = objectKey
                }
            }
        };

        // Implement retry logic with exponential backoff
        Exception? lastException = null;
        for (int attempt = 0; attempt <= MaxRetries; attempt++)
        {
            try
            {
                _logger.LogInformation(
                    "Calling Textract DetectDocumentText API. Key: {ObjectKey}, Attempt: {Attempt}/{MaxAttempts}",
                    objectKey,
                    attempt + 1,
                    MaxRetries + 1);
                
                var response = await _textractClient.DetectDocumentTextAsync(request, cancellationToken);
                var extractedText = ExtractTextFromResponse(response);
                
                var duration = DateTime.UtcNow - startTime;
                _logger.LogInformation(
                    "Textract extraction completed successfully. Key: {ObjectKey}, BlockCount: {BlockCount}, ExtractedTextLength: {TextLength} characters, Duration: {Duration}ms",
                    objectKey,
                    response.Blocks?.Count ?? 0,
                    extractedText.Length,
                    duration.TotalMilliseconds);
                
                return extractedText;
            }
            catch (AmazonTextractException ex) when (IsTransientError(ex) && attempt < MaxRetries)
            {
                lastException = ex;
                var delayMs = InitialRetryDelayMs * (int)Math.Pow(2, attempt);
                
                _logger.LogWarning(
                    "Textract transient error, retrying. Key: {ObjectKey}, Attempt: {Attempt}, ErrorCode: {ErrorCode}, StatusCode: {StatusCode}, RetryDelay: {DelayMs}ms",
                    objectKey,
                    attempt + 1,
                    ex.ErrorCode,
                    ex.StatusCode,
                    delayMs);
                
                await Task.Delay(delayMs, cancellationToken);
            }
            catch (AmazonTextractException ex)
            {
                var duration = DateTime.UtcNow - startTime;
                _logger.LogError(
                    ex,
                    "Textract extraction failed. Key: {ObjectKey}, ErrorCode: {ErrorCode}, StatusCode: {StatusCode}, Duration: {Duration}ms",
                    objectKey,
                    ex.ErrorCode,
                    ex.StatusCode,
                    duration.TotalMilliseconds);
                
                throw new TextExtractionException(
                    $"Textract failed to extract text from PDF: {ex.Message}", 
                    ex);
            }
            catch (Exception ex)
            {
                var duration = DateTime.UtcNow - startTime;
                _logger.LogError(
                    ex,
                    "Unexpected error during Textract extraction. Key: {ObjectKey}, Duration: {Duration}ms",
                    objectKey,
                    duration.TotalMilliseconds);
                
                throw new TextExtractionException(
                    $"Unexpected error during PDF text extraction: {ex.Message}", 
                    ex);
            }
        }

        // If we exhausted all retries
        var finalDuration = DateTime.UtcNow - startTime;
        _logger.LogError(
            lastException,
            "Textract extraction failed after all retries. Key: {ObjectKey}, Attempts: {Attempts}, Duration: {Duration}ms",
            objectKey,
            MaxRetries + 1,
            finalDuration.TotalMilliseconds);
        
        throw new TextExtractionException(
            $"Textract extraction failed after {MaxRetries} retries: {lastException?.Message}", 
            lastException!);
    }

    /// <summary>
    /// Extracts and concatenates all text blocks from Textract response
    /// </summary>
    /// <param name="response">Textract DetectDocumentText response</param>
    /// <returns>Concatenated text from all text blocks</returns>
    private string ExtractTextFromResponse(DetectDocumentTextResponse response)
    {
        if (response?.Blocks == null || response.Blocks.Count == 0)
        {
            return string.Empty;
        }

        // Extract only LINE and WORD blocks (not PAGE blocks)
        // LINE blocks contain complete lines of text
        var textBlocks = response.Blocks
            .Where(block => block.BlockType == BlockType.LINE)
            .OrderBy(block => block.Geometry?.BoundingBox?.Top ?? 0)
            .Select(block => block.Text)
            .Where(text => !string.IsNullOrEmpty(text));

        return string.Join("\n", textBlocks);
    }

    /// <summary>
    /// Determines if a Textract exception is transient and should be retried
    /// </summary>
    /// <param name="exception">The Textract exception</param>
    /// <returns>True if the error is transient</returns>
    private bool IsTransientError(AmazonTextractException exception)
    {
        // Retry on throttling, service unavailable, and internal errors
        return exception.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable ||
               exception.StatusCode == System.Net.HttpStatusCode.InternalServerError ||
               exception.ErrorCode == "ThrottlingException" ||
               exception.ErrorCode == "ProvisionedThroughputExceededException";
    }
}
