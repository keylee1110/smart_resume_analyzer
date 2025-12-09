namespace TheParser.Interfaces;

/// <summary>
/// Service for extracting text from documents using OCR
/// </summary>
public interface IOcrService
{
    /// <summary>
    /// Extracts text from a PDF document in S3 using AWS Textract
    /// </summary>
    /// <param name="bucketName">S3 bucket containing the document</param>
    /// <param name="objectKey">S3 object key of the document</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Extracted text content</returns>
    Task<string> ExtractTextFromPdfAsync(
        string bucketName, 
        string objectKey, 
        CancellationToken cancellationToken = default);
}
