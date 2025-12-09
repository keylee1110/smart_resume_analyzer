using Amazon.S3;
using Amazon.S3.Model;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.Logging;
using TheParser.Exceptions;
using TheParser.Interfaces;

namespace TheParser.Services;

/// <summary>
/// Service for extracting text from DOCX files stored in S3
/// </summary>
public class DocxExtractor : IDocxExtractor
{
    private readonly IAmazonS3 _s3Client;
    private readonly ILogger<DocxExtractor> _logger;

    /// <summary>
    /// Creates a new DocxExtractor instance
    /// </summary>
    /// <param name="s3Client">S3 client for downloading files</param>
    /// <param name="logger">Logger instance</param>
    public DocxExtractor(IAmazonS3 s3Client, ILogger<DocxExtractor> logger)
    {
        _s3Client = s3Client ?? throw new ArgumentNullException(nameof(s3Client));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Extracts text from a DOCX file in S3
    /// </summary>
    /// <param name="bucketName">S3 bucket name</param>
    /// <param name="objectKey">S3 object key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Extracted text content with preserved paragraph structure</returns>
    /// <exception cref="TextExtractionException">Thrown when extraction fails</exception>
    public async Task<string> ExtractTextFromDocxAsync(
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
            "Starting DOCX extraction. Bucket: {BucketName}, Key: {ObjectKey}",
            bucketName,
            objectKey);

        try
        {
            // Download file from S3 to MemoryStream
            _logger.LogInformation("Downloading DOCX file from S3. Key: {ObjectKey}", objectKey);
            using var memoryStream = new MemoryStream();
            
            var getObjectRequest = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = objectKey
            };

            using (var response = await _s3Client.GetObjectAsync(getObjectRequest, cancellationToken))
            {
                await response.ResponseStream.CopyToAsync(memoryStream, cancellationToken);
            }

            _logger.LogInformation(
                "DOCX file downloaded successfully. Key: {ObjectKey}, Size: {Size} bytes",
                objectKey,
                memoryStream.Length);

            // Reset stream position to beginning
            memoryStream.Position = 0;

            // Extract text from DOCX using OpenXml
            _logger.LogInformation("Parsing DOCX document structure. Key: {ObjectKey}", objectKey);
            var extractedText = ExtractTextFromDocxStream(memoryStream);
            
            var duration = DateTime.UtcNow - startTime;
            _logger.LogInformation(
                "DOCX extraction completed successfully. Key: {ObjectKey}, ExtractedTextLength: {TextLength} characters, Duration: {Duration}ms",
                objectKey,
                extractedText.Length,
                duration.TotalMilliseconds);
            
            return extractedText;
        }
        catch (AmazonS3Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;
            _logger.LogError(
                ex,
                "Failed to download DOCX file from S3. Key: {ObjectKey}, ErrorCode: {ErrorCode}, StatusCode: {StatusCode}, Duration: {Duration}ms",
                objectKey,
                ex.ErrorCode,
                ex.StatusCode,
                duration.TotalMilliseconds);
            
            throw new TextExtractionException(
                $"Failed to download DOCX file from S3: {ex.Message}", 
                ex);
        }
        catch (Exception ex) when (ex is not TextExtractionException)
        {
            var duration = DateTime.UtcNow - startTime;
            _logger.LogError(
                ex,
                "Failed to extract text from DOCX file. Key: {ObjectKey}, Duration: {Duration}ms",
                objectKey,
                duration.TotalMilliseconds);
            
            throw new TextExtractionException(
                $"Failed to extract text from DOCX file: {ex.Message}", 
                ex);
        }
    }

    /// <summary>
    /// Extracts text from a DOCX stream using OpenXml
    /// </summary>
    /// <param name="stream">Stream containing DOCX file data</param>
    /// <returns>Extracted text with preserved paragraph structure</returns>
    private string ExtractTextFromDocxStream(Stream stream)
    {
        try
        {
            using var wordDocument = WordprocessingDocument.Open(stream, false);
            
            if (wordDocument.MainDocumentPart == null)
            {
                _logger.LogError("DOCX file has no main document part");
                throw new TextExtractionException("DOCX file has no main document part");
            }

            var body = wordDocument.MainDocumentPart.Document.Body;
            if (body == null)
            {
                _logger.LogError("DOCX file has no body content");
                throw new TextExtractionException("DOCX file has no body content");
            }

            // Extract all paragraph text and preserve structure with line breaks
            var paragraphs = body.Descendants<Paragraph>();
            var textParts = new List<string>();

            foreach (var paragraph in paragraphs)
            {
                var paragraphText = GetParagraphText(paragraph);
                if (!string.IsNullOrEmpty(paragraphText))
                {
                    textParts.Add(paragraphText);
                }
            }

            _logger.LogInformation(
                "Extracted {ParagraphCount} paragraphs from DOCX document",
                textParts.Count);

            // Join paragraphs with line breaks to preserve structure
            return string.Join("\n", textParts);
        }
        catch (Exception ex) when (ex is not TextExtractionException)
        {
            _logger.LogError(
                ex,
                "Failed to parse DOCX document structure: {ErrorMessage}",
                ex.Message);
            
            throw new TextExtractionException(
                $"Failed to parse DOCX document structure: {ex.Message}", 
                ex);
        }
    }

    /// <summary>
    /// Extracts text from a single paragraph, including all text runs
    /// </summary>
    /// <param name="paragraph">The paragraph to extract text from</param>
    /// <returns>Concatenated text from all text runs in the paragraph</returns>
    private string GetParagraphText(Paragraph paragraph)
    {
        var textRuns = paragraph.Descendants<Text>();
        return string.Concat(textRuns.Select(t => t.Text));
    }
}
