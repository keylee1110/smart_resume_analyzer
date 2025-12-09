using Microsoft.Extensions.Logging;
using TheParser.Exceptions;
using TheParser.Interfaces;
using TheParser.Models;

namespace TheParser.Services;

/// <summary>
/// Service for orchestrating file processing and text extraction
/// Detects file type and routes to appropriate extraction service
/// </summary>
public class FileProcessingService : IFileProcessingService
{
    private readonly IOcrService _ocrService;
    private readonly IDocxExtractor _docxExtractor;
    private readonly ITextNormalizer _textNormalizer;
    private readonly ILogger<FileProcessingService> _logger;

    /// <summary>
    /// Creates a new FileProcessingService instance
    /// </summary>
    /// <param name="ocrService">OCR service for PDF processing</param>
    /// <param name="docxExtractor">DOCX extraction service</param>
    /// <param name="textNormalizer">Text normalization service</param>
    /// <param name="logger">Logger instance</param>
    public FileProcessingService(
        IOcrService ocrService,
        IDocxExtractor docxExtractor,
        ITextNormalizer textNormalizer,
        ILogger<FileProcessingService> logger)
    {
        _ocrService = ocrService ?? throw new ArgumentNullException(nameof(ocrService));
        _docxExtractor = docxExtractor ?? throw new ArgumentNullException(nameof(docxExtractor));
        _textNormalizer = textNormalizer ?? throw new ArgumentNullException(nameof(textNormalizer));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Processes a file from S3 and extracts text based on file type
    /// Requirements: 2.1, 2.2, 2.3
    /// </summary>
    /// <param name="bucketName">S3 bucket name</param>
    /// <param name="objectKey">S3 object key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Processing result with extracted text and metadata</returns>
    public async Task<FileProcessingResult> ProcessFileAsync(
        string bucketName, 
        string objectKey, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(bucketName))
            throw new ArgumentException("Bucket name cannot be null or empty", nameof(bucketName));
        
        if (string.IsNullOrWhiteSpace(objectKey))
            throw new ArgumentException("Object key cannot be null or empty", nameof(objectKey));

        var startTime = DateTime.UtcNow;
        
        var result = new FileProcessingResult
        {
            BucketName = bucketName,
            ObjectKey = objectKey,
            ProcessedAt = startTime,
            Success = false
        };

        try
        {
            // Detect file type from extension (case-insensitive)
            var fileType = DetectFileType(objectKey);
            result.FileType = fileType;
            
            _logger.LogInformation(
                "Starting file processing. Bucket: {BucketName}, Key: {ObjectKey}, FileType: {FileType}",
                bucketName,
                objectKey,
                fileType);

            // Route to appropriate extraction service based on file type
            string rawText;
            if (fileType == FileType.Pdf)
            {
                _logger.LogInformation("Routing to Textract OCR service for PDF extraction. Key: {ObjectKey}", objectKey);
                rawText = await _ocrService.ExtractTextFromPdfAsync(bucketName, objectKey, cancellationToken);
            }
            else if (fileType == FileType.Docx)
            {
                _logger.LogInformation("Routing to DOCX extractor service. Key: {ObjectKey}", objectKey);
                rawText = await _docxExtractor.ExtractTextFromDocxAsync(bucketName, objectKey, cancellationToken);
            }
            else
            {
                _logger.LogError("Unsupported file type detected. Key: {ObjectKey}, FileType: {FileType}", objectKey, fileType);
                throw new UnsupportedFileTypeException(Path.GetExtension(objectKey));
            }

            _logger.LogInformation(
                "Text extraction completed. Key: {ObjectKey}, RawTextLength: {TextLength} characters",
                objectKey,
                rawText?.Length ?? 0);

            // Pass extracted text through normalizer
            _logger.LogInformation("Starting text normalization. Key: {ObjectKey}", objectKey);
            var normalizedText = _textNormalizer.Normalize(rawText);

            // Validate that normalized text is not empty
            if (!_textNormalizer.IsValid(normalizedText))
            {
                _logger.LogWarning(
                    "Normalized text is empty or whitespace-only. Key: {ObjectKey}, RawTextLength: {RawLength}, NormalizedTextLength: {NormalizedLength}",
                    objectKey,
                    rawText?.Length ?? 0,
                    normalizedText?.Length ?? 0);
                result.ErrorMessage = "Extracted text is empty or contains only whitespace";
                return result;
            }

            // Set success result
            result.ExtractedText = normalizedText;
            result.Success = true;
            
            var duration = DateTime.UtcNow - startTime;
            _logger.LogInformation(
                "File processing completed successfully. Key: {ObjectKey}, NormalizedTextLength: {TextLength} characters, Duration: {Duration}ms",
                objectKey,
                normalizedText.Length,
                duration.TotalMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;
            _logger.LogError(
                ex,
                "File processing failed. Key: {ObjectKey}, Duration: {Duration}ms, Error: {ErrorMessage}",
                objectKey,
                duration.TotalMilliseconds,
                ex.Message);
            
            result.Success = false;
            result.ErrorMessage = ex.Message;
            throw;
        }
    }

    /// <summary>
    /// Detects file type from file extension (case-insensitive)
    /// Requirement: 2.1
    /// </summary>
    /// <param name="objectKey">S3 object key with file extension</param>
    /// <returns>Detected file type</returns>
    private FileType DetectFileType(string objectKey)
    {
        var extension = Path.GetExtension(objectKey).ToLowerInvariant();

        return extension switch
        {
            ".pdf" => FileType.Pdf,
            ".docx" => FileType.Docx,
            _ => FileType.Unknown
        };
    }
}
