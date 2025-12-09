namespace TheParser.Handlers;

using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using TheParser.Interfaces;
using TheParser.Models;
using TheParser.Exceptions;

/// <summary>
/// Lambda function handler for processing resume files uploaded to S3
/// </summary>
public class ParseResumeLambdaFunction
{
    private readonly IFileProcessingService _fileProcessingService;
    private readonly ILambdaInvoker _lambdaInvoker;
    private readonly IAmazonS3 _s3Client;
    private readonly long _maxFileSizeBytes;
    
    /// <summary>
    /// Creates a new instance of ParseResumeLambdaFunction with default dependencies
    /// </summary>
    public ParseResumeLambdaFunction()
        : this(
            CreateDefaultFileProcessingService(),
            CreateDefaultLambdaInvoker(),
            new Amazon.S3.AmazonS3Client())
    {
    }
    
    /// <summary>
    /// Creates a default file processing service with all dependencies
    /// </summary>
    private static IFileProcessingService CreateDefaultFileProcessingService()
    {
        var s3Client = new Amazon.S3.AmazonS3Client();
        var textractClient = new Amazon.Textract.AmazonTextractClient();
        
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
        });
        
        var textractLogger = loggerFactory.CreateLogger<Services.TextractOcrService>();
        var docxLogger = loggerFactory.CreateLogger<Services.DocxExtractor>();
        var normalizerLogger = loggerFactory.CreateLogger<Services.TextNormalizer>();
        var fileProcessingLogger = loggerFactory.CreateLogger<Services.FileProcessingService>();
        
        return new Services.FileProcessingService(
            new Services.TextractOcrService(textractClient, textractLogger),
            new Services.DocxExtractor(s3Client, docxLogger),
            new Services.TextNormalizer(normalizerLogger),
            fileProcessingLogger);
    }
    
    /// <summary>
    /// Creates a default Lambda invoker with all dependencies
    /// </summary>
    private static ILambdaInvoker CreateDefaultLambdaInvoker()
    {
        var lambdaClient = new Amazon.Lambda.AmazonLambdaClient();
        var analyzerFunctionName = Environment.GetEnvironmentVariable("ANALYZER_FUNCTION_NAME") ?? "AnalyzeResumeFunction";
        
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
        });
        
        var logger = loggerFactory.CreateLogger<Services.LambdaInvoker>();
        
        return new Services.LambdaInvoker(lambdaClient, logger, analyzerFunctionName);
    }
    
    /// <summary>
    /// Creates a new instance of ParseResumeLambdaFunction with injected dependencies
    /// </summary>
    /// <param name="fileProcessingService">Service for processing files</param>
    /// <param name="lambdaInvoker">Service for invoking Lambda functions</param>
    /// <param name="s3Client">S3 client for accessing file metadata</param>
    public ParseResumeLambdaFunction(
        IFileProcessingService fileProcessingService,
        ILambdaInvoker lambdaInvoker,
        IAmazonS3 s3Client)
    {
        _fileProcessingService = fileProcessingService ?? throw new ArgumentNullException(nameof(fileProcessingService));
        _lambdaInvoker = lambdaInvoker ?? throw new ArgumentNullException(nameof(lambdaInvoker));
        _s3Client = s3Client ?? throw new ArgumentNullException(nameof(s3Client));
        
        // Get max file size from environment variable or default to 10MB
        var maxFileSizeMb = int.TryParse(Environment.GetEnvironmentVariable("MAX_FILE_SIZE_MB"), out var size) 
            ? size 
            : 10;
        _maxFileSizeBytes = maxFileSizeMb * 1024 * 1024;
    }
    
    /// <summary>
    /// Lambda function handler that processes S3 events for uploaded resume files
    /// </summary>
    /// <param name="s3Event">S3 event containing information about uploaded files</param>
    /// <param name="context">Lambda execution context</param>
    [LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
    public async Task FunctionHandler(S3Event s3Event, ILambdaContext context)
    {
        // Generate correlation ID for request tracing
        var correlationId = Guid.NewGuid().ToString();
        
        try
        {
            // Validate S3 event
            if (s3Event?.Records == null || s3Event.Records.Count == 0)
            {
                context.Logger.LogLine($"[{correlationId}] No S3 records found in event");
                return;
            }
            
            // Process each S3 record
            foreach (var record in s3Event.Records)
            {
                await ProcessS3RecordAsync(record, correlationId, context);
            }
        }
        catch (Exception ex)
        {
            context.Logger.LogLine($"[{correlationId}] Unexpected error processing S3 event: {ex.Message}");
            context.Logger.LogLine($"[{correlationId}] Stack trace: {ex.StackTrace}");
            throw;
        }
    }
    
    /// <summary>
    /// Processes a single S3 record
    /// </summary>
    private async Task ProcessS3RecordAsync(S3Event.S3EventNotificationRecord record, string correlationId, ILambdaContext context)
    {
        var startTime = DateTime.UtcNow;
        
        try
        {
            // Extract bucket name and object key
            var bucketName = record.S3.Bucket.Name;
            var objectKey = record.S3.Object.Key;
            
            context.Logger.LogLine($"[{correlationId}] Processing file: s3://{bucketName}/{objectKey}");
            
            // Validate file extension
            ValidateFileExtension(objectKey, correlationId, context);
            
            // Get S3 object metadata to check file size
            await ValidateFileSizeAsync(bucketName, objectKey, correlationId, context);
            
            // Process file to extract and normalize text
            context.Logger.LogLine($"[{correlationId}] Starting text extraction");
            var processingResult = await _fileProcessingService.ProcessFileAsync(bucketName, objectKey);
            
            if (!processingResult.Success)
            {
                throw new TextExtractionException(
                    processingResult.ErrorMessage ?? "Unknown error during text extraction",
                    null!);
            }
            
            // Perform Text Extraction (OCR/Textract)
            context.Logger.LogLine($"[{correlationId}] Text extraction completed. Extracted {processingResult.ExtractedText.Length} characters");
            
            // Extract UserID from S3 Key (Format: private/{userId}/{guid}-{filename})
            string userId = "anonymous";
            var keyParts = objectKey.Split('/');
            if (keyParts.Length >= 3 && keyParts[0] == "private")
            {
                userId = keyParts[1];
            }

            // Build payload for Analyzer function
            var payload = new AnalyzerInvocationPayload
            {
                ResumeId = objectKey,
                ResumeText = processingResult.ExtractedText,
                SourceBucket = bucketName,
                SourceKey = objectKey,
                FileType = processingResult.FileType.ToString(),
                ExtractedAt = processingResult.ProcessedAt,
                CorrelationId = correlationId,
                UserId = userId
            };
            
            // Invoke Analyzer function
            context.Logger.LogLine($"[{correlationId}] Invoking Analyzer function with UserId: {payload.UserId}");
            
            await _lambdaInvoker.InvokeAnalyzerAsync(payload);
            
            var duration = DateTime.UtcNow - startTime;
            context.Logger.LogLine($"[{correlationId}] Processing completed successfully in {duration.TotalSeconds:F2} seconds");
        }
        catch (UnsupportedFileTypeException ex)
        {
            ex.CorrelationId = correlationId;
            context.Logger.LogLine($"[{correlationId}] ERROR: {ex.ErrorCode} - {ex.Message}");
            throw;
        }
        catch (FileSizeExceededException ex)
        {
            ex.CorrelationId = correlationId;
            context.Logger.LogLine($"[{correlationId}] ERROR: {ex.ErrorCode} - {ex.Message}");
            context.Logger.LogLine($"[{correlationId}] File size: {ex.FileSize} bytes, Max allowed: {ex.MaxSize} bytes");
            throw;
        }
        catch (TextExtractionException ex)
        {
            ex.CorrelationId = correlationId;
            context.Logger.LogLine($"[{correlationId}] ERROR: {ex.ErrorCode} - {ex.Message}");
            if (ex.InnerException != null)
            {
                context.Logger.LogLine($"[{correlationId}] Inner exception: {ex.InnerException.Message}");
            }
            context.Logger.LogLine($"[{correlationId}] Stack trace: {ex.StackTrace}");
            throw;
        }
        catch (AnalyzerInvocationException ex)
        {
            ex.CorrelationId = correlationId;
            context.Logger.LogLine($"[{correlationId}] ERROR: {ex.ErrorCode} - {ex.Message}");
            if (ex.InnerException != null)
            {
                context.Logger.LogLine($"[{correlationId}] Inner exception: {ex.InnerException.Message}");
            }
            context.Logger.LogLine($"[{correlationId}] Stack trace: {ex.StackTrace}");
            throw;
        }
        catch (ParserException ex)
        {
            ex.CorrelationId = correlationId;
            context.Logger.LogLine($"[{correlationId}] ERROR: {ex.ErrorCode} - {ex.Message}");
            context.Logger.LogLine($"[{correlationId}] Stack trace: {ex.StackTrace}");
            throw;
        }
        catch (Exception ex)
        {
            context.Logger.LogLine($"[{correlationId}] ERROR: Unexpected error - {ex.Message}");
            context.Logger.LogLine($"[{correlationId}] Exception type: {ex.GetType().Name}");
            context.Logger.LogLine($"[{correlationId}] Stack trace: {ex.StackTrace}");
            throw;
        }
    }
    
    /// <summary>
    /// Validates that the file has a supported extension (.pdf or .docx)
    /// </summary>
    private void ValidateFileExtension(string objectKey, string correlationId, ILambdaContext context)
    {
        var extension = Path.GetExtension(objectKey).ToLowerInvariant();
        
        context.Logger.LogLine($"[{correlationId}] Validating file extension: {extension}");
        
        if (extension != ".pdf" && extension != ".docx")
        {
            context.Logger.LogLine($"[{correlationId}] Validation failed: Unsupported file extension '{extension}'");
            throw new UnsupportedFileTypeException(extension);
        }
        
        context.Logger.LogLine($"[{correlationId}] File extension validation passed: {extension}");
    }
    
    /// <summary>
    /// Validates that the file size does not exceed the maximum allowed size
    /// </summary>
    private async Task ValidateFileSizeAsync(string bucketName, string objectKey, string correlationId, ILambdaContext context)
    {
        try
        {
            context.Logger.LogLine($"[{correlationId}] Retrieving file metadata from S3");
            
            var metadataRequest = new GetObjectMetadataRequest
            {
                BucketName = bucketName,
                Key = objectKey
            };
            
            var metadata = await _s3Client.GetObjectMetadataAsync(metadataRequest);
            var fileSize = metadata.ContentLength;
            var fileSizeMB = fileSize / 1024.0 / 1024.0;
            var maxSizeMB = _maxFileSizeBytes / 1024.0 / 1024.0;
            
            context.Logger.LogLine($"[{correlationId}] File size: {fileSize} bytes ({fileSizeMB:F2} MB), Max allowed: {_maxFileSizeBytes} bytes ({maxSizeMB:F2} MB)");
            
            if (fileSize > _maxFileSizeBytes)
            {
                context.Logger.LogLine($"[{correlationId}] Validation failed: File size exceeds maximum allowed size");
                throw new FileSizeExceededException(fileSize, _maxFileSizeBytes);
            }
            
            // Log warning if file is approaching size limit (> 80% of max)
            if (fileSize > _maxFileSizeBytes * 0.8)
            {
                context.Logger.LogLine($"[{correlationId}] WARNING: File size is approaching limit ({fileSizeMB:F2} MB / {maxSizeMB:F2} MB)");
            }
            
            context.Logger.LogLine($"[{correlationId}] File size validation passed");
        }
        catch (AmazonS3Exception ex)
        {
            context.Logger.LogLine($"[{correlationId}] ERROR: Failed to get S3 object metadata - {ex.Message}");
            context.Logger.LogLine($"[{correlationId}] S3 Error Code: {ex.ErrorCode}, Status Code: {ex.StatusCode}");
            throw new ParserException($"Failed to access S3 object: {ex.Message}", "S3_ACCESS_ERROR", ex);
        }
    }
}
